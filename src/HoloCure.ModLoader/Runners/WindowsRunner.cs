using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using HoloCure.ModLoader.Konata;
using UndertaleModLib;

namespace HoloCure.ModLoader.Runners
{
    /// <summary>
    ///     Runs a Windows, non-YYC, GMS2 game.
    /// </summary>
    public class WindowsRunner : IRunner
    {
        protected readonly string GamePath;
        protected readonly string BackupPath;
        protected string? RunnerPath;
        
        public WindowsRunner(string? gamePath = null, string? backupPath = null, string? runnerPath = null) {
            GamePath = gamePath ?? "data.win";
            BackupPath = backupPath ?? "data.win.bak";
            RunnerPath = runnerPath;
        }
        
        public RunnerReturnCtx<bool> DataExists() {
            return MakeReturnCtx(File.Exists(GamePath));
        }

        public RunnerReturnCtx<RestoreLeftOverBackupDataResult> RestoreLeftoverBackupData() {
            if (!File.Exists(BackupPath)) {
                return MakeReturnCtx(RestoreLeftOverBackupDataResult.Skipped);
            }

            try {
                File.Copy(BackupPath, GamePath, true);
            }
            catch (UnauthorizedAccessException) {
                return MakeReturnCtx(RestoreLeftOverBackupDataResult.PermissionError);
            }

            return MakeReturnCtx(RestoreLeftOverBackupDataResult.Success);
        }

        public RunnerReturnCtx<BackupDataResult> BackupData() {
            if (File.Exists(BackupPath)) {
                return MakeReturnCtx(BackupDataResult.Skipped);
            }

            if (!File.Exists(GamePath)) {
                return MakeReturnCtx(BackupDataResult.MissingFile);
            }

            // TODO: More comprehensive checking for file copying. Things like handling directory errors, etc.
            try {
                File.Copy(GamePath, BackupPath, true);
            }
            catch (UnauthorizedAccessException) {
                return MakeReturnCtx(BackupDataResult.PermissionError);
            }

            return MakeReturnCtx(BackupDataResult.Success);
        }

        public RunnerReturnCtx<(LoadGameDataResult result, UndertaleData? data)> LoadGameData() {
            RunnerReturnCtx<(LoadGameDataResult result, UndertaleData? data)> Ctx(LoadGameDataResult result, UndertaleData? data) {
                return MakeReturnCtx((result, data));
            }
            
            if (!File.Exists(GamePath)) {
                return Ctx(LoadGameDataResult.MissingFile, null);
            }

            try {
                using Stream stream = new FileStream(GamePath, FileMode.Open, FileAccess.Read);
                // TODO: Handle exceptions thrown during reading
                return Ctx(LoadGameDataResult.Success, UndertaleIO.Read(stream));
            }
            catch (UnauthorizedAccessException) {
                return Ctx(LoadGameDataResult.PermissionError, null);
            }
        }

        public RunnerReturnCtx<WriteGameDataResult> WriteGameData(UndertaleData data) {
            // If runner path not specified, *finally* determine the path after the data has been patched.
            // Automatic detection assumes the runner is in the same path as the game data file, and uses the naming scheme "[GameName].exe".
            // Obviously will differ between platforms.
            RunnerPath ??= Path.Combine(Path.GetDirectoryName(GamePath) ?? "", Path.ChangeExtension(data.GeneralInfo.Name.Content, ".exe"));
            
            try {
                using Stream stream = new FileStream(GamePath, FileMode.Create, FileAccess.Write);
                UndertaleIO.Write(stream, data);
                return MakeReturnCtx(WriteGameDataResult.Success);
            }
            catch (UnauthorizedAccessException) {
                return MakeReturnCtx(WriteGameDataResult.PermissionError);
            }
        }

        public async Task<RunnerReturnCtx<(ExecuteGameResult result, Process? proc)>> ExecuteGame(IKonataBootstrapper bootstrapper) {
            RunnerReturnCtx<(ExecuteGameResult result, Process? proc)> Ctx(ExecuteGameResult result, Process? proc) {
                return MakeReturnCtx((result, proc));
            }

            if (RunnerPath is null) {
                return Ctx(ExecuteGameResult.RunnerNull, null);
            }

            string fullPath = Path.GetFullPath(RunnerPath);

            if (!File.Exists(fullPath)) {
                return Ctx(ExecuteGameResult.RunnerMissing, null);
            }

            Process? proc = await bootstrapper.StartGame(RunnerPath, GamePath);

            return Ctx(proc is null ? ExecuteGameResult.ProcessNull : ExecuteGameResult.Success, proc);
        }

        public RunnerReturnCtx<RestoreBackupDataResult> RestoreBackupData() {
            if (!File.Exists(BackupPath)) {
                return MakeReturnCtx(RestoreBackupDataResult.MissingFile);
            }

            try {
                File.Copy(BackupPath, GamePath, true);
                File.Delete(BackupPath);
            }
            catch (UnauthorizedAccessException) {
                return MakeReturnCtx(RestoreBackupDataResult.PermissionError);
            }

            return MakeReturnCtx(RestoreBackupDataResult.Success);
        }

        protected RunnerReturnCtx<T> MakeReturnCtx<T>(T value) {
            return new RunnerReturnCtx<T>(value, GamePath, BackupPath, RunnerPath);
        }
    }
}