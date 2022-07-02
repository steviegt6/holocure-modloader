using System.Diagnostics;
using UndertaleModLib;

namespace HoloCure.ModLoader.Runners
{
    /// <summary>
    ///     Non-YYC GMS2 game runner. Expected to be platform-dependant.
    /// </summary>
    public interface IRunner
    {
        /// <summary>
        ///     Whether the data file exists.
        /// </summary>
        RunnerReturnCtx<bool> DataExists();

        /// <summary>
        ///     Restores the data file from a backup file.
        /// </summary>
        /// <param name="allowFirstLaunch">Whether to permit missing backups.</param>
        RunnerReturnCtx<RestoreBackupDataResult> RestoreBackupData(bool allowFirstLaunch = true);

        /// <summary>
        ///     Backs up the data file by writing it to disk.
        /// </summary>
        /// <param name="skipOverwrite">Skip writing a backup if it already exists.</param>
        RunnerReturnCtx<BackupDataResult> BackupData(bool skipOverwrite = false);

        /// <summary>
        ///     Loads game data to an <see cref="UndertaleData"/> instance.
        /// </summary>
        RunnerReturnCtx<(LoadGameDataResult result, UndertaleData? data)> LoadGameData();

        /// <summary>
        ///     Writes the game data back to a data file.
        /// </summary>
        /// <param name="data">The data to write.</param>
        RunnerReturnCtx<WriteGameDataResult> WriteGameData(UndertaleData data);
        
        /// <summary>
        ///     Executes the game using the runner.
        /// </summary>
        /// <returns>The status result and the associated game process. The process will be null if it failed to launch.</returns>
        RunnerReturnCtx<(ExecuteGameResult result, Process? proc)> ExecuteGame();
    }
}