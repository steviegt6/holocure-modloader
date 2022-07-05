using System;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace HoloCure.ModLoader.YYTK.Windows
{
    public class WindowsYYTKLauncher : IYYTKLauncher
    {
        public Process? StartPreloaded(string runnerPath, string? gamePath, string yytkDll) {
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("WindowsYYTKLauncher only supports Windows, use the appropriate YYTK launcher.");
            
            return StartPreloadedInner(runnerPath, gamePath, yytkDll);
        }
        
        public string? GetYYTKDllPath(Type hostType) {
            return YYTKUtils.BruteForceSearch("YYToolkit-windows", "YYToolkit.dll", hostType);
        }

        [SupportedOSPlatform("windows")]
        protected Process? StartPreloadedInner(string runnerPath, string? gamePath, string yytkDll) {
            return DllInject.StartInjected(runnerPath, gamePath == null ? "" : $"-game \"{gamePath}\"", yytkDll);
        }
    }
}