using System;
using System.IO;

namespace HoloCure.ModLoader.API.Platform
{
    public class GameModStorage : IStorage
    {
        protected readonly IStorage BaseStorage;
        protected readonly string GameName;

        public string BasePath => Path.Combine(BaseStorage.BasePath, GameName);
        
        public GameModStorage(IStorage baseStorage, string gameName) {
            BaseStorage = baseStorage;
            GameName = gameName;
        }
        
        public GameModStorage(string gameName) : this(ResolveStorage(), gameName) { }
        
        public static IStorage ResolveStorage() {
            if (OperatingSystem.IsWindows()) return new WindowsStorage();
            if (OperatingSystem.IsMacOS()) return new MacStorage();
            if (OperatingSystem.IsLinux()) return new LinuxStorage();
            
            throw new PlatformNotSupportedException("Only Windows, Mac OS, and Linux operating systems are supported. Paths won't work otherwise!");
        }
    }
}