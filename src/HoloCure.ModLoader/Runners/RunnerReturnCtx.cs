namespace HoloCure.ModLoader.Runners
{
    /// <summary>
    ///     Return context for external logging.
    /// </summary>
    /// <param name="Value">The return value.</param>
    /// <param name="FileName">The data file name.</param>
    /// <param name="BackupName">The data file backup name.</param>
    /// <param name="RunnerName">The executable runner file name.</param>
    /// <typeparam name="T">The return value's type.</typeparam>
    public readonly record struct RunnerReturnCtx<T>(T Value, string FileName, string BackupName, string? RunnerName);
}