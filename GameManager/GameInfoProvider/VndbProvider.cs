using GameManager.DB.Models;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GameManager.GameInfoProvider
{
    public class VndbProvider(ILogger<VndbProvider> logger) : IGameInfoProvider
    {
        private const string VN_REQUEST_URL = "https://api.vndb.org/kana/vn";

        private readonly HttpClient _httpClient = RegisterHttpClient();

        public string ProviderName { get; } = "VNDB";

        public async Task<(List<GameInfo>? infoList, bool hasMore)> FetchGameSearchListAsync(string searchText,
            int itemPerPage, int pageNum)
        {
            int tryCount = 0;
            while (tryCount < 3)
            {
                string queryString = BuildQueryString(["search", "=", searchText], "id , image.url , titles.title",
                    itemPerPage, pageNum);
                HttpResponseMessage response = await Request(queryString);
                if (response.IsSuccessStatusCode)
                {
                    var gameInfos = new List<GameInfo>();
                    string content = await UnzipAsync(response.Content);

                    using var document = JsonDocument.Parse(content);
                    JsonElement rootNode = document.RootElement;
                    bool ok = rootNode.TryGetProperty("results", out JsonElement items);
                    bool hasMore = rootNode.TryGetProperty("more", out JsonElement more) && more.GetBoolean();
                    if (!ok)
                        return ([], false);
                    foreach (JsonElement item in items.EnumerateArray())
                    {
                        string? title = item.GetProperty("titles")[0].GetProperty("title").GetString();
                        string? image = item.GetProperty("image").GetProperty("url").GetString();
                        string? id = item.GetProperty("id").GetString();
                        gameInfos.Add(new GameInfo
                        {
                            GameName = title,
                            GameInfoId = id,
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

                string message = await UnzipAsync(response.Content);

                throw new Exception($"Failed to fetch data from VNDB with code : {response.StatusCode} \n{message}");
            }

            throw new Exception("Failed to fetch data from VNDB");
        }

        public async Task<GameInfo?> FetchGameDetailByIdAsync(string gameId)
        {
            int tryCount = 0;
            while (tryCount < 3)
            {
                string queryString = BuildQueryString(["id", "=", gameId],
                    "id , titles.title , developers.name , image.url , description , released", 1);
                HttpResponseMessage response = await Request(queryString);
                logger.LogDebug("Status Code : {StatusCode}", response.StatusCode);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string content = await UnzipAsync(response.Content);
                    using var document = JsonDocument.Parse(content);
                    JsonElement rootNode = document.RootElement;
                    bool ok = rootNode.TryGetProperty("results", out JsonElement items);
                    if (!ok)
                        return null;
                    var gameInfo = new GameInfo();

                    foreach (JsonElement item in items.EnumerateArray())
                    {
                        string? title = null, image = null, id = null, description = null;
                        if (item.TryGetProperty("titles", out JsonElement titles))
                            title = titles[0].GetProperty("title").GetString();
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

                        gameInfo.CoverPath = image;
                        gameInfo.GameName = title;
                        gameInfo.GameInfoId = id;
                        gameInfo.Developer = develop;
                        gameInfo.Description = description;
                        gameInfo.DateTime = released;
                    }

                    return gameInfo;
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    tryCount++;
                    await Task.Delay(1000);
                    continue;
                }

                string errorMessage = await UnzipAsync(response.Content);
                throw new Exception(
                    $"Failed to fetch data from VNDB with code : {response.StatusCode} \n{errorMessage}");
            }

            throw new Exception("Failed to fetch data from VNDB");
        }

        private string BuildQueryString(List<string> filter, string fields, int itemPerPage = 10, int pageNum = 1)
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

        private async Task<HttpResponseMessage> Request(string queryString)
        {
            HttpRequestMessage request = new(HttpMethod.Post, VN_REQUEST_URL)
            {
                Content = new StringContent(queryString, Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            return response;
        }

        private async Task<string> UnzipAsync(HttpContent content)
        {
            await using Stream stream = await content.ReadAsStreamAsync();
            Stream decompressStream;
            switch (content.Headers.ContentEncoding.ToString())
            {
                case "gzip":
                    decompressStream = new GZipStream(stream, CompressionMode.Decompress);
                    break;
                case "deflate":
                    decompressStream = new DeflateStream(stream, CompressionMode.Decompress);
                    break;
                case "br":
                    decompressStream = new BrotliStream(stream, CompressionMode.Decompress);
                    break;
                default:
                    decompressStream = stream;
                    break;
            }

            using var reader = new StreamReader(decompressStream);
            string result = await reader.ReadToEndAsync();

            return result;
        }
    }
}