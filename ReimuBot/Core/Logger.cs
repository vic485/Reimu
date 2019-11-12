using System;
using System.IO;

namespace Reimu.Core
{
    public static class Logger
    {
        private static readonly string FilePath;
        private static readonly object Lock = new object();

        // static constructor
        static Logger()
        {
            FilePath = Path.Combine(Directory.GetCurrentDirectory(), "log.txt");

            lock (Lock)
            {
                string[] header =
                {
                    @"  _____      _                 ",
                    @" |  __ \    (_)                ",
                    @" | |__) |___ _ _ __ ___  _   _ ",
                    @" |  _  // _ \ | '_ ` _ \| | | |",
                    @" | | \ \  __/ | | | | | | |_| |",
                    @" |_|  \_\___|_|_| |_| |_|\__,_|",
                    ""
                };

                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var line in header)
                    Console.WriteLine(line);
                
                Console.ResetColor();
                Console.WriteLine("Version: 2019/11/12-Dev");
                WriteToFile(
                    $"\n\n=================================[ {DateTime.UtcNow:yyyy/MM/dd HH:mm:ss} ]=================================\n\n");
            }
        }

        /// <summary>
        /// Writes a message to the log
        /// </summary>
        /// <param name="source">Source of the message</param>
        /// <param name="message">Information to log</param>
        /// <param name="color">Color of <see cref="source"/> in the console</param>
        public static void Log(string source, string message, ConsoleColor color)
        {
            lock (Lock)
            {
                Console.WriteLine();
                // Use local time for console/journalctf logging
                var time = DateTime.Now.ToString(" HH:mm");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(time);
                Console.ForegroundColor = color;
                Console.Write($" [{source}] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(message);
                Console.ResetColor();

                WriteToFile($"[{DateTime.UtcNow:yyyy/MM/dd - HH:mm:ss}] [{source}] {message}");
            }
        }

        /// <summary>
        /// Sends a message to the locally stored log.
        /// Timestamps to this should be UTC 
        /// </summary>
        /// <param name="message"></param>
        private static void WriteToFile(string message)
        {
            using var writer = File.AppendText(FilePath);
            writer.WriteLine(message);
        }
    }
}