using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CliFx.Attributes;
using HoloCure.ModLoader.API;
using HoloCure.ModLoader.Config;
using HoloCure.ModLoader.Logging;
using HoloCure.ModLoader.Runners;
using HoloCure.ModLoader.Utils;
using Spectre.Console;
using UndertaleModLib;

namespace HoloCure.ModLoader.Commands
{
    [Command("run", Description = "Runs a game using the mod loader.")]
    public class RunCommand : BaseCommand
    {
        [CommandOption("game", 'g', Description = "Overrides the default launch profile.")]
        public string? Game { get; set; }
        
        [CommandOption("game-path", 'p', Description = "Manually sets the game path. If not set but --game is set, reads from the config. If not specified in the config, assumes CWD.")]
        public string? GamePath { get; set; }

        [CommandOption("backup-path", 'b', Description = "Manually sets the backup path. If not set but --game is set, reads from the config. If not specified in the config, assumes CWD.")]
        public string? BackupPath { get; set; }

        [CommandOption("runner-path", 'r', Description = "Manually sets the runner path. If not set but --game is set, reads from the config. If not specified in the config, assumes CWD.")]
        public string? RunnerPath { get; set; }

        protected override async ValueTask ExecuteAsync() {
            // Get config.
            LaunchConfig cfg = LaunchConfig.Instance;

            // Populate Game if applicable.
            Game ??= cfg.DefaultProfile;

            if (LaunchConfig.Instance.Profiles.Count == 0) {
                throw new IndexOutOfRangeException("No profiles exist in the config. Please make one first. Consider also setting the default profile.");
            }

            if (Game is null) {
                Program.Logger.LogMessage("No game specified and no default profile is present.", LogLevels.Warn);
                SelectionPrompt<string> prompt = new SelectionPrompt<string>()
                                                .Title("[[Interactive]] Please select a profile:")
                                                .PageSize(5)
                                                .MoreChoicesText("(Move up/down to see more profiles)")
                                                .AddChoices(LaunchConfig.Instance.Profiles.Keys);
                Game = AnsiConsole.Prompt(prompt);
            }

            // Get launch profile if present.
            LaunchProfile? profile = cfg.Profiles.ContainsKey(Game) ? cfg.Profiles[Game] : null;

            // Probably shouldn't be able to happen now? I should check later.
            if (profile is null) {
                throw new NullReferenceException($"The chosen game, \"{Game}\", does not correspond with any existing profiles. Please make a profile first.");
            }

            // Get config-adjusted paths.
            GamePath = Utilities.GetUsableString(GamePath, profile.GamePath);
            BackupPath = Utilities.GetUsableString(BackupPath, profile.BackupPath);
            RunnerPath = Utilities.GetUsableString(RunnerPath, profile.RunnerPath);

            if (GamePath is null) {
                throw new NullReferenceException("No valid game path specified, either specify one with --game-path or correctly set the profile game path.");
            }
            
            if (BackupPath is null) {
                throw new NullReferenceException("No valid backup path specified, either specify one with --backup-path or correctly set the profile backup path.");
            }
            
            if (RunnerPath is null) {
                throw new NullReferenceException("No valid runner path specified, either specify one with --runner-path or correctly set the profile runner path.");
            }
            
            Program.Logger.MarkupMessage($"Running on profile: [white]{Game}[/]", "Running on profile: " + Game, LogLevels.Debug);
            Program.Logger.MarkupMessage($"Using game path: [white]{GamePath}[/]", "Using game path: " + GamePath, LogLevels.Debug);
            Program.Logger.MarkupMessage($"Using backup path: [white]{BackupPath}[/]", "Using backup path: " + BackupPath, LogLevels.Debug);
            Program.Logger.MarkupMessage($"Using runner path: [white]{RunnerPath}[/]", "Using runner path: " + RunnerPath, LogLevels.Debug);

            // Use these paths in the runner.
            IRunner runner = GetPlatformDependantRunner(GamePath, BackupPath, RunnerPath);

            VerifyDataExists(runner);
            RestoreLeftoverBackupData(runner);
            BackupGameData(runner);

            Program.Logger.MarkupMessage(
                "Loading game data... [white](this may take a while)[/]",
                "Loading game data... (this may take a while)",
                LogLevels.Debug
            );
            UndertaleData data = LoadGameData(runner);
            Program.Logger.LogMessage("Successfully loaded game data.", LogLevels.Debug);

            // TODO: Add external probing paths.
            Loader loader = new(data.GeneralInfo.Name.Content, new List<string>());
            loader.ResolveMods();
            loader.SortMods();
            loader.InstantiateMods();
            loader.LoadMods();
            loader.PatchGame(data);

            WriteGameData(data, runner);
            await ExecuteGame(runner, loader);
            loader.UnloadMods();
            RestoreBackupData(runner);
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

        private void RestoreLeftoverBackupData(IRunner runner) {
            Program.Logger.LogMessage("Restoring leftover backup data...", LogLevels.Debug);
            RunnerReturnCtx<RestoreLeftOverBackupDataResult> ctx = runner.RestoreLeftoverBackupData();

            switch (ctx.Value) {
                case RestoreLeftOverBackupDataResult.PermissionError:
                    // TODO: Mention issue deleting backup as well. Split, maybe?
                    throw new UnauthorizedAccessException("Cannot overwrite game data at: " + ctx.FileName);

                case RestoreLeftOverBackupDataResult.Skipped:
                    Program.Logger.LogMessage(
                        "Backup file not found, assuming either the program successfully exited previously or this is a first launch, skipping...",
                        LogLevels.Debug
                    );
                    break;

                case RestoreLeftOverBackupDataResult.Success:
                    Program.Logger.LogMessage("Restored game data from backup file. The mod loader did not successfully close last session.", LogLevels.Warn);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // TODO: Necessary every time? Maybe so to ensure users updating their HoloCure game don't have outdated backups (reading comprehension issue).
        private void BackupGameData(IRunner runner) {
            Program.Logger.LogMessage("Backing up game data...", LogLevels.Debug);
            RunnerReturnCtx<BackupDataResult> ctx = runner.BackupData();

            switch (ctx.Value) {
                case BackupDataResult.MissingFile:
                    throw new FileNotFoundException("Cannot backup game data file as game data file does not exist, expected at: " + ctx.FileName);

                case BackupDataResult.PermissionError:
                    throw new UnauthorizedAccessException($"Could not backup game data, permission denied to write to file \"{ctx.BackupName}\".");

                case BackupDataResult.Skipped:
                    Program.Logger.LogMessage($"Skipped backing up to \"{ctx.BackupName}\".", LogLevels.Debug);
                    break;

                case BackupDataResult.Success:
                    Program.Logger.LogMessage($"Game data backed up to \"{ctx.BackupName}\".", LogLevels.Debug);
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
            Program.Logger.LogMessage("Writing game data to file for execution...", LogLevels.Debug);
            RunnerReturnCtx<WriteGameDataResult> ctx = runner.WriteGameData(data);

            switch (ctx.Value) {
                case WriteGameDataResult.PermissionError:
                    throw new UnauthorizedAccessException("Cannot write game data file, permission denied at: " + ctx.FileName);

                case WriteGameDataResult.Success:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            Program.Logger.LogMessage($"Game data written to \"{ctx.FileName}\".", LogLevels.Debug);
        }

        private async Task ExecuteGame(IRunner runner, Loader loader) {
            Program.Logger.LogMessage("Executing game...", LogLevels.Debug);
            loader.GameStarted();
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

        private void RestoreBackupData(IRunner runner) {
            Program.Logger.LogMessage("Game exited, restoring saved backup data...", LogLevels.Debug);
            RunnerReturnCtx<RestoreBackupDataResult> ctx = runner.RestoreBackupData();

            switch (ctx.Value) {
                case RestoreBackupDataResult.MissingFile:
                    throw new FileNotFoundException("Missing backup file (how did this happen?), expected at: " + ctx.FileName);
                
                case RestoreBackupDataResult.PermissionError:
                    // TODO: Blah blah blah message about deleting too.
                    throw new UnauthorizedAccessException("Cannot overwrite game data at: " + ctx.FileName);
                
                case RestoreBackupDataResult.Success:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}