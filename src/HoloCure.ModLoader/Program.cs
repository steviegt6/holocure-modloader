using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using HoloCure.ModLoader.API.Platform;
using HoloCure.ModLoader.Logging;
using HoloCure.ModLoader.Logging.Writers;
using HoloCure.ModLoader.Updater;
using Spectre.Console;

namespace HoloCure.ModLoader
{
    internal static class Program
    {
        internal static readonly IStorage Storage = GameModStorage.ResolveStorage();
        
        internal static ILogWriter Logger = null!;

        private static IProgramUpdatable Updater = null!;

        public static async Task<int> Main(string[] args) {
            try {
                if (args.Length == 0) {
                    args = new[] {"run"};
                }

                string path = Path.Combine(Storage.BasePath, "Logs", "launcher.log");
                const string source = "Launcher";
                bool prepend = args.Contains("run");
                Logger = new LogWriter(path, source, prepend, fileWriter: new ConditionalFileWriter(path, source, actuallyLog: prepend));
                Updater = new HoloCureUpdater(Logger);

                string version = typeof(Program).Assembly.GetName().Version!.ToString();
                LogHeader(args, version);

                if (!args.Contains("-h") && !args.Contains("--help") && !args.Contains("--skip-updates")) {
                    await CheckForUpdates(version);
                }

                return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
            }
            catch (Exception e) {
                // Disable because this can be null exclusively in this method.
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (Logger is not null) {
                    Logger.LogMessage(
                        "A fatal error was caught and the process must be aborted."
                        + "\nBelow is the stacktrace, which you should share when requesting help.",
                        LogLevels.Fatal
                    );
                    Logger.LogMessage(e.ToString(), LogLevels.Fatal);
                }
                else {
                    AnsiConsole.MarkupLine("[red]AN EXCEPTION WAS THROWN BEFORE LOGGING COULD BEGIN:[/]");
                    AnsiConsole.WriteException(e);
                }

                return 1;
            }
        }

        private static void LogHeader(string[] args, string version) {
            if (!args.Contains("-v") && !args.Contains("--version") && !args.Contains("-h") && !args.Contains("--help")) {
                Logger.LogMessage($"HoloCure.ModLoader v{version}", LogLevels.Info);
            }

            Logger.MarkupMessage(
                "Report issues @ [blue underline]https://github.com/steviegt6/holocure-modloader[/]",
                "Report issues @ https://github.com/steviegt6/holocure-modloader",
                LogLevels.Info
            );
            Logger.MarkupMessage(
                "Get support and talk @ [blue underline]https://discord.gg/Y8bvvqyFQw[/]",
                "Get support and talk @ https://discord.gg/Y8bvvqyFQw",
                LogLevels.Info
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

            Logger.LogMessageConsoleOnly("Wait 5 seconds or press any key to skip updating.", LogLevels.Warn);
            await new ConsoleStrangler().Strangle(5000);
        }
    }
}