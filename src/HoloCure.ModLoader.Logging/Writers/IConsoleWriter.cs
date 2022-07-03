namespace HoloCure.ModLoader.Logging.Writers
{
    public interface IConsoleWriter : IWriter
    {
        void MarkupLine(string message, LogLevel level);
    }
}