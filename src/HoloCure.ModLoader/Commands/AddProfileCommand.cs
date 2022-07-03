using System.IO;
using System.Threading.Tasks;
using CliFx.Attributes;
using HoloCure.ModLoader.Config;
using Spectre.Console;

namespace HoloCure.ModLoader.Commands
{
    [Command("add-profile", Description = "Allows you to add a profile.")]
    public class AddProfileCommand : BaseCommand
    {
        [CommandOption("name", 'n', Description = "Sets the profile name.")]
        public string? Name { get; set; }

        [CommandOption("game-path", 'g', Description = "Sets the game path.")]
        public string? GamePath { get; set; }

        [CommandOption("backup-path", 'b', Description = "Sets the backup path.")]
        public string? BackupPath { get; set; }
        
        [CommandOption("launcher-path", 'p', Description = "Sets the launcher path.")]
        public string? LauncherPath { get; set; }

        [CommandOption("interactive", 'i', Description = "Interactively set profile properties.")]
        public bool Interactive { get; set; }

        protected override ValueTask ExecuteAsync() {
            AnsiConsole.WriteLine();
            
            if (!Interactive && (Name == null || GamePath == null || BackupPath == null || LauncherPath == null)) {
                AnsiConsole.MarkupLine("[red]Since interactive mode is not enabled, you must specify all profile properties.[/]");
                return default;
            }

            string? nameFailureReason = ValidateName(Name);
            if (nameFailureReason is not null) {
                AnsiConsole.MarkupLine($"[red]Could not validate name: {nameFailureReason}[/]");
                Name = AnsiConsole.Prompt(
                    new TextPrompt<string>("Please enter a valid profile name:")
                       .Validate(name =>
                        {
                            string? failureReason = ValidateName(name);

                            return failureReason is null ? ValidationResult.Success() : ValidationResult.Error($"[red]Could not validate name: {failureReason}[/]");
                        })
                );
            }

            void Validate(ref string? path, string pathName) {
                string? pathFailureReason = ValidatePath(path);
                if (pathFailureReason is not null) {
                    AnsiConsole.MarkupLine($"[red]Could not validate {pathName}: {pathFailureReason}[/]");
                    AnsiConsole.Prompt(
                        new TextPrompt<string>($"Please enter a valid {pathName}:")
                           .Validate(p =>
                            {
                                p = p.Trim('\"'); // If dragged in, it may have quotes around it. That's no good.
                                string? failureReason = ValidatePath(p);

                                return failureReason is null ? ValidationResult.Success() : ValidationResult.Error($"[red]Could not validate {pathName}: {failureReason}[/]");
                            })
                    );
                }
            }

            string? gamePath = GamePath;
            string? backupPath = BackupPath;
            string? launcherPath = LauncherPath;
            
            Validate(ref gamePath, "game path");
            GamePath = gamePath;
            
            Validate(ref backupPath, "backup path");
            BackupPath = backupPath;
            
            Validate(ref launcherPath, "launcher path");
            LauncherPath = launcherPath;
           
            return default;
        }

        private static string? ValidateName(string? name) {
            if (name is null) return "The given profile name as null.";
            if (LaunchConfig.Instance.Profiles.ContainsKey(name)) return $"A launch profile with the name \"{name}\" already exists!";
            return null;
        }

        private string? ValidatePath(string? path) {
            if (path is null) return "The given path is null.";
            if (!File.Exists(path)) return "The given path does not point to a file present on your system.";
            if (path == GamePath || path == BackupPath || path == LauncherPath) return "The given path is the same as another profile property, which will lead to conflicts.";
            return null;
        }
    }
}