using CliFx.Infrastructure;
using Spectre.Console;

namespace HoloCure.ModLoader.Utils
{
    public static class ConsoleExtensions
    {
        public static IAnsiConsole AsAnsi(this IConsole console) {
            return AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.Detect,
                ColorSystem = ColorSystemSupport.Detect,
                Out = new AnsiConsoleOutput(console.Output)
            });
        }
    }
}