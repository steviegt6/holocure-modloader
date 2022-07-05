using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace HoloCure.ModLoader.Config
{
    public class LaunchConfig
    {
        private static LaunchConfig? UnderlyingConfig;

        public static LaunchConfig Instance {
            get {
                if (UnderlyingConfig is not null) return UnderlyingConfig;
                
                UnderlyingConfig = DeserializeConfig();
                SerializeConfig(UnderlyingConfig);
                return UnderlyingConfig;
            }
        }

        [JsonProperty("check_for_updates")]
        [DefaultValue(true)]
        public bool CheckForUpdates { get; set; }

        [JsonProperty("default_profile")]
        [DefaultValue("")]
        public string? DefaultProfile { get; set; } = null;

        [JsonProperty("profiles")]
        public Dictionary<string, LaunchProfile> Profiles { get; set; } = new();

        public static void SerializeConfig(LaunchConfig config) {
            Directory.CreateDirectory(Program.Storage.BasePath);
            
            File.WriteAllText(
                Path.Combine(Program.Storage.BasePath, "launch_config.json"),
                JsonConvert.SerializeObject(config, Formatting.Indented)
            );
        }

        public static LaunchConfig DeserializeConfig() {
            Directory.CreateDirectory(Program.Storage.BasePath);
            
            string path = Path.Combine(Program.Storage.BasePath, "launch_config.json");
            
            if (!File.Exists(path)) {
                return new LaunchConfig();
            }

            return JsonConvert.DeserializeObject<LaunchConfig>(File.ReadAllText(path)) ?? new LaunchConfig();
        }
    }
}