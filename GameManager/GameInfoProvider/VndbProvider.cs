using GameManager.DB.Enums;
using GameManager.DB.Models;
using GameManager.Services;
using Helper.Web;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameManager.GameInfoProvider
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    public class VndbProvider(ILogger<VndbProvider> logger, IStaffService staffService) : IGameInfoProvider
    {
        private const string VN_REQUEST_URL = "https://api.vndb.org/kana/vn";
        private const string CHARACTER_REQUEST_URL = "https://api.vndb.org/kana/character";
        private const string RELEASE_REQUEST_URL = "https://api.vndb.org/kana/release";

        private readonly HttpClient _httpClient = RegisterHttpClient();

        public string ProviderName => "VNDB";

        public async Task<(List<GameInfo>? infoList, bool hasMore)> FetchGameSearchListAsync(string searchText,
            int itemPerPage, int pageNum)
        {
            int tryCount = 0;
            while (tryCount < 3)
            {
                string queryString = BuildQueryString(["search", "=", searchText],
                    "id , image.url , titles.title , titles.lang",
                    itemPerPage, pageNum);
                HttpResponseMessage response = await Request(queryString);
                if (response.IsSuccessStatusCode)
                {
                    var gameInfos = new List<GameInfo>();
                    string content = await HttpContentHelper.DecompressContent(response.Content);

                    using var document = JsonDocument.Parse(content);
                    JsonElement rootNode = document.RootElement;
                    bool ok = rootNode.TryGetProperty("results", out JsonElement items);
                    bool hasMore = rootNode.TryGetProperty("more", out JsonElement more) && more.GetBoolean();
                    if (!ok)
                        return ([], false);
                    foreach (JsonElement item in items.EnumerateArray())
                    {
                        string? title = null;
                        if (item.TryGetProperty("titles", out JsonElement titlesProp))
                        {
                            List<TitleModel> titlesDeserialized = titlesProp.Deserialize<List<TitleModel>>() ?? [];
                            title = titlesDeserialized.FirstOrDefault(x => x.Language == "ja")?.Title
                                    ?? titlesDeserialized.FirstOrDefault(x => x.Language == "en")?.Title
                                    ?? titlesDeserialized.FirstOrDefault()?.Title;
                        }

                        string? image = item.GetProperty("image").GetProperty("url").GetString();
                        string? id = item.GetProperty("id").GetString();
                        gameInfos.Add(new GameInfo
                        {
                            GameName = title,
                            GameInfoFetchId = id,
                            CoverPath = image
                        });
                    }

                    return (gameInfos, hasMore);
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    tryCount++;
                    await Task.Delay(1000);
                    continue;
                }

                string message = await HttpContentHelper.DecompressContent(response.Content);

                logger.LogError("Failed to fetch data from VNDB with code : {StatusCode} \n{Message}",
                    response.StatusCode,
                    message);
                throw new Exception($"Failed to fetch data from VNDB with code : {response.StatusCode} \n{message}");
            }

            logger.LogError("Failed to fetch data from VNDB with {TryCount} time retry", tryCount);
            throw new Exception("Failed to fetch data from VNDB");
        }

        public async Task<GameInfo?> FetchGameDetailByIdAsync(string gameId)
        {
            int tryCount = 0;
            while (tryCount < 3)
            {
                string queryString = BuildQueryString(["id", "=", gameId],
                    "id , titles.title , titles.lang , developers.name , image.url , description , released , staff{name,original,role} , extlinks{url , label , name} , screenshots{url}",
                    1);
                HttpResponseMessage response = await Request(queryString);
                logger.LogDebug("Status Code : {StatusCode}", response.StatusCode);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string content = await HttpContentHelper.DecompressContent(response.Content);
                    using var document = JsonDocument.Parse(content);
                    JsonElement rootNode = document.RootElement;
                    bool ok = rootNode.TryGetProperty("results", out JsonElement items);
                    if (!ok)
                        return null;
                    var gameInfo = new GameInfo();
                    foreach (JsonElement item in items.EnumerateArray())
                    {
                        string? title = null, image = null, id = null, description = null;
                        string? enTitle = null, zhTitle = null;
                        if (item.TryGetProperty("titles", out JsonElement titlesProp))
                        {
                            List<TitleModel> titlesDeserialized = titlesProp.Deserialize<List<TitleModel>>() ?? [];
                            title = titlesDeserialized.FirstOrDefault(x => x.Language == "ja")?.Title
                                    ?? titlesDeserialized.FirstOrDefault(x => x.Language == "en")?.Title
                                    ?? titlesDeserialized.FirstOrDefault()?.Title;
                            enTitle = titlesDeserialized.FirstOrDefault(x => x.Language == "en")?.Title;
                            zhTitle = titlesDeserialized.FirstOrDefault(x => x.Language == "zh-Hant")?.Title ??
                                      titlesDeserialized.FirstOrDefault(x => x.Language == "zh-Hans")?.Title;
                        }

                        if (item.TryGetProperty("image", out JsonElement imageProp))
                            if (imageProp.TryGetProperty("url", out JsonElement urlProp))
                                image = urlProp.GetString();
                        if (item.TryGetProperty("id", out JsonElement idProp))
                            id = idProp.GetString();
                        JsonElement develops = item.GetProperty("developers");
                        List<string> developerList = [];
                        foreach (JsonElement developElement in develops.EnumerateArray())
                        {
                            if (developElement.TryGetProperty("name", out JsonElement nameProp))
                                developerList.Add(nameProp.GetString() ?? "" + ",");
                        }

                        developerList.Sort();
                        string develop = string.Join(",", developerList).Trim();

                        if (develop.Last() == ',')
                            develop = develop.Remove(develop.Length - 1);
                        if (item.TryGetProperty("description", out JsonElement descProp))
                            description = descProp.GetString();
                        DateTime? released = null;
                        if (item.TryGetProperty("released", out JsonElement prop))
                        {
                            string? dateString = prop.GetString();
                            released = DateTime.Parse(dateString ?? "");
                        }

                        JsonElement staffElements = item.GetProperty("staff");
                        List<StaffModel> fetchStaffList = staffElements.Deserialize<List<StaffModel>>() ?? [];
                        List<Staff> staffList = await FetchStaffListAsync(fetchStaffList);
                        gameInfo.Staffs = staffList;
                        var tags = staffList.Where(x => x.StaffRole.Id != StaffRoleEnum.STAFF).Select(x => x.Name)
                            .ToList();
                        tags.AddRange(developerList);

                        List<RelatedSite> relatedSites =
                        [
                            new()
                            {
                                Name = "VNDB",
                                Url = $"https://vndb.org/{gameId}"
                            }
                        ];
                        if (item.TryGetProperty("extlinks", out JsonElement extLinksProp) &&
                            extLinksProp.ValueKind == JsonValueKind.Array)
                        {
                            List<RelatedSiteModel> relatedSiteModels =
                                extLinksProp.Deserialize<List<RelatedSiteModel>>() ?? [];
                            relatedSites.AddRange(relatedSiteModels.Select(x => new RelatedSite
                            {
                                Name = x.Label,
                                Url = x.Url
                            }).ToList());
                        }

                        List<string> screenShots = [];
                        if (item.TryGetProperty("screenshots", out JsonElement screenShotsProp) &&
                            screenShotsProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement screenShotProp in screenShotsProp.EnumerateArray())
                            {
                                ScreeShotModel? screenShot = screenShotProp.Deserialize<ScreeShotModel>();
                                if (screenShot != null && !string.IsNullOrWhiteSpace(screenShot.Url))
                                    screenShots.Add(screenShot.Url);
                            }
                        }

                        tags = tags.Distinct().ToList();

                        gameInfo.CoverPath = image;
                        gameInfo.GameName = title;
                        gameInfo.GameChineseName = zhTitle;
                        gameInfo.GameEnglishName = enTitle;
                        gameInfo.GameInfoFetchId = id;
                        gameInfo.Developer = develop;
                        gameInfo.Description = description;
                        gameInfo.ReleaseDate = released;
                        gameInfo.Tags.AddRange(tags.Select(x => new Tag
                        {
                            Name = x
                        }));
                        gameInfo.RelatedSites = relatedSites;
                        gameInfo.ScreenShots = screenShots;

                        try
                        {
                            List<Character> list = await FetchCharacterListAsync(gameId);
                            gameInfo.Characters = list;
                            List<ReleaseInfo> releaseInfos = await FetchReleaseInfoListAsync(gameId);
                            gameInfo.ReleaseInfos = releaseInfos;
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    return gameInfo;
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    tryCount++;
                    await Task.Delay(1000);
                    continue;
                }

                string errorMessage = await HttpContentHelper.DecompressContent(response.Content);

                logger.LogError("Failed to fetch data from VNDB with code : {StatusCode} \n{Message}",
                    response.StatusCode,
                    errorMessage);
                throw new Exception(
                    $"Failed to fetch data from VNDB with code : {response.StatusCode} \n{errorMessage}");
            }

            logger.LogError("Failed to fetch data from VNDB with {TryCount} time retry", tryCount);
            throw new Exception("Failed to fetch data from VNDB");
        }

        private async Task<List<Staff>> FetchStaffListAsync(IEnumerable<StaffModel> fetchStaffs)
        {
            var roleMapping = new Dictionary<string, StaffRole>
            {
                { "scenario", await staffService.GetStaffRoleAsync(StaffRoleEnum.SCENARIO) },
                { "music", await staffService.GetStaffRoleAsync(StaffRoleEnum.MUSIC) },
                { "artist", await staffService.GetStaffRoleAsync(StaffRoleEnum.ARTIST) },
                { "songs", await staffService.GetStaffRoleAsync(StaffRoleEnum.SONG) },
                { "director", await staffService.GetStaffRoleAsync(StaffRoleEnum.DIRECTOR) },
                { "chardesign", await staffService.GetStaffRoleAsync(StaffRoleEnum.CHARACTER_DESIGN) },
                { "staff", await staffService.GetStaffRoleAsync(StaffRoleEnum.STAFF) }
            };
            List<Staff> result = [];
            foreach (StaffModel fetchStaff in fetchStaffs)
            {
                if (!roleMapping.ContainsKey(fetchStaff.Role ?? ""))
                    continue;
                string name = fetchStaff.Original ?? fetchStaff.Name ?? "";
                if (string.IsNullOrEmpty(name))
                    continue;
                StaffRoleEnum roleId = roleMapping[fetchStaff.Role!].Id;
                Staff? staff = await staffService.GetStaffAsync(x => x.Name == name && x.StaffRoleId == roleId);
                if (staff != null)
                    result.Add(staff);
                else
                    result.Add(new Staff
                    {
                        Name = name,
                        StaffRole = roleMapping[fetchStaff.Role!]
                    });
            }

            return result;
        }

        private async Task<List<Character>> FetchCharacterListAsync(string gameId)
        {
            int tryCount = 0;
            while (tryCount < 3)
            {
                string queryString = BuildQueryString([
                        "vn", "=", new List<string>
                        {
                            "id",
                            "=",
                            gameId
                        }
                    ],
                    "id , name , original , aliases, description , image.url , blood_type , age , birthday , sex",
                    20);
                HttpResponseMessage response = await Request(queryString, CHARACTER_REQUEST_URL);
                if (response.IsSuccessStatusCode)
                {
                    string content = await HttpContentHelper.DecompressContent(response.Content);
                    using var document = JsonDocument.Parse(content);
                    JsonElement rootNode = document.RootElement;
                    if (!rootNode.TryGetProperty("results", out JsonElement items) ||
                        items.ValueKind != JsonValueKind.Array)
                        return [];
                    var result = new List<Character>();
                    for (int i = 0; i < items.GetArrayLength(); i++)
                    {
                        CharacterModel? fetchCharacter = items[i].Deserialize<CharacterModel>();
                        if (fetchCharacter == null)
                            continue;
                        string sex = "";
                        if (fetchCharacter.Sex != null)
                        {
                            sex = fetchCharacter.Sex.Count > 1
                                ? fetchCharacter.Sex[1]
                                : fetchCharacter.Sex.Count > 0
                                    ? fetchCharacter.Sex[0]
                                    : "unknown";
                            sex = sex switch
                            {
                                "f" => "FEMALE",
                                "m" => "MALE",
                                "b" => "BOTH",
                                _ => "UNKNOWN"
                            };
                        }

                        int? month = fetchCharacter is { Birthday.Count: > 0 } ? fetchCharacter.Birthday[0] : null;
                        int? day = fetchCharacter is { Birthday.Count: > 1 } ? fetchCharacter.Birthday[1] : null;

                        var character = new Character
                        {
                            Name = fetchCharacter.Name,
                            OriginalName = fetchCharacter.Original,
                            Alias = fetchCharacter.Aliases ?? [],
                            Description = fetchCharacter.Description,
                            ImageUrl = fetchCharacter.Image?.Url,
                            Age = fetchCharacter.Age?.ToString(),
                            Sex = sex,
                            Birthday = month == null || day == null
                                ? null
                                : new DateTime(1, month.Value, day.Value).ToString("yyyy-MM-dd"),
                            BloodType = fetchCharacter.BloodType
                        };
                        result.Add(character);
                    }

                    return result;
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    tryCount++;
                    await Task.Delay(1000);
                    continue;
                }

                string message = await HttpContentHelper.DecompressContent(response.Content);

                logger.LogError("Failed to fetch character from VNDB with code : {StatusCode} \n{Message}",
                    response.StatusCode,
                    message);
                throw new Exception(
                    $"Failed to fetch character from VNDB with code : {response.StatusCode} \n{message}");
            }

            return [];
        }

        private PlatformEnum GetPlatformEnum(string platform)
        {
            return platform switch
            {
                "win" => PlatformEnum.WINDOWS,
                "and" => PlatformEnum.ANDROID,
                "ios" => PlatformEnum.IOS,
                "mac" => PlatformEnum.MACOS,
                "lin" => PlatformEnum.LINUX,
                "nds" => PlatformEnum.NDS,
                _ => PlatformEnum.WINDOWS
            };
        }

        private async Task<List<ReleaseInfo>> FetchReleaseInfoListAsync(string gameId)
        {
            int tryCount = 0;
            while (tryCount < 3)
            {
                string queryString = BuildQueryString([
                        "vn", "=", new List<string>
                        {
                            "id",
                            "=",
                            gameId
                        }
                    ],
                    "id , title , languages.lang , languages.title , platforms , minage , released , extlinks.url , extlinks.label , extlinks.name",
                    20);
                HttpResponseMessage response = await Request(queryString, RELEASE_REQUEST_URL);
                if (response.IsSuccessStatusCode)
                {
                    string content = await HttpContentHelper.DecompressContent(response.Content);
                    using var document = JsonDocument.Parse(content);
                    JsonElement rootNode = document.RootElement;
                    if (!rootNode.TryGetProperty("results", out JsonElement resultsProp) ||
                        resultsProp.ValueKind != JsonValueKind.Array)
                        return [];
                    List<ReleaseModel> fetchReleases = resultsProp.Deserialize<List<ReleaseModel>>() ?? [];
                    List<ReleaseInfo> result = [];
                    foreach (ReleaseModel fetchRelease in fetchReleases)
                    {
                        foreach (ReleaseModel.LangaugeModel language in fetchRelease.Langauges ?? [])
                        {
                            if (string.IsNullOrEmpty(language.Title))
                                continue;
                            if (!DateTime.TryParse(fetchRelease.Released ?? "", out DateTime releaseTime))
                            {
                                if (int.TryParse(fetchRelease.Released, out int year))
                                {
                                    releaseTime = new DateTime(year, 1, 1);
                                }
                            }
                            var releaseInfo = new ReleaseInfo
                            {
                                ReleaseName = language.Title,
                                ReleaseLanguage = fetchRelease.Langauges?.FirstOrDefault()?.Language ?? "",
                                ReleaseDate = releaseTime,
                                Platforms = fetchRelease.Platforms?.Select(GetPlatformEnum).ToList() ?? [],
                                AgeRating = fetchRelease.MinAga ?? 0,
                                ExternalLinks = fetchRelease.ExtLinks?.Select(x => new ExternalLink
                                {
                                    Name = x.Name,
                                    Label = x.Label,
                                    Url = x.Url
                                }).ToList() ?? []
                            };
                            result.Add(releaseInfo);
                        }
                    }

                    return result;
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    tryCount++;
                    await Task.Delay(1000);
                    continue;
                }

                string message = await HttpContentHelper.DecompressContent(response.Content);

                logger.LogError("Failed to fetch release from VNDB with code : {StatusCode} \n{Message}",
                    response.StatusCode,
                    message);
                throw new Exception(
                    $"Failed to fetch release from VNDB with code : {response.StatusCode} \n{message}");
            }

            return [];
        }


        private string BuildQueryString(List<object> filter, string fields, int itemPerPage = 10, int pageNum = 1)
        {
            var query = new
            {
                filters = filter,
                fields,
                sort = "id",
                reverse = false,
                results = itemPerPage,
                page = pageNum,
                user = (object?)null,
                count = false,
                compact_filters = false,
                normalized_filters = false
            };
            return JsonSerializer.Serialize(query);
        }

        private static HttpClient RegisterHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "VNGT");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Accept-Language", "*");
            return client;
        }

        private async Task<HttpResponseMessage> Request(string queryString, string apiUrl = VN_REQUEST_URL)
        {
            HttpRequestMessage request = new(HttpMethod.Post, apiUrl)
            {
                Content = new StringContent(queryString, Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            return response;
        }

        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private class ReleaseModel
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("platforms")]
            public List<string>? Platforms { get; set; }

            [JsonPropertyName("languages")]
            public List<LangaugeModel>? Langauges { get; set; }

            [JsonPropertyName("released")]
            public string? Released { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("minage")]
            public int? MinAga { get; set; }

            [JsonPropertyName("extlinks")]
            public List<ExternalLinkModel>? ExtLinks { get; set; }

            public class LangaugeModel
            {
                [JsonPropertyName("title")]
                public string? Title { get; set; }

                [JsonPropertyName("lang")]
                public string? Language { get; set; }
            }

            public class ExternalLinkModel
            {
                [JsonPropertyName("url")]
                public string? Url { get; set; }

                [JsonPropertyName("label")]
                public string? Label { get; set; }

                [JsonPropertyName("name")]
                public string? Name { get; set; }
            }
        }

        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
        private class CharacterModel
        {
            [JsonPropertyName("birthday")]
            public List<int>? Birthday { get; set; } = [];

            [JsonPropertyName("blood_type")]
            public string? BloodType { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("image")]
            public ImageModel? Image { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("original")]
            public string? Original { get; set; }

            [JsonPropertyName("age")]
            public int? Age { get; set; }

            [JsonPropertyName("sex")]
            public List<string>? Sex { get; set; } = [];

            [JsonPropertyName("aliases")]
            public List<string>? Aliases { get; set; } = [];

            [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
            public class ImageModel
            {
                [JsonPropertyName("url")]
                public string? Url { get; set; }
            }
        }

        private class StaffModel
        {
            [JsonPropertyName("original")]
            public string? Original { get; set; }

            [JsonPropertyName("role")]
            public string? Role { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        private class RelatedSiteModel
        {
            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("label")]
            public string? Label { get; set; }
        }

        private class ScreeShotModel
        {
            [JsonPropertyName("url")]
            public string? Url { get; set; }
        }

        private class TitleModel
        {
            [JsonPropertyName("lang")]
            public string? Language { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }
        }
    }
}