using System.Threading.Tasks;
using CliFx.Attributes;
using HoloCure.ModLoader.Config;
using Spectre.Console;

namespace HoloCure.ModLoader.Commands
{
    [Command("list-profiles", Description = "Lists every profile in the user's config.")]
    public class ListProfilesCommand : BaseCommand
    {
        [CommandOption("verbose", Description = "Verbosely display extra profile data.")]
        public bool Verbose { get; set; } = true;

        [CommandOption("display-default", 'd', Description = "Display the current default profile as well.")]
        public bool DisplayDefault { get; set; } = true;

        protected override ValueTask ExecuteAsync() {
            AnsiConsole.WriteLine();

            if (DisplayDefault) {
                AnsiConsole.MarkupLine($"Default profile: [white]{LaunchConfig.Instance.DefaultProfile}[/]\n");
            }

            if (LaunchConfig.Instance.Profiles.Count == 0) {
                AnsiConsole.MarkupLine("[red]No profiles found.[/]");
                return default;
            }

            int num = 0;
            foreach ((string name, LaunchProfile profile) in LaunchConfig.Instance.Profiles) {
                num++;

                AnsiConsole.MarkupLine($"{num}. [white]{name}[/]{(Verbose ? ':' : "")}");

                if (!Verbose) {
                    continue;
                }

                AnsiConsole.MarkupLine($"      [silver]GamePath:[/] [yellow]{profile.GamePath}[/]");
                AnsiConsole.MarkupLine($"      [silver]BackupPath:[/] [yellow]{profile.BackupPath}[/]");
                AnsiConsole.MarkupLine($"      [silver]RunnerPath:[/] [yellow]{profile.RunnerPath}[/]");

                // Extra spacing for formatting.
                if (num != LaunchConfig.Instance.Profiles.Count) {
                    AnsiConsole.WriteLine();
                }
            }

            return default;
        }
    }
}