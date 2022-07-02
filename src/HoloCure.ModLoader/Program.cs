using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using HoloCure.ModLoader.API.Platform;
using HoloCure.ModLoader.Updater;
using Spectre.Console;

namespace HoloCure.ModLoader
{
    internal static class Program
    {
        private static readonly IProgramUpdatable Updater = new HoloCureUpdater();
        
        internal static readonly IStorage Storage = GameModStorage.ResolveStorage();

        public static async Task<int> Main(string[] args) {
            try {
                if (args.Length == 0) {
                    args = new[] {"run", "--no-args"};
                }

                string version = typeof(Program).Assembly.GetName().Version!.ToString();
                LogHeader(version);

                if (!args.Contains("-h") && !args.Contains("--help") && !args.Contains("--skip-updates")) {
                    await CheckForUpdates(version);
                }

                return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
            }
            catch (Exception e) {
                AnsiConsole.MarkupLine(
                    "\n[red]A [bold]fatal[/] error was caught and the process must be aborted."
                    + "\nBelow is the stacktrace, which you should share when requesting help.[/]\n"
                );
                AnsiConsole.WriteException(e);
                return -1;
            }
        }

        private static void LogHeader(string version) {
            AnsiConsole.MarkupLine("[aqua]HoloCure[/].[fuchsia]ModLoader[/] [yellow]v" + version + "[/]");
            AnsiConsole.MarkupLine("[white]by Tomat & contributors.[/]");
            AnsiConsole.MarkupLine("\nReport issues @ [blue underline]https://github.com/steviegt6/holocure-modloader[/]");
            AnsiConsole.MarkupLine("Get support and talk @ [blue underline]https://discord.gg/Y8bvvqyFQw[/]\n");
        }

        private static async Task CheckForUpdates(string version) {
            AnsiConsole.MarkupLine("[gray]Checking for updates...[/]");

            bool available = Updater.CanUpdate() && await Updater.CheckUpdate(version);

            AnsiConsole.MarkupLine(available
                                       ? "[white]Download the latest release here: [blue underline]https://github.com/steviegt6/holocure-modloader/releases[/][/]"
                                       : "[gray]No updates available.[/]\n");

            AnsiConsole.MarkupLine("\n[white]Wait 5 seconds or press any key to skip updating.[/]\n");
            await new ConsoleStrangler().Strangle(5000);
        }
    }
}