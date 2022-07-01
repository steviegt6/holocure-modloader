using System;
using System.IO;

namespace HoloCure.ModLoader.API.Platform
{
    public class WindowsStorage : IStorage
    {
        public string BasePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "holocure-modloader");
    }
}