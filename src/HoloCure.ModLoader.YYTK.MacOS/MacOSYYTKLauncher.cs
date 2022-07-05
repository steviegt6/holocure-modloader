using System;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace HoloCure.ModLoader.YYTK.MacOS
{
    public class MacOSYYTKLauncher : IYYTKLauncher
    {
        public Process? StartPreloaded(string runnerPath, string? gamePath, string yytkDll) {
            if (!OperatingSystem.IsMacOS())
                throw new PlatformNotSupportedException("MacOSYYTKLauncher only supports MacOS, use the appropriate YYTK launcher.");

            return StartPreloadedInner(runnerPath, gamePath, yytkDll);
        }
        
        public string? GetYYTKDllPath(Type hostType) {
            return YYTKUtils.BruteForceSearch("YYToolkit-macos", "YYToolkit.dylib", hostType);
        }

        [SupportedOSPlatform("macos")]
        protected Process? StartPreloadedInner(string runnerPath, string? gamePath, string yytkDll) {
            throw new PlatformNotSupportedException("MacOS does not yet have YYTK support.");
        }
    }
}