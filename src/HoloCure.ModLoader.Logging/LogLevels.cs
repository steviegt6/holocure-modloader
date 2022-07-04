using System;
using System.Collections.Generic;
using Spectre.Console;

namespace HoloCure.ModLoader.Logging
{
    public static class LogLevels
    {
        public static readonly LogLevel Verbose = new("Verbose", 100);
        public static readonly LogLevel Debug = new("Debug", 200);
        public static readonly LogLevel Info = new("Info", 300);
        public static readonly LogLevel Warn = new("Warn", 300);
        public static readonly LogLevel Error = new("Error", 300);
        public static readonly LogLevel Fatal = new("Fatal", 300);

        public static readonly Dictionary<LogLevel, (Color? foreground, Color? background)> ConsoleColors = new()
        {
            {Verbose, (ConsoleColor.DarkGray, null)},
            {Debug, (ConsoleColor.DarkGray, null)},
            {Info, (ConsoleColor.Gray, null)},
            {Warn, (ConsoleColor.Yellow, null)},
            {Error, (ConsoleColor.Red, null)},
            {Fatal, (ConsoleColor.DarkRed, null)}
        };
    }
}