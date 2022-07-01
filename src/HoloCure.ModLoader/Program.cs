using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using HoloCure.ModLoader.API;
using HoloCure.ModLoader.Updater;
using Spectre.Console;
using UndertaleModLib;

namespace HoloCure.ModLoader
{
    internal static class Program
    {
        private static readonly IProgramUpdatable Updater = new HoloCureUpdater();

        public static async Task Main() {
            try {
                string version = typeof(Program).Assembly.GetName().Version!.ToString();
                AnsiConsole.MarkupLine("[gray]HoloCure.ModLoader v" + version + "[/]");

                await CheckForUpdates(version);
                VerifyDataExists();
                RestoreBackupData();
                BackupGameData();

                AnsiConsole.MarkupLine("[gray]Loading game data... [silver](this may take a while)[/][/]");
                UndertaleData data = LoadGameData();
                AnsiConsole.MarkupLine("[gray]Loaded game data! Got game: [silver]" + data.GeneralInfo.Name + "[/][/]");

                Loader loader = new(data.GeneralInfo.Name.Content);
                loader.PatchGame(data);

                WriteGameData(data);
                await ExecuteGame();
            }
            catch (Exception e) {
                AnsiConsole.MarkupLine(
                    "\n\n[red]A [bold]fatal[/] error was caught and the process must be aborted."
                    + "\nBelow is the stacktrace, which you should share when requesting help.[/]\n\n"
                );
                AnsiConsole.WriteException(e);
            }
        }

        private static async Task CheckForUpdates(string version) {
            AnsiConsole.MarkupLine("\n[gray]Checking for updates...[/]\n");

            bool available = Updater.CanUpdate() && await Updater.CheckUpdate(version);

            AnsiConsole.MarkupLine(available
                                       ? "[white]Download the latest release here: [blue underline]https://github.com/steviegt6/holocure-modloader/releases[/][/]"
                                       : "[gray]No updates available.[/]\n");
            
            AnsiConsole.MarkupLine("\n\n[white]Wait 5 seconds or press any key to skip updating.[/]\n");
            await new ConsoleStrangler().Strangle(5000);
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
                AnsiConsole.MarkupLine("[gray]No \"backup.win\" file found, assuming first launch and skipping restore.[/]");
                return;
            }

            AnsiConsole.MarkupLine("[gray]Restoring backup data...[/]");
            File.Copy("backup.win", "data.win", true);
            AnsiConsole.MarkupLine("[gray]Restored backup data.[/]");
        }

        // TODO: Necessary every time? Maybe so to ensure users updating their HoloCure game don't have outdated backups (reading comprehension issue).
        private static void BackupGameData() {
            AnsiConsole.MarkupLine("[gray]Backing up game data...[/]");
            File.Copy("data.win", "backup.win", true);
            AnsiConsole.MarkupLine("[gray]Game data backed up to \"backup.win\".[/]");
        }

        private static UndertaleData LoadGameData() {
            using FileStream stream = new("data.win", FileMode.Open, FileAccess.Read);
            return UndertaleIO.Read(stream);
        }

        private static void WriteGameData(UndertaleData data) {
            AnsiConsole.MarkupLine("[gray]Writing game data to \"data.win\" for execution.[/]");
            using FileStream stream = new("data.win", FileMode.Create, FileAccess.Write);
            UndertaleIO.Write(stream, data);
            AnsiConsole.MarkupLine("[gray]Game data written to \"data.win\".[/]");
        }

        private static async Task ExecuteGame() {
            AnsiConsole.MarkupLine("[gray]Executing game...[/]\n\n");
            ProcessStartInfo info = new(Path.GetFullPath("HoloCure.exe"))
            {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };
            Process? proc = Process.Start(info);
            if (proc is null) throw new Exception("Started process was null.");
            await proc.WaitForExitAsync();
        }
    }
}