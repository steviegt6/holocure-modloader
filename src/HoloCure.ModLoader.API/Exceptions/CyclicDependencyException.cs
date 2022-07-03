using System;
using System.Collections.Generic;
using System.Linq;

namespace HoloCure.ModLoader.API.Exceptions
{
    // https://github.com/tModLoader/tModLoader/blob/dd44e70738e29e5ded0cb979355b671b41f56efe/patches/tModLoader/Terraria/ModLoader/TopoSort.cs
    // MIT license. TODO: License headers.
    public class CyclicDependencyException<TItem> : Exception
    {
        public readonly HashSet<TItem> Set = new();
        public readonly List<List<TItem>> Cycles = new();
        
        private static string CycleToString(List<TItem> cycle) => "Dependency Cycle: " + string.Join(" -> ", cycle);

        public override string Message => string.Join('\n', Cycles.Select(CycleToString));

        public void Add(List<TItem> cycle) {
            Cycles.Add(cycle);
            foreach (TItem item in cycle) Set.Add(item);
        }
    }
}