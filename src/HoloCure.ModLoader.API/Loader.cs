using HoloCure.ModLoader.API.Platform;

namespace HoloCure.ModLoader.API
{
    public class Loader
    {
        public readonly string GameName;
        public readonly IStorage Storage;

        protected readonly ModAssemblyResolver ModAssemblyResolver;

        public Loader(string gameName) {
            GameName = gameName;
            Storage = new GameModStorage(gameName);

            ModAssemblyResolver = new ModAssemblyResolver();
        }
    }
}