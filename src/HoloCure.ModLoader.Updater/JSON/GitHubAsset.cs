using Newtonsoft.Json;

namespace HoloCure.ModLoader.Updater.JSON
{
    public class GitHubAsset
    {
        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = null!;
    }
}