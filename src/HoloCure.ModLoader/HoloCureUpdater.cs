using System.Diagnostics;
using System.Threading.Tasks;
using HoloCure.ModLoader.Config;
using HoloCure.ModLoader.Logging;
using HoloCure.ModLoader.Logging.Writers;
using HoloCure.ModLoader.Updater;

namespace HoloCure.ModLoader
{
    internal sealed class HoloCureUpdater : GitHubProgramUpdatable
    {
        public override string GitHubReleaseUrl => "https://api.github.com/repos/steviegt6/holocure-modloader/releases/latest";

        private readonly ILogWriter Logger;
        
        public HoloCureUpdater(ILogWriter logger) {
            Logger = logger;
        }
        
        public override bool CanUpdate() {
            return !Debugger.IsAttached && LaunchConfig.Instance.CheckForUpdates;
        }

        public override async Task<bool> CheckUpdate(string version) {
            if (!await base.CheckUpdate(version)) return false;
            if (Release is null) return false;

            // TODO: Add some form of auto-updating eventually:tm:.
            Logger.WriteLine($"Update available: {version} -> {Release.TagName}", LogLevels.Warn);
            return true;
        }
    }
}