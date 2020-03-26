using System;
using System.Threading.Tasks;

namespace Reimu
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            // TODO: Here would be a command line program to manage the bot
            // Mainly so it can be shutdown safely (close/dispose) all services.
            // IDisposables may not be disposed on exit
            await Task.Delay(-1);
        }
    }
}