using System.Threading.Tasks;
using CliFx.Attributes;
using HoloCure.ModLoader.Config;
using Spectre.Console;

namespace HoloCure.ModLoader.Commands
{
    [Command("set-profile", Description = "Sets the default user profile. You can find a list with list-profiles.")]
    public class SetProfileCommand : BaseCommand
    {
        [CommandOption("profile", 'p', Description = "The name of the profile to set as default.")]
        public string? Profile { get; set; } = null;

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

            LaunchConfig.Instance.DefaultProfile = Profile;
            AnsiConsole.MarkupLine($"Default profile set to: [yellow]{Profile ?? "null"}[/]");


            return default;
        }
    }
}