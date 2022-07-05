using System.Diagnostics;
using HoloCure.ModLoader.YYTK;
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
        ///     Restore backup data if it was not cleared upon exiting due to an error.
        /// </summary>
        RunnerReturnCtx<RestoreLeftOverBackupDataResult> RestoreLeftoverBackupData();

        /// <summary>
        ///     Backs up the data file by writing it to disk.
        /// </summary>
        RunnerReturnCtx<BackupDataResult> BackupData();

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
        /// <param name="yytkLauncher">Platform-dependant YYTK launcher..</param>
        /// <returns>The status result and the associated game process. The process will be null if it failed to launch.</returns>
        RunnerReturnCtx<(ExecuteGameResult result, Process? proc)> ExecuteGame(IYYTKLauncher yytkLauncher);

        RunnerReturnCtx<RestoreBackupDataResult> RestoreBackupData();
    }
}