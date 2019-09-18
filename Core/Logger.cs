using System;
using System.IO;

namespace Reimu.Core
{
    public static class Logger
    {
        private static readonly object Lock = new object();
        private static string _filePath;

        public static void Log(string source, string message, ConsoleColor color)
        {
            lock (Lock)
            {
                if (!Program.Headless)
                {
                    Console.WriteLine();
                    var date = DateTime.Now.ToString("HH:mm");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"-> {date} ");
                    Console.ForegroundColor = color;
                    Console.Write($"[{source}] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(message);
                    Console.ResetColor();
                }
                
                FileLog($"[{DateTime.UtcNow:yyyy/MM/dd - HH:mm}] [{source}] {message}");
            }
        }

        public static void Initialize()
        {
            if (!Program.Headless)
            {
                string[] header =
                {
                    @"",
                    @"██████╗ ███████╗██╗███╗   ███╗██╗   ██╗",
                    @"██╔══██╗██╔════╝██║████╗ ████║██║   ██║",
                    @"██████╔╝█████╗  ██║██╔████╔██║██║   ██║",
                    @"██╔══██╗██╔══╝  ██║██║╚██╔╝██║██║   ██║",
                    @"██║  ██║███████╗██║██║ ╚═╝ ██║╚██████╔╝",
                    @"╚═╝  ╚═╝╚══════╝╚═╝╚═╝     ╚═╝ ╚═════╝ ",
                    @""
                };

                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var line in header)
                    Console.WriteLine(line);
                
                Console.ResetColor();
                // TODO: Keep version num in a better place
                Console.WriteLine("Version: 2019-PreAlpha-09-17");
            }

            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "log.txt");
            FileLog($"\n\n=================================[ {DateTime.UtcNow:yyyy/MM/dd HH:mm:ss} ]=================================\n\n");
        }

        private static void FileLog(string message)
        {
            using (var writer = File.AppendText(_filePath))
                writer.WriteLine(message);
        }
    }
}