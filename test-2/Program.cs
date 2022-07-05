using System.Runtime.InteropServices;
using System.Diagnostics;

static void Print(object msg)
{
    Console.WriteLine(msg);
}

static void Inject(Process p, string pathToDLL)
{
    IntPtr pLoadLib = WinAPI.GetProcAddress(WinAPI.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
    IntPtr pRemoteString = WinAPI.VirtualAllocEx(p.Handle, IntPtr.Zero, (uint)((pathToDLL.Length + 1) * Marshal.SizeOf(typeof(char))), 12288u, 4u);

    WinAPI.WriteProcessMemory(p.Handle, pRemoteString, System.Text.Encoding.Default.GetBytes(pathToDLL), (uint)((pathToDLL.Length + 1) * Marshal.SizeOf(typeof(char))), out var _);
    WinAPI.CreateRemoteThread(p.Handle, IntPtr.Zero, 0u, pLoadLib, pRemoteString, 0u, IntPtr.Zero);
}

static void InjectH(IntPtr Handle, string pathToDLL)
{
    IntPtr pLoadLib = WinAPI.GetProcAddress(WinAPI.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
    IntPtr pRemoteString = WinAPI.VirtualAllocEx(Handle, IntPtr.Zero, (uint)((pathToDLL.Length + 1) * Marshal.SizeOf(typeof(char))), 12288u, 4u);

    WinAPI.WriteProcessMemory(Handle, pRemoteString, System.Text.Encoding.Default.GetBytes(pathToDLL), (uint)((pathToDLL.Length + 1) * Marshal.SizeOf(typeof(char))), out var _);
    WinAPI.CreateRemoteThread(Handle, IntPtr.Zero, 0u, pLoadLib, pRemoteString, 0u, IntPtr.Zero);
}

static void StartPreloaded(string sRunnerFilePath, string sDataFilePath, string sPathToDll)
{
    WinAPI.PROCESS_INFORMATION pInfo = new WinAPI.PROCESS_INFORMATION();
    WinAPI.STARTUPINFO sInfo = new WinAPI.STARTUPINFO();
    WinAPI.SECURITY_ATTRIBUTES pSec = new WinAPI.SECURITY_ATTRIBUTES();
    WinAPI.SECURITY_ATTRIBUTES tSec = new WinAPI.SECURITY_ATTRIBUTES();
    pSec.nLength = Marshal.SizeOf(pSec);
    tSec.nLength = Marshal.SizeOf(tSec);

    bool Success = false;
    if (String.IsNullOrEmpty(sDataFilePath))
    {
        Success = WinAPI.CreateProcess(sRunnerFilePath, "", ref pSec, ref tSec, false,
        4 /* CREATE_SUSPENDED */, IntPtr.Zero, Path.GetDirectoryName(sRunnerFilePath), ref sInfo, out pInfo);
    }
    else
    {
        Success = WinAPI.CreateProcess(sRunnerFilePath, $"-game \"{sDataFilePath}\"", ref pSec, ref tSec, false,
        4 /* CREATE_SUSPENDED */, IntPtr.Zero, Path.GetDirectoryName(sRunnerFilePath), ref sInfo, out pInfo);
    }

    if (!Success)
    {
        Print("Failed to create a process.\nGetLastError() returned " + Marshal.GetLastWin32Error().ToString());
        return;
    }

    InjectH(pInfo.hProcess, sPathToDll);
}

static void WaitUntilWindow(Process process)
{
    process.WaitForInputIdle();

    while (!process.HasExited)
    {
        try
        {
            if (process.MainWindowHandle != IntPtr.Zero)
                break;
        }
        catch (InvalidOperationException)
        {
            if (!process.HasExited)
                throw;
        }
    }

    process.WaitForInputIdle();
}

const string sRunnerFilePath = @"C:\Users\xxlen\Downloads\HoloCure-modded\HoloCure.exe";
const string sDataFilePath = @"C:\Users\xxlen\Downloads\HoloCure-modded\data.win";
const string YYTKPath = @"C:\Users\xxlen\OneDrive\Documents\Repositories\holocure-modloader\test-2\YYToolkit.dll";

Print(sRunnerFilePath);
Print(sDataFilePath);
Print(YYTKPath);

Print("Y = Pre, anything else = Post");

if (Console.ReadKey(true).KeyChar == 'y') await Pre();
else await Post();

static async Task Pre()
{
    Print("pre");

    StartPreloaded(sRunnerFilePath, sDataFilePath, YYTKPath);
}

static async Task Post()
{
    Print("post");

    Process[] ExistingProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(sRunnerFilePath));

    if (ExistingProcesses.Length > 0)
    {
        // There might be several apps named the same, running from a different path?
        foreach (Process process in ExistingProcesses)
        {
            if (!process.MainModule.FileName.Equals(sRunnerFilePath))
                continue;

            Print($"A runner process with PID {process.Id} already exists.\nInject into it?\n");
            if (Console.ReadKey().KeyChar != 'y')
                break;

            // TODO: Check if YYTK's already injected
            WaitUntilWindow(process);

            Inject(process, YYTKPath);
            return;
        }
    }

    Process p = Process.Start(sRunnerFilePath, string.IsNullOrEmpty(sDataFilePath) ? "" : $"-game \"{sDataFilePath}\"");

    WaitUntilWindow(p);

    Inject(p, YYTKPath);

    await p.WaitForExitAsync();
}

public class WinAPI
{
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct STARTUPINFO
    {
        public Int32 cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public Int32 dwX;
        public Int32 dwY;
        public Int32 dwXSize;
        public Int32 dwYSize;
        public Int32 dwXCountChars;
        public Int32 dwYCountChars;
        public Int32 dwFillAttribute;
        public Int32 dwFlags;
        public Int16 wShowWindow;
        public Int16 cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public int bInheritHandle;
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool CreateProcess(
       string lpApplicationName,
       string lpCommandLine,
       ref SECURITY_ATTRIBUTES lpProcessAttributes,
       ref SECURITY_ATTRIBUTES lpThreadAttributes,
       bool bInheritHandles,
       uint dwCreationFlags,
       IntPtr lpEnvironment,
       string lpCurrentDirectory,
       [In] ref STARTUPINFO lpStartupInfo,
       out PROCESS_INFORMATION lpProcessInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint ResumeThread(IntPtr hThread);
}