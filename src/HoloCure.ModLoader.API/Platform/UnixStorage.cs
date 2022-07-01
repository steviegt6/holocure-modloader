using System;
using System.IO;

namespace HoloCure.ModLoader.API.Platform
{
    public class UnixStorage : IStorage
    {
        public string BasePath {
            get {
                static string GetBasePath() {
                    string? xdgPath = Environment.GetEnvironmentVariable("XDG_DATA_HOME");

                    if (string.IsNullOrEmpty(xdgPath))
                        return Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                            ".local",
                            "share"
                        );

                    return xdgPath;
                }

                return Path.Combine(GetBasePath(), "holocure-modloader");
            }
        }
    }
}