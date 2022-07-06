using System;
using HoloCure.ModLoader.Runners;
using HoloCure.ModLoader.YYTK;

namespace HoloCure.ModLoader.Utils
{
    public static class OperatingSystemUtils
    {
        public static IRunner MakePlatformRunner(string? gamePath = null, string? backupPath = null, string? runnerPath = null) {
            if (OperatingSystem.IsWindows()) return new WindowsRunner(gamePath, backupPath, runnerPath);
            
            throw new PlatformNotSupportedException(
                "Currently, only Windows operating systems are supported."
                + "\nCompatibility with other operating systems through Wine, BootCamp, or similar means may be possible."
                + "\nIf you would like to investigate this, write up an issue on https://github.com/steviegt6/holocure-modloader/issues."
                + "\n\nThere is also always the option to explicitly implement support for other operating systems."
            );
        }

        public static IYYTKLauncher GetYYTKLauncher() {
#if WINDOWS
            return new YYTK.Windows.WindowsYYTKLauncher();
#elif MACOS
            return new YYTK.MacOS.MacOSYYTKLauncher();
#elif LINUX
            return new YYTK.Linux.LinuxYYTKLauncher();
#else
            throw new PlatformNotSupportedException("The given operating system does not support YYTK.");
#endif
        }
    }
}