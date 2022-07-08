using System;
using System.Diagnostics;
using System.IO;

namespace Konata.Windows
{
    public static class Program
    {
        // 0 = runner path
        // 1 = data path
        // 2 = yytk path
        public static void Main(string[] args) {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Konata.Windows exclusively supports Windows.");

            if (args.Length != 3) throw new ArgumentException("Three arguments not provided.");

            foreach (string path in args) {
                if (!File.Exists(path)) {
                    throw new FileNotFoundException($"File does not exist at: {path}");
                }
            }

            Process? proc = DllInject.StartInjected(args[0], $"-game \"{args[1]}\"", args[2]);
            
            Console.WriteLine(proc?.Id ?? -1);
        }
    }
}