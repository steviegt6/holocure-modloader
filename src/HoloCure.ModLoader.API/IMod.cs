using UndertaleModLib;

namespace HoloCure.ModLoader.API
{
    /// <summary>
    ///     Represents a loadable mod.
    /// </summary>
    public interface IMod
    {
        /// <summary>
        ///     Invoked when the game is ready to be patched.
        /// </summary>
        /// <param name="gameData">The loaded game's data, which should be patched.</param>
        void PatchGame(UndertaleData gameData);
    }
}