using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CliFx.Attributes;
using DogScepterLib.Core;
using HoloCure.ModLoader.API;
using HoloCure.ModLoader.Config;
using HoloCure.ModLoader.Konata;
using HoloCure.ModLoader.Logging;
using HoloCure.ModLoader.Utils;
using Spectre.Console;

namespace HoloCure.ModLoader.Commands
{
    [Command("run", Description = "Runs a game using the mod loader.")]
    public class RunCommand : BaseCommand
    {
        [CommandOption("game", 'g', Description = "Overrides the default launch profile.")]
        public string? Game { get; set; }
        
        [CommandOption("game-path", 'p', Description = "Overrides the game path specified in the game profile (--game).")]
        public string? GamePath { get; set; }

        [CommandOption("backup-path", 'b', Description = "Overrides the backup path specified in the game profile (--game).")]
        public string? BackupPath { get; set; }

        [CommandOption("runner-path", 'r', Description = "Overrides the runner path specified in the game profile (--game).")]
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

            VerifyDataExists();
            RestoreLeftoverBackupData();
            BackupGameData();

            Program.Logger.MarkupMessage(
                "Loading game data... [white](this may take a while)[/]",
                "Loading game data... (this may take a while)",
                LogLevels.Debug
            );
            GMData data = ReadData();
            Program.Logger.LogMessage("Successfully loaded game data.", LogLevels.Debug);

            // TODO: Add external probing paths.
            Loader loader = new(Game, new List<string>());
            loader.ResolveMods();
            loader.SortMods();
            loader.InstantiateMods();
            loader.LoadMods();
            loader.PatchGame(data);

            WriteData(data);
            await ExecuteGame(loader, OperatingSystemUtils.GetKonataBootstrapper());
            loader.UnloadMods();
            RestoreBackupData();
        }

        #region Runner Logic

        private void VerifyDataExists() {
            if (!File.Exists(GamePath)) throw new FileNotFoundException("Could not resolve data file: " + GamePath);
        }

        private void RestoreLeftoverBackupData() {
            Program.Logger.LogMessage("Restoring leftover backup data...", LogLevels.Debug);

            if (!File.Exists(BackupPath)) {
                Program.Logger.LogMessage("Backup file not found, skipping.", LogLevels.Debug);
                return;
            }

            try {
                Program.Logger.LogMessage("Backup file found - mod loader did not successfully close last launch - restoring...", LogLevels.Warn);
                File.Copy(BackupPath, GamePath!, true);
                Program.Logger.LogMessage("Backup file restored.", LogLevels.Debug);
            }
            catch {
                Program.Logger.LogMessage("Could not restore backup!", LogLevels.Fatal);
                throw;
            }
        }

        private void BackupGameData() {
            Program.Logger.LogMessage("Backing up game data...", LogLevels.Debug);
            
            if (!File.Exists(GamePath)) throw new FileNotFoundException("Could not resolve game file to back up.");

            try {
                File.Copy(GamePath, BackupPath!, true);
                Program.Logger.LogMessage("Backed up game data.", LogLevels.Debug);
            }
            catch {
                Program.Logger.LogMessage("Could not back up game data!", LogLevels.Fatal);
                throw;
            }
        }

        private GMData ReadData() {
            Program.Logger.LogMessage("Reading game data...", LogLevels.Debug);
            
            if (!File.Exists(GamePath)) throw new FileNotFoundException("Could not resolve game file to read.");

            try {
                Stream stream = new FileStream(GamePath, FileMode.Open, FileAccess.Read);
                GMDataReader reader = new(stream, GamePath)
                {
                    Data =
                    {
                        Logger = message => { Program.Logger.LogMessage(message, LogLevels.Debug); }
                    }
                };
                reader.Deserialize();
                return reader.Data;
            }
            catch {
                Program.Logger.LogMessage("An exception occured while reading the game data.", LogLevels.Fatal);
                throw;
            }
        }

        private void WriteData(GMData data) {
            Program.Logger.LogMessage("Writing game data to file for execution...", LogLevels.Debug);

            try {
                using Stream stream = new FileStream(GamePath!, FileMode.Create, FileAccess.Write);
                using GMDataWriter writer = new(data, stream, GamePath);
                writer.Write();
                writer.Flush();
            }
            catch {
                Program.Logger.LogMessage("An exception occured while writing the game data.", LogLevels.Fatal);
                throw;
            }
        }

        private async Task ExecuteGame(Loader loader, IKonataBootstrapper bootstrapper) {
            Program.Logger.LogMessage("Executing game...", LogLevels.Debug);
            
            string fillPath = Path.GetFullPath(RunnerPath!);
            if (!File.Exists(fillPath)) throw new FileNotFoundException("Could not resolve runner file to execute.");
            
            loader.GameStarting();
            
            Process? proc;
            
            try {
                proc = await bootstrapper.StartGame(RunnerPath!, GamePath!);
            }
            catch {
                Program.Logger.LogMessage("Could not start game!", LogLevels.Fatal);
                throw;
            }
            
            if (proc is null) throw new NullReferenceException("Could not start game!");
            await proc.WaitForExitAsync();
        }
        
        private void RestoreBackupData() {
            Program.Logger.LogMessage("Game exited, restoring saved backup data...", LogLevels.Debug);
            
            if (!File.Exists(BackupPath)) throw new FileNotFoundException("Could not resolve backup file to restore.");

            try {
                File.Copy(BackupPath, GamePath!, true);
            }
            catch {
                Program.Logger.LogMessage("Could not restore backup!", LogLevels.Fatal);
                throw;
            }

            try {
                File.Delete(BackupPath);
            }
            catch {
                Program.Logger.LogMessage("Could not delete backup file!", LogLevels.Fatal);
                throw;
            }
        }

        #endregion
    }
}