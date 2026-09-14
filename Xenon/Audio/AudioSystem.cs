using System;
using System.Runtime.InteropServices;

namespace Xenon.Audio
{
    public static class AudioSystem
    {
        private static uint _deviceId;
        private static Sdl2Audio.SDL_AudioSpec _deviceSpec;
        private static Sdl2Audio.SDL_AudioCallback _callbackDelegate;

        // Defines concurrent overlay size
        private const int MAX_CHANNELS = 32;
        private static AudioPoolChannel[] _channels;

        public static bool IsInitialized { get; private set; }
        public static Sdl2Audio.SDL_AudioSpec DeviceSpec => _deviceSpec;

        public static void Initialize()
        {
            if (IsInitialized) return;

            if (Sdl2Audio.SDL_InitSubSystem(Sdl2Audio.SDL_INIT_AUDIO) != 0)
            {
                throw new Exception("Failed to initialize SDL Audio System");
            }

            // Pin delegate in memory so GC doesn't destroy the native callback hook
            _callbackDelegate = new Sdl2Audio.SDL_AudioCallback(AudioCallback);

            Sdl2Audio.SDL_AudioSpec desired = new Sdl2Audio.SDL_AudioSpec
            {
                freq = 44100,
                format = Sdl2Audio.AUDIO_S16SYS,
                channels = 2,
                samples = 2048,
                callback = Marshal.GetFunctionPointerForDelegate(_callbackDelegate),
                userdata = IntPtr.Zero
            };

            // Passing 0 at the end guarantees we receive the exact requested format
            _deviceId = Sdl2Audio.SDL_OpenAudioDevice(IntPtr.Zero, 0, ref desired, out _deviceSpec, 0);

            _channels = new AudioPoolChannel[MAX_CHANNELS];
            for (int i = 0; i < MAX_CHANNELS; i++)
                _channels[i] = new AudioPoolChannel();

            // Unpause SDL stream
            Sdl2Audio.SDL_PauseAudioDevice(_deviceId, 0);
            IsInitialized = true;
        }

        public static AudioPoolChannel Play(AudioClip clip, float volume = 1f, bool loop = false)
        {
            if (!IsInitialized || clip == null || clip.Data == null) return null;

            int volInt = (int)(Math.Clamp(volume, 0f, 1f) * Sdl2Audio.SDL_MIX_MAXVOLUME);

            // Locks the audio processing background thread safely
            Sdl2Audio.SDL_LockAudioDevice(_deviceId);
            try
            {
                for (int i = 0; i < MAX_CHANNELS; i++)
                {
                    if (!_channels[i].IsActive)
                    {
                        _channels[i].Clip = clip;
                        _channels[i].Position = 0;
                        _channels[i].Volume = volInt;
                        _channels[i].Loop = loop;
                        _channels[i].IsActive = true;
                        return _channels[i];
                    }
                }
            }
            finally
            {
                Sdl2Audio.SDL_UnlockAudioDevice(_deviceId);
            }
            return null;
        }

        public static void Shutdown()
        {
            if (!IsInitialized) return;
            Sdl2Audio.SDL_CloseAudioDevice(_deviceId);
            Sdl2Audio.SDL_QuitSubSystem(Sdl2Audio.SDL_INIT_AUDIO);
            IsInitialized = false;
        }

        private static unsafe void AudioCallback(IntPtr userdata, IntPtr stream, int len)
        {
            // Wipe the destination buffer to silence each tick
            new Span<byte>(stream.ToPointer(), len).Fill(_deviceSpec.silence);

            for (int i = 0; i < MAX_CHANNELS; i++)
            {
                var channel = _channels[i];
                if (!channel.IsActive) continue;

                var data = channel.Clip?.Data;
                if (data == null)
                {
                    channel.IsActive = false;
                    continue;
                }

                int targetOffset = 0;
                while (targetOffset < len && channel.IsActive)
                {
                    int bytesLeft = data.Length - channel.Position;
                    int bytesToMix = Math.Min(bytesLeft, len - targetOffset);

                    fixed (byte* ptr = &data[channel.Position])
                    {
                        Sdl2Audio.SDL_MixAudioFormat(
                            IntPtr.Add(stream, targetOffset),
                            (IntPtr)ptr,
                            _deviceSpec.format,
                            (uint)bytesToMix,
                            channel.Volume);
                    }

                    channel.Position += bytesToMix;
                    targetOffset += bytesToMix;

                    if (channel.Position >= data.Length)
                    {
                        if (channel.Loop)
                            channel.Position = 0;
                        else
                            channel.IsActive = false;
                    }
                }
            }
        }
    }
}