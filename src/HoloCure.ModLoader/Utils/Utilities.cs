using System.Linq;

namespace HoloCure.ModLoader.Utils
{
    public static class Utilities
    {
        public static T? FirstNotNull<T>(params T?[] values) where T : class {
            return values.FirstOrDefault(value => value is not null);
        }
    }
}