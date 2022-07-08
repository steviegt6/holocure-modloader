using HoloCure.ModLoader.Konata;
using HoloCure.ModLoader.Runners;

namespace HoloCure.ModLoader.Utils
{
    public static class OperatingSystemUtils
    {
        public static IRunner MakePlatformRunner(string? gamePath = null, string? backupPath = null, string? runnerPath = null) {
#if WINDOWS
            return new WindowsRunner(gamePath, backupPath, runnerPath); 
#elif MACOS
            throw new System.PlatformNotSupportedException("MacOS does not have a supported runner."
                + "\nIf you would like to contribute, you may open a ticket or create a pr @ https://github.com/steviegt6/holocure-modloader/"
                + "\nCurrently, non-Windows support is relied on entirely by other contributors, so anything is appreciated!"
            );
#elif LINUX
            throw new System.PlatformNotSupportedException("Linux does not have a supported runner."
                + "\nIf you would like to contribute, you may open a ticket or create a pr @ https://github.com/steviegt6/holocure-modloader/"
                + "\nCurrently, non-Windows support is relied on entirely by other contributors, so anything is appreciated!"
            );
#else
            throw new System.PlatformNotSupportedException("Your operating system does not have a supported runner."
                + "\nIf you would like to contribute, you may open a ticket or create a pr @ https://github.com/steviegt6/holocure-modloader/"
                + "\nCurrently, non-Windows support is relied on entirely by other contributors, so anything is appreciated!"
            );
#endif
        }

        public static IKonataBootstrapper GetKonataBootstrapper() {
#if WINDOWS
            return new KonataWindowsBootstrapper();
#elif MACOS
            throw new System.PlatformNotSupportedException("YYTK does not support MacOS."); // return new YYTK.MacOS.MacOSYYTKLauncher();
#elif LINUX
            throw new System.PlatformNotSupportedException("YYTK does not support Linux."); // return new YYTK.Linux.LinuxYYTKLauncher();
#else
            throw new System.PlatformNotSupportedException("YYTK does not support the given operating system.");
#endif
        }
    }
}