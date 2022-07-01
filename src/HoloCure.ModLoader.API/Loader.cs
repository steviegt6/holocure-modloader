using System.Collections.Generic;
using System.IO;
using HoloCure.ModLoader.API.Platform;
using UndertaleModLib;

namespace HoloCure.ModLoader.API
{
    public class Loader
    {
        public readonly string GameName;
        public readonly IStorage Storage;
        public readonly List<string> ModResolutionPaths;

        protected readonly ModAssemblyResolver ModAssemblyResolver;

        public Loader(string gameName) {
            GameName = gameName;
            Storage = new GameModStorage(gameName);

            ModAssemblyResolver = new ModAssemblyResolver();

            ModResolutionPaths = new List<string>
            {
                Path.Combine(Storage.BasePath, "mods"),
                "mods"
            };

            foreach (string path in ModResolutionPaths) Directory.CreateDirectory(path);
        }

        public void PatchGame(UndertaleData game) {
        }
    }
}