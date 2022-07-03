using System.Reflection;
using UndertaleModLib;

namespace HoloCure.ModLoader.API
{
    public class Mod : IMod
    {
        /// <inheritdoc cref="IMod.Assembly"/>
        public Assembly? Assembly { get; set; }
        
        /// <inheritdoc cref="IMod.AssemblyResolver"/>
        public ModAssemblyResolver.Resolver? AssemblyResolver { get; set; }

        /// <inheritdoc cref="IMod.Metadata"/>
        public ModMetadata? Metadata { get; set; }

        /// <inheritdoc cref="IMod.Load"/>
        public virtual void Load() {
        }

        /// <inheritdoc cref="IMod.Unload"/>
        public virtual void Unload() {
        }

        /// <inheritdoc cref="IMod.PatchGame"/>
        public virtual void PatchGame(UndertaleData gameData) {
        }

        /// <inheritdoc cref="IMod.GameStarted"/>
        public virtual void GameStarted() {
        }

        // For tests. Kinda guh...
        public override string ToString() {
            return Metadata!.UniqueName;
        }
    }
}