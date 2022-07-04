using System;

namespace HoloCure.ModLoader.Logging.Writers
{
    public interface ILogWriter : IWriter, IDisposable
    {
        IConsoleWriter ConsoleWriter { get; }

        IFileWriter FileWriter { get; }

        void LogMessage(string message, LogLevel level);

        void MarkupMessage(string markup, string noMarkup, LogLevel level);
        
        void LogMessageConsoleOnly(string message, LogLevel level);
        
        void MarkupMessageConsoleOnly(string markup, LogLevel level);

        void LogMessageFileOnly(string message, LogLevel level);
    }
}