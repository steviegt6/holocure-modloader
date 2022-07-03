using System.IO;
using HoloCure.ModLoader.Logging;
using HoloCure.ModLoader.Logging.Writers;

namespace HoloCure.ModLoader
{
    public class ConditionalFileWriter : IFileWriter
    {
        public string Source { get; }
        
        public int MinimumLogLevel { get; }
        
        public bool PrependSignature { get; }
        
        public string FilePath { get; }
        
        public StreamWriter Writer { get; }
        
        protected readonly FileWriter? UnderlyingWriter;

        public ConditionalFileWriter(string filePath, string source, int minimumLogLevel = 100, bool actuallyLog = true) {
            FilePath = filePath;
            Source = source;
            MinimumLogLevel = minimumLogLevel;
            PrependSignature = actuallyLog;
            
            UnderlyingWriter = actuallyLog ? new FileWriter(filePath, source, minimumLogLevel, actuallyLog) : null;
            Writer = UnderlyingWriter is not null ? UnderlyingWriter.Writer : StreamWriter.Null;
        }

        public void WriteLine(string message, LogLevel level) {
             UnderlyingWriter?.WriteLine(message, level);
        }

        public void Dispose() {
            UnderlyingWriter?.Dispose();
        }
    }
}