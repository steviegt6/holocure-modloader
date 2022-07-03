using System.Linq;

namespace HoloCure.ModLoader.Utils
{
    public static class Utilities
    {
        public static string? GetUsableString(params string?[] values) {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        }
    }
}