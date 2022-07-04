using System;
using System.IO;
using HoloCure.ModLoader.Logging.Writers;

namespace HoloCure.ModLoader.Logging
{
    public class FileWriter : IFileWriter
    {
        public string Source { get; }

        public int MinimumLogLevel { get; }
        
        public bool PrependSignature { get; }

        public string FilePath { get; }

        public StreamWriter Writer { get; }

        public FileWriter(string filePath, string source, int minimumLogLevel, bool prependSignature) {
            FilePath = filePath;
            Source = source;
            MinimumLogLevel = minimumLogLevel;
            PrependSignature = prependSignature;

            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new ArgumentException($"File path \"{filePath}\" is not valid."));
            Writer = new StreamWriter(filePath, false)
            {
                AutoFlush = true
            };
        }

        public virtual void WriteLine(string message, LogLevel level) {
            if (level.Level < MinimumLogLevel) return;
            foreach (string msg in message.SplitNewlines())
                Writer.WriteLine((PrependSignature ? LogUtils.GetMessageSignature(DateTime.Now, level, Source) : "") + msg);
        }

        public virtual void Dispose() {
            Writer.Dispose();
        }
    }
}