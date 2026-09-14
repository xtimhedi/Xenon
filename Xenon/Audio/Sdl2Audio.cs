using System;
using System.Runtime.InteropServices;

namespace Xenon.Audio
{
    public static class Sdl2Audio
    {
        private const string LibName = "SDL2";

        public const int SDL_INIT_AUDIO = 0x00000010;
        public const int SDL_MIX_MAXVOLUME = 128;

        public const ushort AUDIO_S16LSB = 0x8010;
        public const ushort AUDIO_S16MSB = 0x9010;
        public static readonly ushort AUDIO_S16SYS = BitConverter.IsLittleEndian ? AUDIO_S16LSB : AUDIO_S16MSB;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SDL_AudioCallback(IntPtr userdata, IntPtr stream, int len);

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_AudioSpec
        {
            public int freq;
            public ushort format;
            public byte channels;
            public byte silence;
            public ushort samples;
            public uint size;
            public IntPtr callback;
            public IntPtr userdata;
        }

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_InitSubSystem(uint flags);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_QuitSubSystem(uint flags);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SDL_OpenAudioDevice(
            IntPtr device, int iscapture, ref SDL_AudioSpec desired, out SDL_AudioSpec obtained, int allowed_changes);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_CloseAudioDevice(uint dev);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_PauseAudioDevice(uint dev, int pause_on);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_LockAudioDevice(uint dev);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_UnlockAudioDevice(uint dev);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_RWFromMem(IntPtr mem, int size);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_LoadWAV_RW(IntPtr src, int freesrc, ref SDL_AudioSpec spec, out IntPtr audio_buf, out uint audio_len);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_FreeWAV(IntPtr audio_buf);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_NewAudioStream(
            ushort src_format, byte src_channels, int src_rate,
            ushort dst_format, byte dst_channels, int dst_rate);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_AudioStreamPut(IntPtr stream, IntPtr buf, int len);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_AudioStreamFlush(IntPtr stream);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_AudioStreamAvailable(IntPtr stream);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_AudioStreamGet(IntPtr stream, IntPtr buf, int len);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_FreeAudioStream(IntPtr stream);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_MixAudioFormat(IntPtr dst, IntPtr src, ushort format, uint len, int volume);
    }
}