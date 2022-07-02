using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using HoloCure.ModLoader.API;
using HoloCure.ModLoader.Runners;
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
                IRunner runner = GetPlatformDependantRunner();

                VerifyDataExists(runner);
                RestoreBackupData(runner);
                BackupGameData(runner);

                AnsiConsole.MarkupLine("[gray]Loading game data... [silver](this may take a while)[/][/]");
                UndertaleData data = LoadGameData(runner);
                AnsiConsole.MarkupLine("[gray]Loaded game data! Got game: [silver]" + data.GeneralInfo.Name + "[/][/]");

                Loader loader = new(data.GeneralInfo.Name.Content);
                loader.PatchGame(data);

                WriteGameData(data, runner);
                await ExecuteGame(runner);
            }
            catch (Exception e) {
                AnsiConsole.MarkupLine(
                    "\n\n[red]A [bold]fatal[/] error was caught and the process must be aborted."
                    + "\nBelow is the stacktrace, which you should share when requesting help.[/]\n\n"
                );
                AnsiConsole.WriteException(e);
            }
        }

        private static IRunner GetPlatformDependantRunner() {
            if (OperatingSystem.IsWindows()) return new WindowsRunner();
            throw new PlatformNotSupportedException(
                "Currently, only Windows operating systems are supported."
                + "\nCompatibility with other operating systems through Wine, BootCamp, or similar means may be possible."
                + "\nIf you would like to investigate this, write up an issue on https://github.com/steviegt6/holocure-modloader/issues."
            );
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

        private static void VerifyDataExists(IRunner runner) {
            RunnerReturnCtx<bool> ctx = runner.DataExists();
            if (!ctx.Value) {
                throw new FileNotFoundException("Could not find game data file, expected at: " + ctx.FileName);
            }
        }

        private static void RestoreBackupData(IRunner runner) {
            AnsiConsole.MarkupLine("[gray]Restoring backup data...[/]");
            RunnerReturnCtx<RestoreBackupDataResult> ctx = runner.RestoreBackupData();

            switch (ctx.Value) {
                case RestoreBackupDataResult.MissingBackupFile:
                    throw new FileNotFoundException("Could not find backup data file, expected at: " + ctx.BackupName);

                case RestoreBackupDataResult.MissingDataFile:
                    throw new FileNotFoundException("Could not find game data file, expected at: " + ctx.FileName);

                case RestoreBackupDataResult.PermissionError:
                    throw new UnauthorizedAccessException("Cannot overwrite game data at: " + ctx.FileName);

                case RestoreBackupDataResult.Skipped:
                    AnsiConsole.MarkupLine("[gray]Backup file not found, assuming first launch and skipping restoration.[/]");
                    break;

                case RestoreBackupDataResult.Success:
                    AnsiConsole.MarkupLine("[gray]Restored game data to saved backup.[/]");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // TODO: Necessary every time? Maybe so to ensure users updating their HoloCure game don't have outdated backups (reading comprehension issue).
        private static void BackupGameData(IRunner runner) {
            AnsiConsole.MarkupLine("[gray]Backing up game data...[/]");
            RunnerReturnCtx<BackupDataResult> ctx = runner.BackupData();

            switch (ctx.Value) {
                case BackupDataResult.MissingFile:
                    throw new FileNotFoundException("Cannot backup game data file as game data file does not exist, expected at: " + ctx.FileName);

                case BackupDataResult.PermissionError:
                    throw new UnauthorizedAccessException($"Could not backup game data, permission denied to write to file \"{ctx.BackupName}\".");

                case BackupDataResult.Skipped:
                    AnsiConsole.MarkupLine($"[gray]Skipped backing up to \"{ctx.BackupName}\".[/]");
                    break;

                case BackupDataResult.Success:
                    AnsiConsole.MarkupLine($"[gray]Game data backed up to \"{ctx.BackupName}\".[/]");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static UndertaleData LoadGameData(IRunner runner) {
            RunnerReturnCtx<(LoadGameDataResult result, UndertaleData? data)> ctx = runner.LoadGameData();

            switch (ctx.Value.result) {
                case LoadGameDataResult.MissingFile:
                    throw new FileNotFoundException("Could not find game data file, expected at: " + ctx.FileName);

                case LoadGameDataResult.PermissionError:
                    throw new UnauthorizedAccessException("Cannot read game data file, permission denied at: " + ctx.FileName);

                case LoadGameDataResult.Success:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Assuming not null because we should be throwing if the object is null.
            // The object should only be null if the returned result isn't successful.
            // We can always just add a check, but undefined behavior is awesome.
            // Didn't your parents ever tell you that?
            return ctx.Value.data!;
        }

        private static void WriteGameData(UndertaleData data, IRunner runner) {
            AnsiConsole.MarkupLine("[gray]Writing game data to file for execution.[/]");
            RunnerReturnCtx<WriteGameDataResult> ctx = runner.WriteGameData(data);

            switch (ctx.Value) {
                case WriteGameDataResult.PermissionError:
                    throw new UnauthorizedAccessException("Cannot write game data file, permission denied at: " + ctx.FileName);

                case WriteGameDataResult.Success:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            AnsiConsole.MarkupLine($"[gray]Game data written to \"{ctx.FileName}\".[/]");
        }

        private static async Task ExecuteGame(IRunner runner) {
            AnsiConsole.MarkupLine("[gray]Executing game...[/]\n\n");
            RunnerReturnCtx<(ExecuteGameResult result, Process? proc)> ctx = runner.ExecuteGame();

            switch (ctx.Value.result) {
                case ExecuteGameResult.ProcessNull:
                    throw new NullReferenceException("The started process was somehow null.");

                case ExecuteGameResult.RunnerMissing:
                    throw new FileNotFoundException("The runner file is missing, expected at: " + ctx.RunnerName);

                case ExecuteGameResult.RunnerNull:
                    throw new NullReferenceException("The runner file is null. Somehow, it was never auto-initialized.");

                case ExecuteGameResult.Success:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            await ctx.Value.proc!.WaitForExitAsync();
        }
    }
}