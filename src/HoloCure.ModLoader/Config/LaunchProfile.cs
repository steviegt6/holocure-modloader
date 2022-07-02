using Newtonsoft.Json;

namespace HoloCure.ModLoader.Config
{
    public class LaunchProfile
    {
        [JsonProperty("game_path")]
        public string? GamePath { get; set; } = null;

        [JsonProperty("backup_path")]
        public string? BackupPath { get; set; } = null;

        [JsonProperty("runner_path")]
        public string? RunnerPath { get; set; } = null;
    }
}