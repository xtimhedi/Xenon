using System;
using System.IO;
using NLayer;
using Xenon.Audio;

namespace Xenon.Assets
{
    public class Mp3ClipLoader : IAssetLoader
    {
        public object Load(string filePath)
        {
            using var fs = File.OpenRead(filePath);
            return Load(fs);
        }

        public unsafe object Load(Stream stream)
        {
            // NLayer will decode the MP3 stream
            using var mpegFile = new MpegFile(stream);

            // NLayer outputs standard IEEE 32-bit float PCM. 
            // We need to convert it to 16-bit PCM for our SDL AudioSystem.
            float[] floatSamples = new float[mpegFile.Length];
            int samplesRead = mpegFile.ReadSamples(floatSamples, 0, floatSamples.Length);

            // 1 float = 1 short (2 bytes)
            byte[] pcm16Data = new byte[samplesRead * 2];

            // Convert floats (-1.0 to 1.0) to 16-bit shorts (-32768 to 32767)
            for (int i = 0; i < samplesRead; i++)
            {
                // Clamp just in case the MP3 goes over 0dB
                float clamped = Math.Clamp(floatSamples[i], -1f, 1f);
                short shortSample = (short)(clamped * short.MaxValue);

                // Write the short into the byte array (Little Endian by default on x86/ARM)
                pcm16Data[i * 2] = (byte)(shortSample & 0xFF);
                pcm16Data[i * 2 + 1] = (byte)((shortSample >> 8) & 0xFF);
            }

            // At this point, we have raw 16-bit PCM audio.
            // Now we check if the MP3's sample rate/channels match our hardware output.
            var deviceSpec = AudioSystem.DeviceSpec;

            if (mpegFile.SampleRate != deviceSpec.freq || mpegFile.Channels != deviceSpec.channels)
            {
                // Use SDL to resample the raw PCM we just generated
                IntPtr audioStream = Sdl2Audio.SDL_NewAudioStream(
                    Sdl2Audio.AUDIO_S16SYS, (byte)mpegFile.Channels, mpegFile.SampleRate,
                    deviceSpec.format, deviceSpec.channels, deviceSpec.freq);

                try
                {
                    fixed (byte* ptr = pcm16Data)
                    {
                        Sdl2Audio.SDL_AudioStreamPut(audioStream, (IntPtr)ptr, pcm16Data.Length);
                        Sdl2Audio.SDL_AudioStreamFlush(audioStream);

                        int convertedLen = Sdl2Audio.SDL_AudioStreamAvailable(audioStream);
                        byte[] convertedData = new byte[convertedLen];

                        fixed (byte* outPtr = convertedData)
                        {
                            Sdl2Audio.SDL_AudioStreamGet(audioStream, (IntPtr)outPtr, convertedLen);
                        }
                        return new AudioClip(convertedData);
                    }
                }
                finally
                {
                    Sdl2Audio.SDL_FreeAudioStream(audioStream);
                }
            }

            // If the sample rate already matches perfectly, just return it
            return new AudioClip(pcm16Data);
        }
    }
}