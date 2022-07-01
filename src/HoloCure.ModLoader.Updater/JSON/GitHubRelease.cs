using Newtonsoft.Json;

namespace HoloCure.ModLoader.Updater.JSON
{
    public class GitHubRelease
    {
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; } = null!;

        [JsonProperty("tag_name")]
        public string TagName { get; set; } = null!;

        [JsonProperty("assets")]
        public GitHubAsset[] Assets { get; set; } = null!;
    }
}