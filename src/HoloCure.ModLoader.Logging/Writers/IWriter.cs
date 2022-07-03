namespace HoloCure.ModLoader.Logging.Writers
{
    public interface IWriter
    {
        string Source { get; }
        
        int MinimumLogLevel { get; }
        
        void WriteLine(string message, LogLevel level);
    }
}