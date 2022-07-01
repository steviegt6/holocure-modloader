using System;
using System.Threading.Tasks;
using HoloCure.ModLoader.Updater;

namespace HoloCure.ModLoader
{
    internal static class Program
    {
        private static readonly IProgramUpdatable Updater = new HoloCureUpdater();
        
        public static async Task Main(string[] args) {
            Console.WriteLine("TEST: Checking for updates...");
            bool available = Updater.CanUpdate() && await Updater.CheckUpdate(typeof(Program).Assembly.GetName().Version!.ToString());
            if (!available) {
                Console.WriteLine("Your version of the program is up to date.");
            }
        }
    }
}