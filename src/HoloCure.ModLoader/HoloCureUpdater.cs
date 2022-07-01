using System;
using System.Threading.Tasks;
using HoloCure.ModLoader.Updater;

namespace HoloCure.ModLoader
{
    internal sealed class HoloCureUpdater : GitHubProgramUpdatable
    {
        public override string GitHubReleaseUrl => "https://api.github.com/repos/steviegt6/holocure-modloader/releases/latest";
        
        public override bool CanUpdate() {
            return true;
        }

        public override async Task<bool> CheckUpdate(string version) {
            if (!await base.CheckUpdate(version)) return false;
            if (Release is null) return false;

            // TODO: Add some form of auto-updating eventually:tm:.
            Console.WriteLine($"Update available: {version} -> {Release.TagName}.");
            Console.WriteLine($"Download it @ {GitHubReleaseUrl}");
            return true;
        }
    }
}