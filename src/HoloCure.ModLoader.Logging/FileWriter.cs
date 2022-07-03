using HoloCure.ModLoader.Logging.Writers;

namespace HoloCure.ModLoader.Logging
{
    public class FileWriter : IFileWriter
    {
        public string Source { get; }

        public int MinimumLogLevel { get; }

        public string FilePath { get; }

        public StreamWriter Writer { get; }

        public FileWriter(string filePath, string source, int minimumLogLevel) {
            FilePath = filePath;
            Source = source;
            MinimumLogLevel = minimumLogLevel;

            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new ArgumentException($"File path \"{filePath}\" is not valid."));
            Writer = new StreamWriter(filePath, false)
            {
                AutoFlush = true
            };
        }

        public virtual void WriteLine(string message, LogLevel level) {
            if (level.Level < MinimumLogLevel) return;
            foreach (string msg in message.SplitNewlines()) Writer.WriteLine(LogUtils.GetMessageSignature(DateTime.Now, level, Source) + msg);
        }

        public void Dispose() {
            Writer.Dispose();
        }
    }
}