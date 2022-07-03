using HoloCure.ModLoader.Logging.Writers;

namespace HoloCure.ModLoader.Logging
{
    public class LogWriter : ILogWriter
    {
        public string Source { get; }
        
        public int MinimumLogLevel { get; }

        public IConsoleWriter ConsoleWriter { get; }
        public IFileWriter FileWriter { get; }
        
        public LogWriter(string filePath, string source, int minimumLogLevel = 100, IConsoleWriter? consoleWriter = null, IFileWriter? fileWriter = null) {
            Source = source;
            MinimumLogLevel = minimumLogLevel;
            ConsoleWriter = consoleWriter ?? new ConsoleWriter(source, minimumLogLevel);
            FileWriter = fileWriter ?? new FileWriter(filePath, source, minimumLogLevel);
        }
        
        public void WriteLine(string message, LogLevel level) {
            LogMessage(message, level);
        }
        
        public void LogMessage(string message, LogLevel level) {
            ConsoleWriter.WriteLine(message, level);
            FileWriter.WriteLine(message, level);
        }

        public void MarkupMessage(string markup, string noMarkup, LogLevel level) {
            ConsoleWriter.MarkupLine(markup, level);
            FileWriter.WriteLine(noMarkup, level);
        }

        public void LogMessageConsoleOnly(string message, LogLevel level) {
            ConsoleWriter.WriteLine(message, level);
        }

        public void MarkupMessageConsoleOnly(string markup, LogLevel level) {
            ConsoleWriter.MarkupLine(markup, level);
        }

        public void LogMessageFileOnly(string message, LogLevel level) {
            FileWriter.WriteLine(message, level);
        }
        
        public void Dispose() {
            FileWriter.Dispose();
        }
    }
}