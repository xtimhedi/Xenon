using System;
using System.IO;
using System.Runtime.InteropServices;
using Xenon.Audio;

namespace Xenon.Assets
{
    public class AudioClipLoader : IAssetLoader
    {
        public object Load(string filePath)
        {
            using var fs = File.OpenRead(filePath);
            return Load(fs);
        }

        public unsafe object Load(Stream stream)
        {
            using MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] fileBytes = ms.ToArray();

            fixed (byte* ptr = fileBytes)
            {
                IntPtr rwOps = Sdl2Audio.SDL_RWFromMem((IntPtr)ptr, fileBytes.Length);
                if (rwOps == IntPtr.Zero)
                    throw new Exception("Failed to create SDL_RWops.");

                Sdl2Audio.SDL_AudioSpec wavSpec = new Sdl2Audio.SDL_AudioSpec();

                // Freesrc = 1 guarantees memory handles itself 
                IntPtr result = Sdl2Audio.SDL_LoadWAV_RW(rwOps, 1, ref wavSpec, out IntPtr audioBuf, out uint audioLen);
                if (result == IntPtr.Zero)
                    throw new Exception("Failed to load WAV file.");

                try
                {
                    var deviceSpec = AudioSystem.DeviceSpec;

                    // Standardize audio formats behind the scenes if WAV differs from Device
                    if (wavSpec.format != deviceSpec.format || wavSpec.channels != deviceSpec.channels || wavSpec.freq != deviceSpec.freq)
                    {
                        IntPtr audioStream = Sdl2Audio.SDL_NewAudioStream(
                            wavSpec.format, wavSpec.channels, wavSpec.freq,
                            deviceSpec.format, deviceSpec.channels, deviceSpec.freq);

                        try
                        {
                            Sdl2Audio.SDL_AudioStreamPut(audioStream, audioBuf, (int)audioLen);
                            Sdl2Audio.SDL_AudioStreamFlush(audioStream);

                            int convertedLen = Sdl2Audio.SDL_AudioStreamAvailable(audioStream);
                            byte[] convertedData = new byte[convertedLen];

                            fixed (byte* outPtr = convertedData)
                            {
                                Sdl2Audio.SDL_AudioStreamGet(audioStream, (IntPtr)outPtr, convertedLen);
                            }
                            return new AudioClip(convertedData);
                        }
                        finally
                        {
                            Sdl2Audio.SDL_FreeAudioStream(audioStream);
                        }
                    }
                    else
                    {
                        byte[] exactData = new byte[audioLen];
                        Marshal.Copy(audioBuf, exactData, 0, (int)audioLen);
                        return new AudioClip(exactData);
                    }
                }
                finally
                {
                    Sdl2Audio.SDL_FreeWAV(audioBuf);
                }
            }
        }
    }
}