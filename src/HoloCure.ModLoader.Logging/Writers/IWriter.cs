namespace HoloCure.ModLoader.Logging.Writers
{
    public interface IWriter
    {
        string Source { get; }
        
        int MinimumLogLevel { get; }

        bool PrependSignature { get; }

        void WriteLine(string message, LogLevel level);
    }
}