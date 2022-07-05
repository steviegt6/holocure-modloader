using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

// Adapted from https://github.com/Archie-osu/YYToolkit/blob/stable/YYLauncher/Utils.cs
namespace HoloCure.ModLoader.YYTK.Windows
{
    /// <summary>
    ///     Utilities for injecting a native DLL into a process.
    /// </summary>
    public static class DllInject
    {
        /// <summary>
        ///     Injects into the given process.
        /// </summary>
        /// <param name="proc">The process to inject into.</param>
        /// <param name="dllPath">The path to the DLL to inject into the process.</param>
        [SupportedOSPlatform("windows")]
        public static void InjectIntoProcess(Process proc, string dllPath) {
            InjectWithHandle(proc.Handle, dllPath);
        }

        /// <summary>
        ///     Injects into a process given its handle.
        /// </summary>
        /// <param name="handle">The process handle to use for injection.</param>
        /// <param name="dllPath">The path to the DLL to inject into the process.</param>
        [SupportedOSPlatform("windows")]
        public static void InjectWithHandle(IntPtr handle, string dllPath) {
            IntPtr pLoadLib = WindowsApiMethods.GetProcAddress(
                WindowsApiMethods.GetModuleHandle("kernel32.dll"),
                "LoadLibraryA"
            );
            
            IntPtr pRemoteString = WindowsApiMethods.VirtualAllocEx(
                handle,
                IntPtr.Zero,
                (uint) ((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))),
                12288u,
                4u
            );

            WindowsApiMethods.WriteProcessMemory(
                handle,
                pRemoteString,
                Encoding.Default.GetBytes(dllPath),
                (uint) ((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))),
                out _
            );
            
            WindowsApiMethods.CreateRemoteThread(
                handle,
                IntPtr.Zero,
                0u,
                pLoadLib,
                pRemoteString,
                0u,
                IntPtr.Zero
            );
        }

        /// <summary>
        ///     Starts a process and injects into it prematurely.
        /// </summary>
        /// <param name="processPath">The process to start.</param>
        /// <param name="args">Any launch arguments to go with it.</param>
        /// <param name="dllPath">The path to the DLL to inject into the process.</param>
        /// <returns>The started and injected process. Null if the process failed to start.</returns>
        [SupportedOSPlatform("windows")]
        public static Process? StartInjected(string processPath, string args, string dllPath) {
            string? wd = Path.GetDirectoryName(processPath);

            if (wd is null) {
                return null;
            }
            
            WindowsApiTypes.PROCESS_INFORMATION pInfo = new();
            WindowsApiTypes.STARTUPINFO sInfo = new();
            WindowsApiTypes.SECURITY_ATTRIBUTES pSec = new();
            WindowsApiTypes.SECURITY_ATTRIBUTES tSec = new();
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            WindowsApiMethods.CreateProcess(
                processPath,
                args,
                ref pSec,
                ref tSec,
                false,
                4, /* CREATE_SUSPENDED */
                IntPtr.Zero,
                wd,
                ref sInfo,
                out pInfo
            );

            InjectWithHandle(pInfo.hProcess, dllPath);


            return Process.GetProcesses().SingleOrDefault(x => x.Id == WindowsApiMethods.GetProcessId(pInfo.hProcess));
        }
    }
}