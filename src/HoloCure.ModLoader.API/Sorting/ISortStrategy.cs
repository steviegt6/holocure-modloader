using System.Collections.Generic;

namespace HoloCure.ModLoader.API.Sorting
{
    /// <summary>
    ///     Describes a dependency-aware sort strategy.
    /// </summary>
    /// <typeparam name="TItem">The item type to sort.</typeparam>
    public interface ISortStrategy<TItem>
    {
        void AddEntry(TItem dependency, TItem dependent);

        List<TItem> GetDependencies(TItem item);

        List<TItem> GetDependents(TItem item);

        ISet<TItem> GetAllDependencies(TItem item);
        
        ISet<TItem> GetAllDependents(TItem item);

        List<TItem> Sort();
    }
}