using System.Reflection;
using System.Text.Json;

namespace GameManager.Services
{
    public class VersionService : IVersionService
    {
        private string? _newestVersionCache;

        public async Task<string?> DetectNewestVersion()
        {
            if (_newestVersionCache != null)
                return _newestVersionCache;
            try
            {
                HttpClient client = new()
                {
                    DefaultRequestHeaders =
                    {
                        { "User-Agent", "VNGT" }
                    }
                };
                const string releaseUrl = "https://api.github.com/repos/charles7668/VNGT/releases";
                Version? currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                if (currentVersion == null)
                    return null;
                string response = await client.GetStringAsync(releaseUrl, CancellationToken.None);
                JsonElement releaseInfos = JsonSerializer.Deserialize<JsonElement>(response);
                int releaseCount = releaseInfos.GetArrayLength();
                if (releaseCount == 0)
                    return null;
                if (!releaseInfos[0].TryGetProperty("name", out JsonElement title) ||
                    string.IsNullOrWhiteSpace(title.GetString()))
                    return null;
                var version = new Version(title.GetString()!);
                if (version.Major > currentVersion.Major ||
                    (version.Major == currentVersion.Major && version.Minor > currentVersion.Minor) ||
                    (version.Major == currentVersion.Major && version.Minor == currentVersion.Minor &&
                     version.Build > currentVersion.Build))
                {
                    return version.ToString();
                }

                return null;
            }
            finally
            {
                _newestVersionCache ??= "";
            }
        }
    }
}