using GameManager.DB.Enums;
using GameManager.DB.Models;
using GameManager.Models;
using GameManager.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace GameManager.GameInfoProvider
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    public class YmgalInfoProvider(
        IWebService webService,
        ILogger<YmgalInfoProvider> logger,
        IStaffService staffService) : IGameInfoProvider
    {
        private const string BASE_GAME_DETAIL_URL = "https://www.ymgal.games/open/archive";
        private const string BASE_SEARCH_URL = "https://www.ymgal.games/open/archive/search-game";
        private const string GET_TOKEN_URL = "https://www.ymgal.games/oauth/token";

        private string _token = "Bearer";
        public string ProviderName => "YMGalgame";

        public async Task<(List<GameInfo>? infoList, bool hasMore)> FetchGameSearchListAsync(string searchText,
            int itemPerPage, int pageNum)
        {
            HttpClient httpClient = webService.GetDefaultHttpClient();
            try
            {
                while (true)
                {
                    var request =
                        new HttpRequestMessage(HttpMethod.Get, MakeGameListSearchUrl(searchText, pageNum, itemPerPage));
                    request.Headers.Add("Authorization", _token);
                    request.Headers.Add("version", "1");
                    HttpResponseMessage response = httpClient.SendAsync(request).Result;
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Result<string> getTokenResult = GetToken();
                        if (!getTokenResult.Success || string.IsNullOrWhiteSpace(getTokenResult.Value))
                        {
                            return (null, false);
                        }

                        _token = getTokenResult.Value;
                        continue;
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return (null, false);
                    }

                    string responseContent = await response.Content.ReadAsStringAsync();
                    using var jsonDocument = JsonDocument.Parse(responseContent);
                    JsonElement jsonRoot = jsonDocument.RootElement;
                    jsonRoot.TryGetProperty("success", out JsonElement successProp);
                    if (!successProp.GetBoolean())
                    {
                        return (null, false);
                    }

                    if (!jsonRoot.TryGetProperty("data", out JsonElement dataProp))
                    {
                        return (null, false);
                    }

                    if (!dataProp.TryGetProperty("result", out JsonElement resultProp) ||
                        resultProp.ValueKind != JsonValueKind.Array)
                    {
                        return (null, false);
                    }

                    List<GameInfo> gameInfoList = new();
                    foreach (JsonElement element in resultProp.EnumerateArray())
                    {
                        string id = "";
                        string name = "";
                        if (element.TryGetProperty("id", out JsonElement idProp))
                        {
                            id = idProp.GetInt32().ToString();
                        }

                        if (element.TryGetProperty("name", out JsonElement nameProp))
                        {
                            name = nameProp.GetString() ?? "";
                        }

                        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        gameInfoList.Add(new GameInfo
                        {
                            GameInfoFetchId = id,
                            GameName = name
                        });
                    }

                    bool hasMore = false;
                    if (jsonRoot.TryGetProperty("hasNext", out JsonElement hasNextProp))
                    {
                        hasMore = hasNextProp.GetBoolean();
                    }

                    return (gameInfoList, hasMore);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch game search list");
                return (null, false);
            }
        }

        public async Task<GameInfo?> FetchGameDetailByIdAsync(string gameId)
        {
            HttpClient httpClient = webService.GetDefaultHttpClient();
            try
            {
                while (true)
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, MakeGetGameDetailUrl(int.Parse(gameId)));
                    request.Headers.Add("Authorization", _token);
                    request.Headers.Add("version", "1");
                    HttpResponseMessage response = httpClient.SendAsync(request).Result;
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Result<string> getTokenResult = GetToken();
                        if (!getTokenResult.Success || string.IsNullOrWhiteSpace(getTokenResult.Value))
                        {
                            return null;
                        }

                        _token = getTokenResult.Value;
                        continue;
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return null;
                    }

                    string responseContent = await response.Content.ReadAsStringAsync();
                    using var jsonDocument = JsonDocument.Parse(responseContent);
                    JsonElement jsonRoot = jsonDocument.RootElement;
                    jsonRoot.TryGetProperty("success", out JsonElement successProp);
                    if (!successProp.GetBoolean())
                    {
                        return null;
                    }

                    if (!jsonRoot.TryGetProperty("data", out JsonElement dataProp))
                    {
                        return null;
                    }

                    if (!dataProp.TryGetProperty("game", out JsonElement gameProp))
                    {
                        return null;
                    }

                    string introduction = "";
                    string developer = "";
                    string coverUrl = "";
                    List<Tag> tagList = new();
                    if (!gameProp.TryGetProperty("name", out JsonElement nameProp))
                    {
                        throw new Exception("Failed to get game name");
                    }

                    string name = nameProp.GetString() ?? throw new Exception("Failed to get game name");
                    string? chineseName = null;
                    string? englishName = null;
                    if (gameProp.TryGetProperty("chineseName", out JsonElement chineseNameProp))
                    {
                        chineseName = chineseNameProp.GetString();
                    }

                    if (gameProp.TryGetProperty("extensionName", out JsonElement extensionNameProp) &&
                        extensionNameProp.ValueKind == JsonValueKind.Array)
                    {
                        if (extensionNameProp.GetArrayLength() > 0)
                            englishName = extensionNameProp[0].GetProperty("name").GetString();
                    }

                    if (gameProp.TryGetProperty("introduction", out JsonElement introductionProp))
                    {
                        introduction = introductionProp.GetString() ?? "";
                    }

                    if (gameProp.TryGetProperty("developerId", out JsonElement developerProp))
                    {
                        try
                        {
                            int developerId = developerProp.GetInt32();
                            Result<string> getDeveloperResult = await GetDevelopersAsync(developerId);
                            if (getDeveloperResult.Success)
                            {
                                developer = getDeveloperResult.Value ?? "";
                                tagList.Add(new Tag
                                {
                                    Name = developer
                                });
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    var staffList = new List<Staff>();
                    if (gameProp.TryGetProperty("staff", out JsonElement staffProp) &&
                        staffProp.ValueKind == JsonValueKind.Array)
                    {
                        List<StaffModel>? staffModels = staffProp.Deserialize<List<StaffModel>>();
                        if (staffModels != null)
                        {
                            staffList = await GetStaffListAsync(staffModels);
                            tagList.AddRange(staffList.Where(x => x.StaffRole.Id != StaffRoleEnum.STAFF).Select(x =>
                                new Tag
                                {
                                    Name = x.Name
                                }));
                        }
                    }

                    if (gameProp.TryGetProperty("mainImg", out JsonElement mainImgProp))
                    {
                        try
                        {
                            coverUrl = mainImgProp.GetString() ?? "";
                            coverUrl = "cors://" + coverUrl;
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    List<Character> characterList = [];
                    if (dataProp.TryGetProperty("cidMapping", out JsonElement cidMappingProp) &&
                        cidMappingProp.ValueKind == JsonValueKind.Object)
                    {
                        foreach (JsonProperty cidMapping in cidMappingProp.EnumerateObject())
                        {
                            Result<CharacterModel> characterModel =
                                await GetCharacterAsync(Convert.ToInt32(cidMapping.Name));
                            if (!characterModel.Success) continue;
                            CharacterModel? modelValue = characterModel.Value;
                            string sexString = modelValue?.Gender switch
                            {
                                Gender.MALE => "MALE",
                                Gender.FEMALE => "FEMALE",
                                Gender.BOTH => "BOTH",
                                Gender.UNKNOWN => "UNKNOWN",
                                _ => "UNKNOWN"
                            };
                            characterList.Add(new Character
                            {
                                Name = modelValue?.Name,
                                Description = modelValue?.Introduction,
                                Birthday = modelValue?.Birthday,
                                Alias = modelValue?.ExtensionName?.Where(x => x.Name != null).Select(x => x.Name!)
                                    .ToList() ?? [],
                                Sex = sexString,
                                OriginalName = modelValue?.Name,
                                ImageUrl = modelValue?.MainImage == null ? null : "cors://" + modelValue.MainImage
                            });
                        }
                    }

                    List<ReleaseInfo> releaseList = [];
                    if (gameProp.TryGetProperty("releases", out JsonElement releasesProp) &&
                        releasesProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement release in releasesProp.EnumerateArray())
                        {
                            ReleaseModel? releaseModel = release.Deserialize<ReleaseModel>();
                            if (releaseModel == null || string.IsNullOrWhiteSpace(releaseModel.ReleaseName))
                                continue;
                            int ageRating = releaseModel.RestrictionLevel switch
                            {
                                "R18+" => 18,
                                "R18" => 18,
                                "未分级" => 0,
                                _ => 0
                            };
                            if (!DateTime.TryParse(releaseModel.ReleaseDate ?? "", out DateTime releaseDate))
                            {
                                releaseDate = DateTime.MinValue;
                            }

                            var releaseInfo = new ReleaseInfo
                            {
                                ReleaseName = releaseModel.ReleaseName,
                                ReleaseLanguage = releaseModel.ReleaseLanguage ?? "",
                                ReleaseDate = releaseDate,
                                Platforms = [GetPlatformEnum(releaseModel.Platform ?? "")],
                                AgeRating = ageRating,
                                ExternalLinks = string.IsNullOrEmpty(releaseModel.RelatedLink)
                                    ? []
                                    :
                                    [
                                        new ExternalLink
                                        {
                                            Url = releaseModel.RelatedLink,
                                            Name = "website",
                                            Label = "website"
                                        }
                                    ]
                            };
                            releaseList.Add(releaseInfo);
                        }
                    }

                    List<RelatedSite> relatedSites = [];
                    if (gameProp.TryGetProperty("website", out JsonElement websiteProp) &&
                        websiteProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement site in websiteProp.EnumerateArray())
                        {
                            RelatedSiteModel? siteModel = site.Deserialize<RelatedSiteModel>();
                            if (siteModel == null || string.IsNullOrWhiteSpace(siteModel.Title))
                                continue;
                            relatedSites.Add(new RelatedSite
                            {
                                Name = siteModel.Title,
                                Url = siteModel.Link
                            });
                        }
                    }

                    relatedSites.Add(new RelatedSite
                    {
                        Name = "YMGalgame",
                        Url = $"https://www.ymgal.games/GA{gameId}"
                    });

                    DateTime gameReleaseDate = DateTime.MinValue;
                    if (gameProp.TryGetProperty("releaseDate", out JsonElement releaseDateProp))
                    {
                        DateTime.TryParse(releaseDateProp.GetString(), out gameReleaseDate);
                    }

                    var resultGameInfo = new GameInfo
                    {
                        GameInfoFetchId = gameId,
                        GameName = name,
                        ReleaseDate = gameReleaseDate,
                        GameChineseName = chineseName,
                        GameEnglishName = englishName,
                        Description = introduction,
                        Developer = developer,
                        CoverPath = coverUrl,
                        Staffs = staffList,
                        Characters = characterList,
                        ReleaseInfos = releaseList,
                        RelatedSites = relatedSites,
                        ScreenShots = [coverUrl],
                        Tags = tagList
                    };
                    return resultGameInfo;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch game detail");
                return null;
            }
        }

        private PlatformEnum GetPlatformEnum(string platform)
        {
            return platform switch
            {
                "PlayStation Portable" => PlatformEnum.PSP,
                "VNDS" => PlatformEnum.VNDS,
                "Nintendo DS" => PlatformEnum.NDS,
                "Android" => PlatformEnum.ANDROID,
                "Windows" => PlatformEnum.WINDOWS,
                _ => PlatformEnum.WINDOWS
            };
        }

        private async Task<Result<CharacterModel>> GetCharacterAsync(int cid)
        {
            HttpClient httpClient = webService.GetDefaultHttpClient();
            try
            {
                while (true)
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, MakeGetCharacterInfoUrl(cid));
                    request.Headers.Add("Authorization", _token);
                    request.Headers.Add("version", "1");
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Result<string> getTokenResult = GetToken();
                        if (!getTokenResult.Success || string.IsNullOrWhiteSpace(getTokenResult.Value))
                        {
                            return Result<CharacterModel>.Failure("Failed to get token");
                        }

                        _token = getTokenResult.Value;
                        continue;
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return Result<CharacterModel>.Failure("Failed to get character info");
                    }

                    string responseContent = await response.Content.ReadAsStringAsync();
                    using var jsonDocument = JsonDocument.Parse(responseContent);
                    JsonElement jsonRoot = jsonDocument.RootElement;
                    if (!jsonRoot.TryGetProperty("success", out JsonElement successProp) ||
                        successProp.GetBoolean() == false)
                    {
                        return Result<CharacterModel>.Failure("Failed to get character info");
                    }

                    if (!jsonRoot.TryGetProperty("data", out JsonElement dataProp) ||
                        !dataProp.TryGetProperty("character", out JsonElement characterProp))
                        return Result<CharacterModel>.Failure("Failed to get character info");
                    CharacterModel? characterModel = characterProp.Deserialize<CharacterModel>();
                    return characterModel != null
                        ? Result<CharacterModel>.Ok(characterModel)
                        : Result<CharacterModel>.Failure("Failed to get character info");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get developer info");
                return Result<CharacterModel>.Failure("Failed to get character info");
            }
        }

        private async Task<Result<string>> GetDevelopersAsync(int developerId)
        {
            HttpClient httpClient = webService.GetDefaultHttpClient();
            try
            {
                while (true)
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, MakeGetDeveloperInfoUrl(developerId));
                    request.Headers.Add("Authorization", _token);
                    request.Headers.Add("version", "1");
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Result<string> getTokenResult = GetToken();
                        if (!getTokenResult.Success || string.IsNullOrWhiteSpace(getTokenResult.Value))
                        {
                            return Result<string>.Failure("Failed to get token");
                        }

                        _token = getTokenResult.Value;
                        continue;
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return Result<string>.Failure("Failed to get developer info");
                    }

                    string responseContent = await response.Content.ReadAsStringAsync();
                    using var jsonDocument = JsonDocument.Parse(responseContent);
                    JsonElement jsonRoot = jsonDocument.RootElement;
                    jsonRoot.TryGetProperty("success", out JsonElement successProp);
                    if (!successProp.GetBoolean())
                    {
                        return Result<string>.Failure("Failed to get developer info");
                    }

                    if (!jsonRoot.TryGetProperty("data", out JsonElement dataProp))
                    {
                        return Result<string>.Failure("Failed to get developer info");
                    }

                    if (!dataProp.TryGetProperty("org", out JsonElement orgProp))
                    {
                        return Result<string>.Failure("Failed to get developer info");
                    }

                    if (!orgProp.TryGetProperty("name", out JsonElement nameProp))
                    {
                        return Result<string>.Failure("Failed to get developer info");
                    }

                    string name = nameProp.GetString() ?? "";
                    return Result<string>.Ok(name);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get developer info");
                return Result<string>.Failure("Failed to get developer info");
            }
        }

        private static string MakeGetDeveloperInfoUrl(int developerId)
        {
            var builder = new UriBuilder(BASE_GAME_DETAIL_URL);
            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);

            query["orgId"] = developerId.ToString();

            builder.Query = query.ToString();
            return builder.ToString();
        }

        private static string MakeGetCharacterInfoUrl(int cid)
        {
            var builder = new UriBuilder(BASE_GAME_DETAIL_URL);
            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);

            query["cid"] = cid.ToString();

            builder.Query = query.ToString();
            return builder.ToString();
        }

        private Result<string> GetToken()
        {
            HttpClient httpClient = webService.GetDefaultHttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, MakeTokenGetUrl());
            HttpResponseMessage response = httpClient.SendAsync(request).Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return Result<string>.Failure("Failed to get token");
            }

            string content = response.Content.ReadAsStringAsync().Result;
            using var document = JsonDocument.Parse(content);
            JsonElement rootElement = document.RootElement;
            string? accessToken = rootElement.GetProperty("access_token").GetString();
            return string.IsNullOrWhiteSpace(accessToken)
                ? Result<string>.Failure("Failed to get token")
                : Result<string>.Ok("Bearer " + accessToken);
        }

        private static string MakeTokenGetUrl()
        {
            var builder = new UriBuilder(GET_TOKEN_URL);
            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);

            query["grant_type"] = "client_credentials";
            query["client_id"] = "ymgal";
            query["client_secret"] = "luna0327";
            query["scope"] = "public";

            builder.Query = query.ToString();
            return builder.ToString();
        }

        private static string MakeGetGameDetailUrl(int gameId)
        {
            var builder = new UriBuilder(BASE_GAME_DETAIL_URL);
            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);

            query["gid"] = gameId.ToString();

            builder.Query = query.ToString();
            return builder.ToString();
        }

        private static string MakeGameListSearchUrl(string searchText, int pageNum, int pageSize)
        {
            var builder = new UriBuilder(BASE_SEARCH_URL);
            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);

            query["keyword"] = searchText;
            query["mode"] = "list";
            query["pageNum"] = pageNum.ToString();
            query["pageSize"] = pageSize.ToString();

            builder.Query = query.ToString();
            return builder.ToString();
        }

        private async Task<List<Staff>> GetStaffListAsync(IEnumerable<StaffModel> staffModels)
        {
            var roleMapping = new Dictionary<string, StaffRole>
            {
                { "脚本", await staffService.GetStaffRoleAsync(StaffRoleEnum.SCENARIO) },
                { "音乐", await staffService.GetStaffRoleAsync(StaffRoleEnum.MUSIC) },
                { "歌曲", await staffService.GetStaffRoleAsync(StaffRoleEnum.SONG) },
                { "原画", await staffService.GetStaffRoleAsync(StaffRoleEnum.ARTIST) },
                { "导演/监督", await staffService.GetStaffRoleAsync(StaffRoleEnum.DIRECTOR) },
                { "人物设计", await staffService.GetStaffRoleAsync(StaffRoleEnum.CHARACTER_DESIGN) },
                { "其他", await staffService.GetStaffRoleAsync(StaffRoleEnum.STAFF) }
            };
            List<Staff> result = [];
            foreach (StaffModel fetchStaff in staffModels)
            {
                if (!roleMapping.ContainsKey(fetchStaff.JobName ?? ""))
                    continue;
                string name = fetchStaff.Name ?? "";
                if (string.IsNullOrEmpty(name))
                    continue;
                StaffRoleEnum roleId = roleMapping[fetchStaff.JobName!].Id;
                Staff? staff = await staffService.GetStaffAsync(x => x.Name == name && x.StaffRole.Id == roleId);
                if (staff != null)
                    result.Add(staff);
                else
                    result.Add(new Staff
                    {
                        Name = name,
                        StaffRole = roleMapping[fetchStaff.JobName!]
                    });
            }

            return result;
        }

        private enum Gender
        {
            UNKNOWN = 0,
            MALE = 1,
            FEMALE = 2,
            BOTH = 3
        }

        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
        private class CharacterModel
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("introduction")]
            public string? Introduction { get; set; }

            [JsonPropertyName("mainImg")]
            public string? MainImage { get; set; }

            [JsonPropertyName("gender")]
            public Gender Gender { get; set; } = Gender.UNKNOWN;

            [JsonPropertyName("extensionName")]
            public List<ExtensionNameModel>? ExtensionName { get; set; }

            [JsonPropertyName("birthday")]
            public string? Birthday { get; set; }

            public class ExtensionNameModel
            {
                [JsonPropertyName("name")]
                public string? Name { get; set; }
            }
        }

        private class StaffModel
        {
            [JsonPropertyName("empName")]
            public string? Name { get; set; }

            [JsonPropertyName("jobName")]
            public string? JobName { get; set; }
        }

        private class ReleaseModel
        {
            [JsonPropertyName("releaseName")]
            public string? ReleaseName { get; set; }

            [JsonPropertyName("releaseDate")]
            public string? ReleaseDate { get; set; }

            [JsonPropertyName("relatedLink")]
            public string? RelatedLink { get; set; }

            [JsonPropertyName("platform")]
            public string? Platform { get; set; }

            [JsonPropertyName("releaseLanguage")]
            public string? ReleaseLanguage { get; set; }

            [JsonPropertyName("restrictionLevel")]
            public string? RestrictionLevel { get; set; }
        }

        private class RelatedSiteModel
        {
            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("link")]
            public string? Link { get; set; }
        }
    }
}