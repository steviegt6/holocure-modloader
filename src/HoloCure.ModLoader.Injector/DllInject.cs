using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using HoloCure.ModLoader.Injector.Windows;

// Adapted from https://github.com/Archie-osu/YYToolkit/blob/stable/YYLauncher/Utils.cs
namespace HoloCure.ModLoader.Injector
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
        public static void InjectIntoProcess(Process proc, string dllPath) {
            InjectWithHandle(proc.Handle, dllPath);
        }

        /// <summary>
        ///     Injects into a process given its handle.
        /// </summary>
        /// <param name="handle">The process handle to use for injection.</param>
        /// <param name="dllPath">The path to the DLL to inject into the process.</param>
        public static void InjectWithHandle(IntPtr handle, string dllPath) {
            if (OperatingSystem.IsWindows()) InjectWithHandleWindows(handle, dllPath);
            else throw new PlatformNotSupportedException("Native DLL injection is currently only supported on Windows.");
        }

        [SupportedOSPlatform("windows")]
        private static void InjectWithHandleWindows(IntPtr handle, string dllPath) {
            uint dwSize = (uint) ((dllPath.Length + 1) * Marshal.SizeOf(typeof(char)));
            uint nsize = (uint) ((dllPath.Length + 1) * Marshal.SizeOf(typeof(char)));
            IntPtr pLoadLib = WindowsApiMethods.GetProcAddress(WindowsApiMethods.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            IntPtr pRemoteString = WindowsApiMethods.VirtualAllocEx(handle, IntPtr.Zero, dwSize, 12288u, 4u);

            WindowsApiMethods.WriteProcessMemory(handle, pRemoteString, Encoding.Default.GetBytes(dllPath), nsize, out _);
            WindowsApiMethods.CreateRemoteThread(handle, IntPtr.Zero, 0u, pLoadLib, pRemoteString, 0u, IntPtr.Zero);
        }

        /// <summary>
        ///     Starts a process and injects into it prematurely.
        /// </summary>
        /// <param name="processPath">The process to start.</param>
        /// <param name="args">Any launch arguments to go with it.</param>
        /// <param name="dllPath">The path to the DLL to inject into the process.</param>
        /// <returns>The started and injected process. Null if the process failed to start.</returns>
        public static Process? StartInjected(string processPath, string args, string dllPath) {
            if (OperatingSystem.IsWindows()) return StartInjectedWindows(processPath, args, dllPath);
            throw new PlatformNotSupportedException("Native DLL injection is currently only supported on Windows.");
        }
        
        [SupportedOSPlatform("windows")]
        private static Process? StartInjectedWindows(string processPath, string args, string dllPath) {
            // WindowsApiTypes.PROCESS_INFORMATION pInfo;
            // WindowsApiTypes.STARTUPINFO sInfo = new();
            // WindowsApiTypes.SECURITY_ATTRIBUTES pSec = new();
            // WindowsApiTypes.SECURITY_ATTRIBUTES tSec = new();
            // pSec.nLength = Marshal.SizeOf(pSec);
            // tSec.nLength = Marshal.SizeOf(tSec);
            // 
            // string? wd = Path.GetDirectoryName(processPath);
            // 
            // if (wd is null) {
            //     return null;
            // }
            // 
            // bool didStart = WindowsApiMethods.CreateProcess(
            //     processPath,
            //     args,
            //     ref pSec,
            //     ref tSec,
            //     false,
            //     4, /* CREATE_SUSPENDED */
            //     IntPtr.Zero,
            //     wd,
            //     ref sInfo,
            //     out WindowsApiTypes.PROCESS_INFORMATION pInfo
            // );
            // 
            // if (!didStart) {
            //     return null;
            // }
            // 
            // ProcessStartInfo startInfo = new(processPath, args)
            // {
            //     WorkingDirectory = wd,
            //     UseShellExecute = false
            // };
            // 
            // Process test = new();
            // test.StartInfo = startInfo;
            // 
            // // Process? proc = Process.GetProcesses().SingleOrDefault(x => x.Id == WindowsApiMethods.GetProcessId(pInfo.hProcess));
            // 
            // // InjectWithHandle(pInfo.hProcess, dllPath);
            // InjectWithHandle(test.Handle, dllPath);
            // 
            // bool success = test.Start();
            // return success ? test : null;
            
            WindowsApiTypes.PROCESS_INFORMATION pInfo = new();
            WindowsApiTypes.STARTUPINFO sInfo = new();
            WindowsApiTypes.SECURITY_ATTRIBUTES pSec = new();
            WindowsApiTypes.SECURITY_ATTRIBUTES tSec = new();
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);
            
            WindowsApiMethods.CreateProcess(processPath, args, ref pSec, ref tSec, false, 4, IntPtr.Zero, Path.GetDirectoryName(processPath), ref sInfo, out pInfo);
            
            InjectWithHandle(pInfo.hProcess, dllPath);

            Process? proc = Process.GetProcesses().SingleOrDefault(x => x.Id == WindowsApiMethods.GetProcessId(pInfo.hProcess));
            return proc;
        }
    }
}