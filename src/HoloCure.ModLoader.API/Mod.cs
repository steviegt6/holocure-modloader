using System.Reflection;
using UndertaleModLib;

namespace HoloCure.ModLoader.API
{
    public class Mod : IMod
    {
        /// <inheritdoc cref="IMod.Assembly"/>
        public Assembly Assembly { get; set; } = null!;
        
        /// <inheritdoc cref="IMod.AssemblyResolver"/>
        public ModAssemblyResolver.Resolver AssemblyResolver { get; set; } = null!;

        /// <inheritdoc cref="IMod.Metadata"/>
        public ModMetadata Metadata { get; set; } = null!;

        /// <inheritdoc cref="IMod.Load"/>
        public virtual void Load() {
        }

        /// <inheritdoc cref="IMod.Unload"/>
        public virtual void Unload() {
        }

        /// <inheritdoc cref="IMod.PatchGame"/>
        public virtual void PatchGame(UndertaleData gameData) {
        }

        /// <inheritdoc cref="IMod.GameStarting"/>
        public virtual void GameStarting() {
        }
    }
}