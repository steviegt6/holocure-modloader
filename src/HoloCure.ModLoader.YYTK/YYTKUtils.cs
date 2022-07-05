using System;
using System.IO;

namespace HoloCure.ModLoader.YYTK
{
    public static class YYTKUtils
    {
        public static string? BruteForceSearch(string folder, string dllName, Type hostType) {
            string implicitCwd = Path.Combine(folder, dllName);
            string explicitCwd = Path.Combine(Environment.CurrentDirectory, folder, dllName);
            string assemblyDir = Path.Combine(hostType.Assembly.Location, folder, dllName);

            string? AsFile(string path) {
                return File.Exists(path) ? path : null;
            }

            string? path = AsFile(implicitCwd) ?? AsFile(explicitCwd) ?? AsFile(assemblyDir);
            return path is not null ? Path.GetFullPath(path) : null;
        }
    }
}