using System.Threading.Tasks;
using CliFx.Attributes;
using HoloCure.ModLoader.Config;
using Spectre.Console;

namespace HoloCure.ModLoader.Commands
{
    [Command("config", Description = "Allows you to modify the config. For profiles, set set-profile, list-profiles, add-profile, and remove-profile.")]
    public class ConfigCommand : BaseCommand
    {
        [CommandOption("check-for-updates", 'u', Description = "Whether the program should check for updates when launched.")]
        public bool? CheckForUpdates { get; set; }

        protected override ValueTask ExecuteAsync() {
            if (CheckForUpdates.HasValue) {
                AnsiConsole.MarkupLine($"Setting \"check_for_updates\" to [yellow]{CheckForUpdates.Value}[/].");
                LaunchConfig.Instance.CheckForUpdates = CheckForUpdates.Value;
            }

            return default;
        }
    }
}