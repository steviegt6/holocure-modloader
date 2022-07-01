using System;
using System.IO;
using System.Threading.Tasks;
using HoloCure.ModLoader.API;
using HoloCure.ModLoader.Updater;
using UndertaleModLib;

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
            
            VerifyDataExists();
            RestoreBackupData();
            // TODO: Necessary every time? Maybe so to ensure users updating their HoloCrush game don't have outdated backups (reading comprehension issue).
            BackupGameData();
            
            Console.WriteLine("Loading game data...");
            UndertaleData data = LoadGameData();
            Console.WriteLine("Loaded game data, got game: " + data.GeneralInfo.Name);
            Loader loader = new(data.GeneralInfo.Name.Content);
        }

        private static void VerifyDataExists() {
            // TODO: Add support for other file formats as the game releases to more systems.
            if (!File.Exists("data.win")) {
                throw new FileNotFoundException(
                    "Could not find game data file \"data.win\" in the current working directory."
                    + "\nEnsure you launched the mod loader in the same folder as the game."
                );
            }
        }

        private static void RestoreBackupData() {
            if (!File.Exists("backup.win")) {
                Console.WriteLine("No \"backup.win\" file found, assuming first launch and skipping restore.");
                return;
            }

            Console.WriteLine("Restoring backup data...");
            File.Copy("backup.win", "data.win", true);
            Console.WriteLine("Restored backup data.");
        }

        private static void BackupGameData() {
            Console.WriteLine("Backing up game data...");
            File.Copy("data.win", "backup.win", true);
            Console.WriteLine("Game data backed up to \"backup.win\".");
        }

        private static UndertaleData LoadGameData() {
            return UndertaleIO.Read(new FileStream("data.win", FileMode.Open, FileAccess.Read));
        }
    }
}