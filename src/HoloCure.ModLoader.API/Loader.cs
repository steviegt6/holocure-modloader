using System.Collections.Generic;
using System.IO;
using HoloCure.ModLoader.API.Platform;
using HoloCure.ModLoader.Logging;
using HoloCure.ModLoader.Logging.Writers;
using UndertaleModLib;

namespace HoloCure.ModLoader.API
{
    public class Loader
    {
        public readonly string GameName;
        public readonly IStorage Storage;
        public readonly List<string> ModResolutionPaths;

        protected readonly ModAssemblyResolver ModAssemblyResolver;
        protected readonly ILogWriter Logger;

        public Loader(string gameName) {
            GameName = gameName;
            Storage = new GameModStorage(gameName);
            
            Logger = new LogWriter(Path.Combine(Path.GetDirectoryName(Storage.BasePath)!, "Logs", "loader.log"), "Loader");

            ModAssemblyResolver = new ModAssemblyResolver();

            ModResolutionPaths = new List<string>
            {
                Path.Combine(Storage.BasePath, "mods"),
                "mods"
            };

            foreach (string path in ModResolutionPaths) Directory.CreateDirectory(path);
        }

        public void PatchGame(UndertaleData game) {
            Logger.WriteLine("Hello from Loader, patching game...", LogLevels.Debug);
        }
    }
}