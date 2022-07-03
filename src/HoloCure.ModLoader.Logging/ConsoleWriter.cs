using HoloCure.ModLoader.Logging.Writers;
using Spectre.Console;

namespace HoloCure.ModLoader.Logging
{
    public class ConsoleWriter : IConsoleWriter
    {
        public string Source { get; }
        
        public int MinimumLogLevel { get; }
        
        public ConsoleWriter(string source, int minimumLogLevel) {
            Source = source;
            MinimumLogLevel = minimumLogLevel;
        }

        public virtual void WriteLine(string message, LogLevel level) {
            if (level.Level < MinimumLogLevel) return;
            
            foreach (string msg in message.SplitNewlines()) {
                DoActionPreserveColors((_, _) =>
                {
                    if (LogLevels.ConsoleColors.ContainsKey(level)) {
                        (Color? foreground, Color? background) = LogLevels.ConsoleColors[level];
                    
                        if (foreground.HasValue) AnsiConsole.Foreground = foreground.Value;
                        if (background.HasValue) AnsiConsole.Background = background.Value;
                    }

                    AnsiConsole.Write(LogUtils.GetMessageSignature(DateTime.Now, level, Source));
                    AnsiConsole.WriteLine(msg);
                });
            }
        }

        public virtual void MarkupLine(string message, LogLevel level) {
            if (level.Level < MinimumLogLevel) return;
            foreach (string msg in message.SplitNewlines()) {
                DoActionPreserveColors((_, background) =>
                {
                    if (LogLevels.ConsoleColors.ContainsKey(level)) {
                        (Color? newForeground, Color? newBackgroun) = LogLevels.ConsoleColors[level];
                    
                        if (newForeground.HasValue) AnsiConsole.Foreground = newForeground.Value;
                        if (newBackgroun.HasValue) AnsiConsole.Background = newBackgroun.Value;
                    }

                    string color = '#' + AnsiConsole.Foreground.ToHex();
                    
                    if (AnsiConsole.Background != background)
                        color += " on #" + AnsiConsole.Background.ToHex();

                    AnsiConsole.Write(LogUtils.GetMessageSignature(DateTime.Now, level, Source));
                    AnsiConsole.MarkupLine($"[{color}]" + msg + "[/]");
                });
            }
        }

        protected static void DoActionPreserveColors(Action<Color, Color> action) {
            Color foreground = AnsiConsole.Foreground;
            Color background = AnsiConsole.Background;

            action(foreground, background);
            
            AnsiConsole.Foreground = foreground;
            AnsiConsole.Background = background;
        }
    }
}