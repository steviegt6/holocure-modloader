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
        // Put this in the base so it's obviously shown.
        [CommandOption("skip-updates", Description = "Skips checking updates upon launch.")]
        public bool SkipUpdates { get; set; }

        public async ValueTask ExecuteAsync(IConsole console) {
            await ExecuteAsync();
            LaunchConfig.SerializeConfig(LaunchConfig.Instance);
        }

        protected abstract ValueTask ExecuteAsync();
    }
}