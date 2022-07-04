using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HoloCure.ModLoader.Updater.JSON;
using Newtonsoft.Json;

namespace HoloCure.ModLoader.Updater
{
    public abstract class GitHubProgramUpdatable : IProgramUpdatable
    {
        public abstract string GitHubReleaseUrl { get; }

        public virtual string UserAgent => "HoloCure.ModLoader.Updater Client";

        public abstract bool CanUpdate();

        public virtual GitHubRelease? Release { get; protected set; }

        public virtual async Task<bool> CheckUpdate(string version) {
            HttpResponseMessage? resp;

            try {
                HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                resp = await client.GetAsync(GitHubReleaseUrl);
            }
            catch (HttpRequestException) {
                return false;
            }
            catch (TaskCanceledException) {
                return false;
            }

            GitHubRelease? latestRelease;

            try {
                string json = await resp.Content.ReadAsStringAsync();
                latestRelease = JsonConvert.DeserializeObject<GitHubRelease>(json);
            }
            catch (JsonReaderException) {
                return false;
            }

            if (latestRelease is null) return false;
            if (version == latestRelease.TagName) return false;

            Release = latestRelease;
            return true;
        }
    }
}