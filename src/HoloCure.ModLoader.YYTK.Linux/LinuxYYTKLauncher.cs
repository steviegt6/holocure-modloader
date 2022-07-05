using System;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace HoloCure.ModLoader.YYTK.Linux
{
    public class LinuxYYTKLauncher : IYYTKLauncher
    {
        public Process? StartPreloaded(string runnerPath, string? gamePath, string yytkDll) {
            if (!OperatingSystem.IsLinux())
                throw new PlatformNotSupportedException("LinuxYYTKLauncher only supports Linux, use the appropriate YYTK launcher.");

            return StartPreloadedInner(runnerPath, gamePath, yytkDll);
        }

        public string? GetYYTKDllPath(Type hostType) {
            return YYTKUtils.BruteForceSearch("YYToolkit-linux", "YYToolkit.so", hostType);
        }

        [SupportedOSPlatform("linux")]
        protected Process? StartPreloadedInner(string runnerPath, string? gamePath, string yytkDll) {
            throw new PlatformNotSupportedException("Linux does not yet have YYTK support.");
        }
    }
}