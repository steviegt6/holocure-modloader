using HoloCure.ModLoader.Konata;

namespace HoloCure.ModLoader.Utils
{
    public static class OperatingSystemUtils
    {
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