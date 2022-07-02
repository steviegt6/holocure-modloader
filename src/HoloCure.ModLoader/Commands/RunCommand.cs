using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CliFx.Attributes;
using HoloCure.ModLoader.API;
using HoloCure.ModLoader.Config;
using HoloCure.ModLoader.Runners;
using HoloCure.ModLoader.Utils;
using Spectre.Console;
using UndertaleModLib;

namespace HoloCure.ModLoader.Commands
{
    [Command("run", Description = "Runs a game using the mod loader.")]
    public class RunCommand : BaseCommand
    {
        [CommandOption("game", 'g', Description = "The game being ran. This is used for reading configs. If not set, config-set paths will not be used.")]
        public string? Game { get; set; }

        [CommandOption("no-default-profile", 'n', Description = "Disables automatically setting --game if it's null.")]
        public bool NoDefaultProfile { get; set; } = false;

        [CommandOption("game-path", 'p', Description = "Manually sets the game path. If not set but --game is set, reads from the config. If not specified in the config, assumes CWD.")]
        public string? GamePath { get; set; }

        [CommandOption("backup-path", 'b', Description = "Manually sets the backup path. If not set but --game is set, reads from the config. If not specified in the config, assumes CWD.")]
        public string? BackupPath { get; set; }

        [CommandOption("runner-path", 'r', Description = "Manually sets the runner path. If not set but --game is set, reads from the config. If not specified in the config, assumes CWD.")]
        public string? RunnerPath { get; set; }

        [CommandOption("no-args", Description = "Indicates this program was launched with no arguments. Do not set manually.")]
        public bool NoArgs { get; set; }

        protected override async ValueTask ExecuteAsync() {
            if (NoArgs && !Debugger.IsAttached) {
                // TODO: This message sucks.
                AnsiConsole.MarkupLine("[red]The program was launched with no arguments. Consider launching this from the command line while supplying arguments appropriately.[/]");
            }
            
            // Get config.
            LaunchConfig cfg = LaunchConfig.Instance;
            
            // Populate Game if applicable.
            Game ??= NoDefaultProfile ? null : cfg.DefaultProfile;
            
            // Get launch profile if present.
            LaunchProfile? profile = cfg.Profiles.ContainsKey(Game ?? "") ? cfg.Profiles[Game ?? ""] : null;
            
            // Get config-adjusted paths.
            GamePath = Utilities.FirstNotNull(GamePath, profile?.GamePath);
            BackupPath = Utilities.FirstNotNull(BackupPath, profile?.BackupPath);
            RunnerPath = Utilities.FirstNotNull(RunnerPath, profile?.RunnerPath);
            
            // Use these paths in the runner.
            IRunner runner = GetPlatformDependantRunner(GamePath, BackupPath, RunnerPath);
            
            VerifyDataExists(runner);
            RestoreBackupData(runner);
            BackupGameData(runner);

            AnsiConsole.MarkupLine("[gray]Loading game data... [silver](this may take a while)[/][/]");
            UndertaleData data = LoadGameData(runner);
            // TODO: Throw when --game doesn't match? Rather meh. Maybe a config option, heh.
            AnsiConsole.MarkupLine("[gray]Loaded game data! Got game: [silver]" + data.GeneralInfo.Name + "[/][/]");

            Loader loader = new(data.GeneralInfo.Name.Content);
            loader.PatchGame(data);

            WriteGameData(data, runner);
            await ExecuteGame(runner);
        }
        
        // TODO: Config option to override the runner used? IDK.
        private static IRunner GetPlatformDependantRunner(string? gamePath = null, string? backupPath = null, string? runnerPath = null) {
            if (OperatingSystem.IsWindows()) return new WindowsRunner(gamePath, backupPath, runnerPath);
            throw new PlatformNotSupportedException(
                "Currently, only Windows operating systems are supported."
                + "\nCompatibility with other operating systems through Wine, BootCamp, or similar means may be possible."
                + "\nIf you would like to investigate this, write up an issue on https://github.com/steviegt6/holocure-modloader/issues."
            );
        }

        #region IRunner Wrapping

        private void VerifyDataExists(IRunner runner) {
            RunnerReturnCtx<bool> ctx = runner.DataExists();
            if (!ctx.Value) {
                throw new FileNotFoundException("Could not find game data file, expected at: " + ctx.FileName);
            }
        }

        private void RestoreBackupData(IRunner runner) {
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
        private void BackupGameData(IRunner runner) {
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

        private UndertaleData LoadGameData(IRunner runner) {
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

        private void WriteGameData(UndertaleData data, IRunner runner) {
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

        private async Task ExecuteGame(IRunner runner) {
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

        #endregion
    }
}