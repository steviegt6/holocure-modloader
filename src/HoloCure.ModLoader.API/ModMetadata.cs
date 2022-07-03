using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace HoloCure.ModLoader.API
{
    public class ModMetadata
    {
        [JsonProperty("unique_name")]
        public string UniqueName { get; set; } = "";

        [JsonProperty("version")]
        public string Version { get; set; } = "";

        [JsonProperty("dependencies")]
        public List<string> Dependencies { get; set; } = new();

        [JsonProperty("sort_before")]
        public List<string> SortBefore { get; set; } = new();

        [JsonProperty("sort_after")]
        public List<string> SortAfter { get; set; } = new();

        public IEnumerable<(string dep, Version depVersion)> CollectDependencies() {
            foreach (string dep in Dependencies) {
                string[] split = dep.Split('@', 2);

                if (split.Length == 1) {
                    yield return (dep, new Version(0, 0, 0, 0));
                }

                yield return (split[0], System.Version.Parse(split[1]));
            }
        }

        public Version LiteralVersion() {
            return System.Version.Parse(Version);
        }
    }
}