using System;

namespace Xenon.Audio
{
    public class AudioClip : IDisposable
    {
        public byte[] Data { get; private set; }

        public AudioClip(byte[] data)
        {
            Data = data;
        }

        public void Dispose()
        {
            Data = null;
        }
    }

    public class AudioPoolChannel
    {
        public AudioClip Clip { get; internal set; }
        public int Position { get; internal set; }
        public int Volume { get; set; } = 128; // 0 to 128 (SDL_MIX_MAXVOLUME)
        public bool Loop { get; set; }
        public bool IsActive { get; internal set; }

        public void Stop() => IsActive = false;
    }
}