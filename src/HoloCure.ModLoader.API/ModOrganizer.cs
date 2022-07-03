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
        private static ISortStrategy<IMod> TopoSort(ICollection<IMod> mods) {
            Dictionary<string, IMod> nameMap = mods.ToDictionary(x => x.Metadata!.UniqueName);
            return new TopologicalSortStrategy<IMod>(
                mods,
                x => x.Metadata!.SortAfter.Where(nameMap.ContainsKey).Select(y => nameMap[y]),
                x => x.Metadata!.SortBefore.Where(nameMap.ContainsKey).Select(y => nameMap[y])
            );
        }

        public static List<IMod> Sort(IEnumerable<IMod> mods) {
            List<IMod> preSorted = mods.OrderBy(x => x.Metadata!.UniqueName).ToList();
            ISortStrategy<IMod> fullSort = TopoSort(preSorted);

            try {
                return fullSort.Sort();
            }
            catch (CyclicDependencyException<IMod> e) {
                throw new ModOrganizationException(e.Set, e.Message);
            }
        }

        public static void EnsureDependenciesExist(ICollection<IMod> mods) {
            Dictionary<string, IMod> nameMap = mods.ToDictionary(x => x.Metadata!.UniqueName);
            HashSet<IMod> errored = new();
            StringBuilder errorLog = new();

            foreach (IMod mod in mods) {
                foreach (string dep in mod.Metadata!.Dependencies) {
                    if (!nameMap.ContainsKey(dep)) {
                        errored.Add(mod);
                        errorLog.AppendLine($"Missing mod: {dep} required by {mod.Metadata!.UniqueName}");
                    }
                }
            }
            
            if (errored.Count > 0)
                throw new ModOrganizationException(errored, errorLog.ToString());
        }

        public static void EnsureTargetVersionsMet(ICollection<IMod> mods) {
            Dictionary<string, IMod> nameMap = mods.ToDictionary(x => x.Metadata!.UniqueName);
            HashSet<IMod> errored = new();
            StringBuilder errorLog = new();

            foreach (IMod mod in mods) {
                foreach ((string dep, Version depVersion) in mod.Metadata!.CollectDependencies()) {
                    if (!nameMap.TryGetValue(dep, out IMod? inst)) {
                        continue;
                    }

                    if (inst.Metadata!.LiteralVersion() < depVersion) {
                        errored.Add(mod);
                        errorLog.AppendLine(
                            $"{mod.Metadata!.UniqueName} requires version {depVersion}+ of {dep} but version {inst.Metadata!.LiteralVersion()} is installed"
                        );
                    }
                }
            }
            
            if (errored.Count > 0)
                throw new ModOrganizationException(errored, errorLog.ToString());
        }
    }
}