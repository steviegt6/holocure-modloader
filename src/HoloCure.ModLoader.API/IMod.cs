using System.Reflection;
using DogScepterLib.Core;

namespace HoloCure.ModLoader.API
{
    /// <summary>
    ///     Represents a loadable mod.
    /// </summary>
    public interface IMod
    {
        /// <summary>
        ///     This mod's assembly.
        /// </summary>
        Assembly Assembly { get; set; }

        /// <summary>
        ///     This mod's assembly resolver.
        /// </summary>
        ModAssemblyResolver.Resolver AssemblyResolver { get; set; }

        /// <summary>
        ///     This mod's metadata file.
        /// </summary>
        ModMetadata Metadata { get; set; }

        /// <summary>
        ///     Called once this mod has been loaded.
        /// </summary>
        void Load();

        /// <summary>
        ///     Called when this mod is about to be unloaded.
        /// </summary>
        void Unload();
        
        /// <summary>
        ///     Invoked when the game is ready to be patched.
        /// </summary>
        /// <param name="gameData">The loaded game's data, which should be patched.</param>
        void PatchGame(GMData gameData);

        /// <summary>
        ///     Executed once the game begins to start.
        /// </summary>
        void GameStarting();
    }
}