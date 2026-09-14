using System;
using System.Runtime.InteropServices;

namespace Xenon.ContentSystem
{
    public static class WindowsAnsiInitializer
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        /// <summary>
        /// Explicitly requests Windows to parse ANSI styling flags for the native executable process.
        /// </summary>
        public static void EnsureEnabled()
        {
            // Only execute if the platform is Windows (macOS and Linux support ANSI sequences out of the box)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);

                if (GetConsoleMode(stdOutHandle, out uint consoleMode))
                {
                    consoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                    SetConsoleMode(stdOutHandle, consoleMode);
                }
            }
        }
    }
}
