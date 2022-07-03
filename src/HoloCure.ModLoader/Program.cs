using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using HoloCure.ModLoader.API.Platform;
using HoloCure.ModLoader.Logging;
using HoloCure.ModLoader.Logging.Writers;
using HoloCure.ModLoader.Updater;

namespace HoloCure.ModLoader
{
    internal static class Program
    {
        internal static readonly IStorage Storage = GameModStorage.ResolveStorage();
        internal static readonly ILogWriter Logger;

        private static readonly IProgramUpdatable Updater;
        
        static Program() {
            Logger = new LogWriter(Path.Combine(Storage.BasePath, "Logs", "launcher.log"), "Launcher");
            Updater = new HoloCureUpdater(Logger);
        }

        public static async Task<int> Main(string[] args) {
            try {
                if (args.Length == 0) {
                    args = new[] {"run"};
                }

                string version = typeof(Program).Assembly.GetName().Version!.ToString();
                LogHeader(args, version);

                if (!args.Contains("-h") && !args.Contains("--help") && !args.Contains("--skip-updates")) {
                    await CheckForUpdates(version);
                }

                return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
            }
            catch (Exception e) {
                Logger.LogMessage(
                    "A fatal error was caught and the process must be aborted."
                    + "\nBelow is the stacktrace, which you should share when requesting help.",
                    LogLevels.Fatal
                );
                Logger.LogMessage(e.ToString(), LogLevels.Fatal);
                return -1;
            }
        }

        private static void LogHeader(string[] args, string version) {
            if (!args.Contains("-v") && !args.Contains("--version") && !args.Contains("-h") && !args.Contains("--help")) {
                Logger.LogMessage($"HoloCure.ModLoader v{version}", LogLevels.Debug);
            }

            Logger.MarkupMessage(
                "Report issues @ [blue underline]https://github.com/steviegt6/holocure-modloader[/]",
                "Report issues @ https://github.com/steviegt6/holocure-modloader",
                LogLevels.Debug
            );
            Logger.MarkupMessage(
                "Get support and talk @ [blue underline]https://discord.gg/Y8bvvqyFQw[/]\n",
                "Get support and talk @ https://discord.gg/Y8bvvqyFQw\n",
                LogLevels.Debug
            );
        }

        private static async Task CheckForUpdates(string version) {
            if (!Updater.CanUpdate()) {
                Logger.LogMessage("Skipping update checks...", LogLevels.Debug);
                return;
            }

            Logger.LogMessage("Checking for updates...", LogLevels.Debug);

            bool available = await Updater.CheckUpdate(version);

            if (available) {
                Logger.MarkupMessage("Download the latest release here: [blue underline]https://github.com/steviegt6/holocure-modloader/releases[/]",
                                     "Download the latest release here: https://github.com/steviegt6/holocure-modloader/releases",
                                     LogLevels.Info
                );
            }
            else {
                Logger.LogMessage("No updates available.", LogLevels.Debug);
            }

            Logger.LogMessageConsoleOnly("Wait 5 seconds or press any key to skip updating.\n", LogLevels.Warn);
            await new ConsoleStrangler().Strangle(5000);
        }
    }
}