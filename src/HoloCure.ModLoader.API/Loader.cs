using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using HoloCure.ModLoader.API.Platform;
using HoloCure.ModLoader.Logging;
using HoloCure.ModLoader.Logging.Writers;
using Newtonsoft.Json;
using UndertaleModLib;

[assembly: InternalsVisibleTo("HoloCure.ModLoader.API.Tests")]

namespace HoloCure.ModLoader.API
{
    public class Loader
    {
        public const string MANIFEST_FILE_NAME = "manifest.json";
        
        public readonly string GameName;
        public readonly IStorage Storage;
        public readonly List<string> ModResolutionPaths;

        protected readonly ModAssemblyResolver ModAssemblyResolver;
        protected readonly ILogWriter Logger;
        protected List<ModMetadata> ModMetadataList = new();
        protected List<IMod> Mods = new();

        public ReadOnlyCollection<IMod> ReadonlyMods => Mods.AsReadOnly();

        public Loader(string gameName, IEnumerable<string> externalProbingPaths) {
            GameName = gameName;
            Storage = new GameModStorage(gameName);

            Logger = new LogWriter(Path.Combine(Path.GetDirectoryName(Storage.BasePath)!, "Logs", "loader.log"), "Loader", true);

            ModAssemblyResolver = new ModAssemblyResolver();

            ModResolutionPaths = new List<string>
            {
                Path.Combine(Storage.BasePath, "mods"),
            };
            ModResolutionPaths.AddRange(externalProbingPaths);

            foreach (string path in ModResolutionPaths) Directory.CreateDirectory(path);
        }

        public void ResolveMods() {
            Logger.LogMessage("Resolving mods...", LogLevels.Debug);
            List<DirectoryInfo> directories = new();
            
            foreach (string path in ModResolutionPaths) {
                directories.AddRange(new DirectoryInfo(path).EnumerateDirectories());
            }
            
            Logger.LogMessage($"Found {directories.Count} directories to search for mods in.", LogLevels.Debug);
            
            foreach (DirectoryInfo directory in directories) {
                string fullName = directory.FullName;
                string manifestPath = Path.Combine(fullName, MANIFEST_FILE_NAME);
                Logger.LogMessage($"Searching for mods in {fullName}...", LogLevels.Debug);

                if (!File.Exists(manifestPath)) {
                    Logger.LogMessage($"No manifest file found in {fullName}.", LogLevels.Warn);
                    continue;
                }

                ModMetadata? metadata = null;
                Exception? exception = null;

                try {
                    metadata = JsonConvert.DeserializeObject<ModMetadata>(manifestPath);
                }
                catch (Exception e) {
                    exception = e;
                }

                if (metadata is null) {
                    Logger.LogMessage("Invalid manifest file found in " + fullName + ".", LogLevels.Error);

                    if (exception is not null) {
                        Logger.LogMessage("The accompanying error was thrown during deserialization: " + exception, LogLevels.Error);
                    }

                    continue;
                }

                string dllPath = Path.Combine(fullName, metadata.DllName);

                if (!File.Exists(dllPath)) {
                    Logger.LogMessage($"The specified DLL file {metadata.DllName} was not present in {fullName}.", LogLevels.Error);
                    continue;
                }
                
                ModMetadataList.Add(metadata);
            }
        }

        public void SortMods() {
            Logger.LogMessage("Ensuring all mod dependencies are present...", LogLevels.Debug);
            ModOrganizer.EnsureDependenciesExist(ModMetadataList);
            
            Logger.LogMessage("Ensuring target dependency versions are met...", LogLevels.Debug);
            ModOrganizer.EnsureTargetVersionsMet(ModMetadataList);
            
            Logger.LogMessage("Sorting mods...", LogLevels.Debug);
            ModMetadataList = ModOrganizer.Sort(ModMetadataList);
        }

        public void InstantiateMods() {
            Logger.LogMessage("Creating mod resolvers...", LogLevels.Debug);

            foreach (ModMetadata metadata in ModMetadataList) {
                ModAssemblyResolver.AddAssembly(metadata);
            }
            
            Logger.LogMessage("Populating mod resolver dependencies...", LogLevels.Debug);
            ModAssemblyResolver.PopulateResolverDependencies();
            
            Logger.LogMessage("Subscribing mod resolvers...", LogLevels.Debug);
            ModAssemblyResolver.SubscribeAll();
            
            Logger.LogMessage("Instantiating mods from loaded assemblies...", LogLevels.Debug);
            Mods.AddRange(ModAssemblyResolver.InstantiateMods());
        }

        public void LoadMods() {
            Logger.LogMessage("Loading mods...", LogLevels.Debug);
            foreach (IMod mod in Mods) {
                mod.Load();
            }
        }

        public void UnloadMods() {
            Logger.LogMessage("Unloading mods...", LogLevels.Debug);
            foreach (IMod mod in Mods) {
                mod.Unload();
            }
            
            Logger.LogMessage("Unsubscribing mod resolvers...", LogLevels.Debug);
            ModAssemblyResolver.UnsubscribeAll();
        }

        public void PatchGame(UndertaleData game) {
            Logger.LogMessage($"Patching game with {Mods.Count} loaded mods...", LogLevels.Debug);

            foreach (IMod mod in Mods) {
                mod.PatchGame(game);
            }
        }

        public void GameStarted() {
            Logger.LogMessage("Received game started event, alerting mods...", LogLevels.Debug);
            
            foreach (IMod mod in Mods) {
                mod.GameStarted();
            }
        }
    }
}