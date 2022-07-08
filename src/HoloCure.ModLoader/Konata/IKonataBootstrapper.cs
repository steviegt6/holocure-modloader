using System.Diagnostics;
using System.Threading.Tasks;

namespace HoloCure.ModLoader.Konata
{
    /// <summary>
    ///     Launches the game process using the associated Konata.X bootstrapper.
    /// </summary>
    public interface IKonataBootstrapper
    {
        public Task<Process?> StartGame(string runnerPath, string gamePath);
    }
}