using UndertaleModLib;

namespace HoloCure.ModLoader.API
{
    public abstract class Mod : IMod
    {
        /// <inheritdoc cref="IMod.PatchGame"/>
        public abstract void PatchGame(UndertaleData gameData);
    }
}