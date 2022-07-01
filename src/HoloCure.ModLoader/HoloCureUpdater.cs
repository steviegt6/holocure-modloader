using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HoloCure.ModLoader.Updater;
using Spectre.Console;

namespace HoloCure.ModLoader
{
    internal sealed class HoloCureUpdater : GitHubProgramUpdatable
    {
        public override string GitHubReleaseUrl => "https://api.github.com/repos/steviegt6/holocure-modloader/releases/latest";
        
        public override bool CanUpdate() {
            return !Debugger.IsAttached;
        }

        public override async Task<bool> CheckUpdate(string version) {
            if (!await base.CheckUpdate(version)) return false;
            if (Release is null) return false;

            // TODO: Add some form of auto-updating eventually:tm:.
            AnsiConsole.MarkupLine($"[white]Update available: [red]{version}[/] -> [yellow]{Release.TagName}[/].[/]");
            return true;
        }
    }
}