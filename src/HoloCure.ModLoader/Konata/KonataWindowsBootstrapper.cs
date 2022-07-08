using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace HoloCure.ModLoader.Konata
{
    /// <summary>
    ///     Boostraps a game using Konata.Windows to load YYTK on Windows platforms.
    /// </summary>
    public class KonataWindowsBootstrapper : IKonataBootstrapper
    {
        public async Task<Process?> StartGame(string runnerPath, string gamePath) {
            /*string? yytoolkitDllpath = yytkLauncher.GetYYTKDllPath();
            if (yytoolkitDllpath is not null) {
                proc = yytkLauncher.StartPreloaded(RunnerPath, GamePath, yytoolkitDllpath);
            }
            else {
                ProcessStartInfo info = new(fullPath, $"-game \"{GamePath}\"")
                {
                    UseShellExecute = true
                };
                
                proc = Process.Start(info);
            }*/

            string? exePath = GetKonataExecutable();
            string? yytkDll = GetYYTKDll();

            if (exePath is null || yytkDll is null) {
                return null;
            }
            
            string stdout = "-1";

            Process bootstrap = new();
            bootstrap.StartInfo.FileName = exePath;
            bootstrap.StartInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
            bootstrap.StartInfo.Arguments = $"\"{runnerPath}\" \"{gamePath}\" \"{yytkDll}\"";
            bootstrap.StartInfo.UseShellExecute = false;
            bootstrap.StartInfo.RedirectStandardOutput = true;
            bootstrap.StartInfo.RedirectStandardError = true;
            bootstrap.OutputDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data)) stdout = args.Data;
                Console.WriteLine(args.Data);
            };
            bootstrap.ErrorDataReceived += (_, args) => { Console.WriteLine(args.Data); };
            bootstrap.Start();
            bootstrap.BeginOutputReadLine();
            bootstrap.BeginErrorReadLine();

            await bootstrap.WaitForExitAsync();

            if (!int.TryParse(stdout, out int procId)) {
                return null;
            }

            return Process.GetProcessById(procId);
        }

        protected static string? GetKonataExecutable() {
            string? IfExists(string path) {
                return File.Exists(path) ? Path.GetFullPath(path) : null;
            }

            return IfExists(Path.Combine("Konata.Windows", "Konata.Windows.exe"));
        }

        protected static string? GetYYTKDll() {
            string? IfExists(string path) {
                return File.Exists(path) ? Path.GetFullPath(path) : null;
            }

            return IfExists(Path.Combine("Konata.Windows", "YYToolkit", "YYToolkit.dll"));
        }
    }
}