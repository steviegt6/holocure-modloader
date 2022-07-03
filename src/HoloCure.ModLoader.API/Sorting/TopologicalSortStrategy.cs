using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HoloCure.ModLoader.API.Exceptions;

namespace HoloCure.ModLoader.API.Sorting
{
    // https://github.com/tModLoader/tModLoader/blob/dd44e70738e29e5ded0cb979355b671b41f56efe/patches/tModLoader/Terraria/ModLoader/TopoSort.cs
    // MIT license. TODO: License headers.
    public class TopologicalSortStrategy<TItem> : ISortStrategy<TItem>
    {
        protected readonly ReadOnlyCollection<TItem> List;
        protected Dictionary<TItem, List<TItem>> Dependencies = new();
        protected Dictionary<TItem, List<TItem>> Dependents = new();

        public TopologicalSortStrategy(
            IEnumerable<TItem> elements,
            Func<TItem, IEnumerable<TItem>>? dependencies = null,
            Func<TItem, IEnumerable<TItem>>? dependents = null
        ) {
            List = elements.ToList().AsReadOnly();

            if (dependencies is not null) {
                foreach (TItem item in List) {
                    foreach (TItem dependency in dependencies(item))
                        AddEntry(dependency, item);
                }
            }
            
            if (dependents is not null) {
                foreach (TItem item in List) {
                    foreach (TItem dependent in dependents(item))
                        AddEntry(item, dependent);
                }
            }
        }

        public void AddEntry(TItem dependency, TItem dependent) {
            if (!Dependencies.TryGetValue(dependent, out List<TItem>? list)) {
                Dependencies[dependent] = list = new List<TItem>();
            }
            list.Add(dependency);

            if (!Dependents.TryGetValue(dependency, out list)) {
                Dependents[dependency] = list = new List<TItem>();
            }
            list.Add(dependent);
        }

        public List<TItem> GetDependencies(TItem item) {
            return Dependencies.TryGetValue(item, out List<TItem>? list) ? list : new List<TItem>();
        }

        public List<TItem> GetDependents(TItem item) {
            return Dependents.TryGetValue(item, out List<TItem>? list) ? list : new List<TItem>();
        }

        public ISet<TItem> GetAllDependencies(TItem item) {
            HashSet<TItem> set = new();
            BuildSet(item, Dependencies, set);
            return set;
        }

        public ISet<TItem> GetAllDependents(TItem item) {
            HashSet<TItem> set = new();
            BuildSet(item, Dependents, set);
            return set;
        }

        public List<TItem> Sort() {
            CyclicDependencyException<TItem> exception = new();
            Stack<TItem> visiting = new();
            List<TItem> sorted = new();

            Action<TItem>? visit = null;
            visit = item =>
            {
                if (sorted.Contains(item) || exception.Set.Contains(item)) {
                    return;
                }
                
                visiting.Push(item);
                foreach (TItem dependency in GetDependencies(item)) {
                    if (visiting.Contains(dependency)) {
                        List<TItem> cycle = new();
                        cycle.Add(dependency);
                        cycle.AddRange(visiting.TakeWhile(x => !EqualityComparer<TItem>.Default.Equals(x, dependency)));
                        cycle.Add(dependency);
                        cycle.Reverse();
                        exception.Add(cycle);
                        continue;
                    }

                    visit?.Invoke(dependency);
                }

                visiting.Pop();
                sorted.Add(item);
            };

            foreach (TItem item in List) {
                visit(item);
            }

            if (exception.Set.Count > 0) throw exception;

            return sorted;
        }

        private static void BuildSet(TItem item, Dictionary<TItem, List<TItem>> dict, HashSet<TItem> set) {
            if (!dict.TryGetValue(item, out List<TItem>? list)) {
                return;
            }

            foreach (TItem entry in dict[item]) {
                if (set.Add(entry)) {
                    BuildSet(entry, dict, set);
                }
            }
        }
    }
}