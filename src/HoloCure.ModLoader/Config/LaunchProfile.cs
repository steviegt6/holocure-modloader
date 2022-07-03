using System.ComponentModel;
using Newtonsoft.Json;

namespace HoloCure.ModLoader.Config
{
    public class LaunchProfile
    {
        [JsonProperty("game_path")]
        [DefaultValue("")]
        public string GamePath { get; set; } = "";

        [JsonProperty("backup_path")]
        [DefaultValue("")]
        public string BackupPath { get; set; } = "";

        [JsonProperty("runner_path")]
        [DefaultValue("")]
        public string RunnerPath { get; set; } = "";
    }
}