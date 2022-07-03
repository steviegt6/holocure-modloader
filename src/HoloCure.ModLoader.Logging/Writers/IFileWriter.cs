namespace HoloCure.ModLoader.Logging.Writers
{
    public interface IFileWriter : IWriter, IDisposable
    {
        string FilePath { get; }

        StreamWriter Writer { get; }
    }
}