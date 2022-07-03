using HoloCure.ModLoader.Logging.Writers;

namespace HoloCure.ModLoader.Logging
{
    public class LogWriter : ILogWriter
    {
        public string Source { get; }
        
        public int MinimumLogLevel { get; }
        
        public bool PrependSignature { get; }

        public IConsoleWriter ConsoleWriter { get; }
        
        public IFileWriter FileWriter { get; }
        
        public LogWriter(string filePath, string source, bool prependSignature, int minimumLogLevel = 100, IConsoleWriter? consoleWriter = null, IFileWriter? fileWriter = null) {
            Source = source;
            PrependSignature = prependSignature;
            MinimumLogLevel = minimumLogLevel;
            ConsoleWriter = consoleWriter ?? new ConsoleWriter(source, minimumLogLevel, prependSignature);
            FileWriter = fileWriter ?? new FileWriter(filePath, source, minimumLogLevel, prependSignature);
        }
        
        public virtual void WriteLine(string message, LogLevel level) {
            LogMessage(message, level);
        }
        
        public virtual void LogMessage(string message, LogLevel level) {
            ConsoleWriter.WriteLine(message, level);
            FileWriter.WriteLine(message, level);
        }

        public virtual void MarkupMessage(string markup, string noMarkup, LogLevel level) {
            ConsoleWriter.MarkupLine(markup, level);
            FileWriter.WriteLine(noMarkup, level);
        }

        public virtual void LogMessageConsoleOnly(string message, LogLevel level) {
            ConsoleWriter.WriteLine(message, level);
        }

        public virtual void MarkupMessageConsoleOnly(string markup, LogLevel level) {
            ConsoleWriter.MarkupLine(markup, level);
        }

        public virtual void LogMessageFileOnly(string message, LogLevel level) {
            FileWriter.WriteLine(message, level);
        }
        
        public virtual void Dispose() {
            FileWriter.Dispose();
        }
    }
}