using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HoloCure.ModLoader.API.Exceptions;
using HoloCure.ModLoader.API.Sorting;

namespace HoloCure.ModLoader.API
{
    // https://github.com/tModLoader/tModLoader/blob/e88e8677f419f9bf7b268cec1e2d3e1b62ea63aa/patches/tModLoader/Terraria/ModLoader/Core/ModOrganizer.cs
    // MIT license. TODO: License headers.
    /// <summary>
    ///     Handles mod organization.
    /// </summary>
    public static class ModOrganizer
    {
        private static ISortStrategy<ModMetadata> TopoSort(ICollection<ModMetadata> mods) {
            Dictionary<string, ModMetadata> nameMap = mods.ToDictionary(x => x.UniqueName);
            return new TopologicalSortStrategy<ModMetadata>(
                mods,
                x => x.SortAfter.Where(nameMap.ContainsKey).Select(y => nameMap[y]),
                x => x.SortBefore.Where(nameMap.ContainsKey).Select(y => nameMap[y])
            );
        }

        public static List<ModMetadata> Sort(IEnumerable<ModMetadata> mods) {
            List<ModMetadata> preSorted = mods.OrderBy(x => x.UniqueName).ToList();
            ISortStrategy<ModMetadata> fullSort = TopoSort(preSorted);

            try {
                return fullSort.Sort();
            }
            catch (CyclicDependencyException<ModMetadata> e) {
                throw new ModOrganizationException(e.Set, e.Message);
            }
        }

        public static void EnsureDependenciesExist(ICollection<ModMetadata> mods) {
            Dictionary<string, ModMetadata> nameMap = mods.ToDictionary(x => x.UniqueName);
            HashSet<ModMetadata> errored = new();
            StringBuilder errorLog = new();

            foreach (ModMetadata mod in mods) {
                foreach (string dep in mod.Dependencies) {
                    if (!nameMap.ContainsKey(dep)) {
                        errored.Add(mod);
                        errorLog.AppendLine($"Missing mod: {dep} required by {mod.UniqueName}");
                    }
                }
            }
            
            if (errored.Count > 0)
                throw new ModOrganizationException(errored, errorLog.ToString());
        }

        public static void EnsureTargetVersionsMet(ICollection<ModMetadata> mods) {
            Dictionary<string, ModMetadata> nameMap = mods.ToDictionary(x => x.UniqueName);
            HashSet<ModMetadata> errored = new();
            StringBuilder errorLog = new();

            foreach (ModMetadata mod in mods) {
                foreach ((string dep, Version depVersion) in mod.CollectDependencies()) {
                    if (!nameMap.TryGetValue(dep, out ModMetadata? inst)) {
                        continue;
                    }

                    if (inst.LiteralVersion() < depVersion) {
                        errored.Add(mod);
                        errorLog.AppendLine(
                            $"{mod.UniqueName} requires version {depVersion}+ of {dep} but version {inst.LiteralVersion()} is installed"
                        );
                    }
                }
            }
            
            if (errored.Count > 0)
                throw new ModOrganizationException(errored, errorLog.ToString());
        }
    }
}