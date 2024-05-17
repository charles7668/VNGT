﻿using GameManager.DB.Models;
using GameManager.Services;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace GameManager.GameInfoProvider
{
    public class VndbProvider(IHttpService httpService) : IGameInfoProvider
    {
        private const string VN_REQUEST_URL = "https://api.vndb.org/kana/vn";

        public async Task<(List<GameInfo>? infoList, bool hasMore)> FetchGameSearchListAsync(string searchText,
            int itemPerPage, int pageNum)
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

            string message = await UnzipAsync(response.Content);

            throw new Exception(
                $"Failed to fetch data from VNDB with code : {response.StatusCode} \n{message}");
        }

        public async Task<GameInfo?> FetchGameDetailByIdAsync(string gameId)
        {
            string queryString = BuildQueryString(["id", "=", gameId],
                "id , titles.title , developers.name , image.url , description , released", 1);
            HttpResponseMessage response = await Request(queryString);
            if (response.IsSuccessStatusCode)
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

            string errorMessage = await UnzipAsync(response.Content);
            throw new Exception($"Failed to fetch data from VNDB with code : {response.StatusCode} \n{errorMessage}");
        }

        private async Task<HttpResponseMessage> Request(string queryString)
        {
            HttpClient client = httpService.DefaultClient;
            HttpRequestMessage request = new(HttpMethod.Post, VN_REQUEST_URL)
            {
                Content = new StringContent(queryString, Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await client.SendAsync(request);
            return response;
        }

        private async Task<string> UnzipAsync(HttpContent content)
        {
            await using Stream stream = await content.ReadAsStreamAsync();
            await using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzipStream);
            string result = await reader.ReadToEndAsync();

            return result;
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
    }
}