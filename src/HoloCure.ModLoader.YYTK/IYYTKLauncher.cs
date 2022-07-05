using System;
using System.Diagnostics;

namespace HoloCure.ModLoader.YYTK
{
    // TODO: Post-load injection.
    /// <summary>
    ///     Represents an object that can start and inject a DLL into a process.
    /// </summary>
    public interface IYYTKLauncher
    {
        /// <summary>
        ///     Starts and suspends a process in order to inject the DLL.
        /// </summary>
        /// <param name="runnerPath">The path to the runner.</param>
        /// <param name="gamePath">The path to the game file.</param>
        /// <param name="yytkDll">The path to the DLL to inject.</param>
        /// <returns>The started process.</returns>
        Process? StartPreloaded(string runnerPath, string? gamePath, string yytkDll);

        /// <summary>
        ///     Gets the DLL path of the platform YYTK DLL.
        /// </summary>
        /// <param name="hostType">Yeah.</param>
        /// <returns>The absolute path.</returns>
        string? GetYYTKDllPath(Type hostType);
    }
}