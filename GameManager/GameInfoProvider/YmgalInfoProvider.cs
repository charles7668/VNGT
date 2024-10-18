using GameManager.DB.Models;
using GameManager.Models;
using GameManager.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Web;

namespace GameManager.GameInfoProvider
{
    public class YmgalInfoProvider(IWebService webService, ILogger<YmgalInfoProvider> logger) : IGameInfoProvider
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
                            GameInfoId = id,
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

                    if (gameProp.TryGetProperty("staff", out JsonElement staffProp) &&
                        staffProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement staffElement in staffProp.EnumerateArray())
                        {
                            if (!staffElement.TryGetProperty("empName", out JsonElement staffNameProp)) continue;
                            string staffName = staffNameProp.GetString() ?? "";
                            if (string.IsNullOrWhiteSpace(staffName))
                                continue;
                            tagList.Add(new Tag
                            {
                                Name = staffName
                            });
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

                    var resultGameInfo = new GameInfo
                    {
                        GameInfoId = gameId,
                        GameName = name,
                        Description = introduction,
                        Developer = developer,
                        CoverPath = coverUrl,
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
    }
}