using System.Threading.Tasks;

namespace HoloCure.ModLoader.Updater
{
    /// <summary>
    ///     Implementable interface that dictates whether a program may update and how updating should be handled.
    /// </summary>
    public interface IProgramUpdatable
    {
        /// <summary>
        ///     Whether this program may update.
        /// </summary>
        bool CanUpdate();

        /// <summary>
        ///     Verify if this program has an available update.
        /// </summary>
        /// <param name="version">The version to check.</param>
        Task<bool> CheckUpdate(string version);
    }
}