using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using HoloCure.ModLoader.Config;
using HoloCure.ModLoader.Utils;
using Spectre.Console;

namespace HoloCure.ModLoader.Commands
{
    public abstract class BaseCommand : ICommand
    {
        protected IAnsiConsole AnsiConsole { get; private set; } = null!; // Not null when actually executing.

        // Put this in the base so it's obviously shown.
        [CommandOption("skip-updates", Description = "Skips checking updates upon launch.")]
        public bool SkipUpdates { get; set; }

        public async ValueTask ExecuteAsync(IConsole console) {
            AnsiConsole = console.AsAnsi();
            await ExecuteAsync();
            LaunchConfig.SerializeConfig(LaunchConfig.Instance);
        }

        protected abstract ValueTask ExecuteAsync();
    }
}