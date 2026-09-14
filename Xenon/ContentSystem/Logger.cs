using AnsiFormatting;
using System;
using System.Collections.Generic;
using System.IO; // Added for Directory, File, and Path
using System.Text.RegularExpressions;

namespace Xenon.ContentSystem
{
    public class Logger
    {
        // Internal storage for pure, clean text (no ANSI clutter)
        public List<string> LogFile { get; } = new List<string>();

        // Matches standard ANSI escape sequences to strip them for file writing
        private static readonly Regex AnsiRegex = new Regex(@"\x1B\[[0-9;]*[a-zA-Z]", RegexOptions.Compiled);

        private readonly string _logFilePath;
        private readonly object _lockObj = new object();

        public Logger()
        {
            // 1. Define the Logs directory relative to the application executable
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            // 2. Create the directory if it doesn't exist
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // 3. Create a unique filename for this session based on the current time
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _logFilePath = Path.Combine(logDirectory, $"Log_{timestamp}.txt");
        }

        private void WriteLog(string plainText, string coloredText)
        {
            // Write to console immediately
            Console.WriteLine(coloredText);

            // Lock ensures thread-safety if multiple threads log simultaneously
            lock (_lockObj)
            {
                LogFile.Add(plainText);

                try
                {
                    // Automatically append the new log line to the file in the Logs folder
                    File.AppendAllText(_logFilePath, plainText + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Logger Internal Error] Failed to auto-write to log file: {ex.Message}".ToBold(Color.Red));
                }
            }
        }

        public void Log(string text, string sender = "Log")
        {
            string plain = $"[{sender}] {text}";
            string colored = $"{"_s_".Replace("_s_", $"[{sender}]").ToBold(Color.Blue)} {text.ToRegular(Color.Green)}";

            WriteLog(plain, colored);
        }

        public void LogError(string text, string sender = "Log")
        {
            string plain = $"[{sender}] Error: {text}";
            string colored = $"{"_s_".Replace("_s_", $"[{sender}]").ToBold(Color.Blue)} {"Error:".ToBold(Color.Red)} {text.ToBold(Color.Red)}";

            WriteLog(plain, colored);
        }

        public void LogWarn(string text, string sender = "Log")
        {
            string plain = $"[{sender}] Warn: {text}";
            string colored = $"{"_s_".Replace("_s_", $"[{sender}]").ToBold(Color.Blue)} {"Warn:".ToBold(Color.Yellow)} {text.ToRegular(Color.Yellow)}";

            WriteLog(plain, colored);
        }

        /// <summary>
        /// Saves the pure plaintext logs to a specific custom file path.
        /// Note: The logger already auto-saves to the Logs folder.
        /// </summary>
        public void SaveToFile(string filePath)
        {
            try
            {
                lock (_lockObj)
                {
                    File.WriteAllLines(filePath, LogFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logger Internal Error] Failed to save log file: {ex.Message}".ToBold(Color.Red));
            }
        }

        public void Flush()
        {
            lock (_lockObj)
            {
                LogFile.Clear();
            }
            Console.Clear();
        }
    }
}