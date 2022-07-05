using System;
using HoloCure.ModLoader.Runners;
using HoloCure.ModLoader.YYTK;
using HoloCure.ModLoader.YYTK.Linux;
using HoloCure.ModLoader.YYTK.MacOS;
using HoloCure.ModLoader.YYTK.Windows;

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

        public static IYYTKLauncher MakePlatformYYTKLauncher() {
            if (OperatingSystem.IsWindows()) return new WindowsYYTKLauncher();
            if (OperatingSystem.IsMacOS()) return new MacOSYYTKLauncher();
            if (OperatingSystem.IsLinux()) return new LinuxYYTKLauncher();

            throw new PlatformNotSupportedException("You are not using an operating system with an existing YYTK launcher.");
        }
    }
}