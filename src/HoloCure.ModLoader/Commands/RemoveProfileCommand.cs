using System.Threading.Tasks;
using CliFx.Attributes;
using HoloCure.ModLoader.Config;
using Spectre.Console;

namespace HoloCure.ModLoader.Commands
{
    [Command("remove-profile", Description = "Lets you remove a profile from the config.")]
    public class RemoveProfile : BaseCommand
    {
        [CommandOption("profile", 'p', Description = "The name of the profile to set as default.")]
        public string Profile { get; set; } = "";

        [CommandOption("interactive", 'i', Description = "Enables interactive mode, letting you choose from the CLI.")]
        public bool Interactive { get; set; } = false;

        protected override ValueTask ExecuteAsync() {
            AnsiConsole.WriteLine();

            if (Interactive) {
                SelectionPrompt<string> prompt = new SelectionPrompt<string>()
                                                .Title("[[Interactive]] Please select a profile:")
                                                .PageSize(5)
                                                .MoreChoicesText("(Move up/down to see more profiles)")
                                                .AddChoices(LaunchConfig.Instance.Profiles.Keys);
                Profile = AnsiConsole.Prompt(prompt);
            }

            if (!LaunchConfig.Instance.Profiles.ContainsKey(Profile)) {
                AnsiConsole.MarkupLine($"[red]The profile \"[yellow]{Profile}[/]\" does not exist. It cannot be removed.[/]");
            }
            else {
                LaunchConfig.Instance.Profiles.Remove(Profile);
                AnsiConsole.MarkupLine($"Successfully removed profile: [yellow]{Profile}[/]");
            }


            return default;
        }
    }
}