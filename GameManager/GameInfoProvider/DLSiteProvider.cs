using GameManager.DB.Models;
using Helper.Web;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Web;

namespace GameManager.GameInfoProvider
{
    public class DLSiteProvider(ILogger<DLSiteProvider> logger) : IGameInfoProvider
    {
        private const string DL_SITE_SEARCH_URL_BASE = "https://www.dlsite.com/maniax/fsr/=/";
        private const string DL_SITE_DETAIL_URL_BASE = "https://www.dlsite.com/maniax/work/=/product_id/";

        private readonly HttpClient _httpClient = RegisterHttpClient();

        public string ProviderName => "DLSite";

        public async Task<(List<GameInfo>? infoList, bool hasMore)> FetchGameSearchListAsync(string searchText,
            int itemPerPage, int pageNum)
        {
            string queryString = DL_SITE_SEARCH_URL_BASE + BuildQueryString(searchText, itemPerPage, pageNum);
            HttpResponseMessage response = await Request(queryString);
            if (response.IsSuccessStatusCode)
            {
                var gameInfos = new List<GameInfo>();
                string content = await HttpContentHelper.DecompressContent(response.Content);
                HtmlDocument document = new();
                try
                {
                    document.LoadHtml(content);
                }
                catch (Exception e)
                {
                    logger.LogError("Failed to parse html content : {Exception}", e.ToString());
                    return (null, false);
                }

                IList<HtmlNode>? nodes = document.DocumentNode.QuerySelectorAll(".search_result_img_box_inner");
                if (nodes == null)
                {
                    logger.LogWarning("No search result found");
                    return (null, false);
                }

                foreach (HtmlNode? node in nodes)
                {
                    HtmlNode? titleNode = node.QuerySelector(".work_name a");
                    HtmlNode? idNode = node.QuerySelector("dt.search_img > a");
                    if (titleNode == null || idNode == null)
                        continue;
                    string title = titleNode.InnerText ?? "";
                    string? productUrl = idNode.Attributes["href"].Value;
                    if (productUrl == null)
                        continue;
                    string id = productUrl.Split("/").Last().Replace(".html", "");
                    HtmlNode? imgNode = idNode.QuerySelector("img");
                    string img = imgNode?.Attributes["src"] == null ? "" : "https" + imgNode.Attributes["src"].Value;

                    var gameInfo = new GameInfo
                    {
                        GameName = title,
                        GameInfoId = id,
                        CoverPath = img
                    };
                    gameInfos.Add(gameInfo);
                }

                IList<HtmlNode>? pageNode = document.DocumentNode.QuerySelectorAll(".page_no li");

                return (gameInfos, pageNode is { Count: > 0 });
            }

            string message = await HttpContentHelper.DecompressContent(response.Content);

            logger.LogError("Failed to fetch data from DLSite with code : {StatusCode} \n{Message}",
                response.StatusCode,
                message);
            throw new Exception($"Failed to fetch data from DLSite with code : {response.StatusCode} \n{message}");
        }

        public async Task<GameInfo?> FetchGameDetailByIdAsync(string gameId)
        {
            string queryString = DL_SITE_DETAIL_URL_BASE + gameId + ".html";
            HttpResponseMessage response = await Request(queryString);
            logger.LogDebug("Status Code : {StatusCode}", response.StatusCode);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string content = await HttpContentHelper.DecompressContent(response.Content);
                var gameInfo = new GameInfo();
                HtmlDocument document = new();
                try
                {
                    document.LoadHtml(content);
                }
                catch (Exception e)
                {
                    logger.LogError("Failed to parse html content : {Exception}", e.ToString());
                    return null;
                }

                HtmlNode? titleNode = document.DocumentNode.QuerySelector("#work_name");
                string? title = titleNode?.InnerText;
                if (title == null)
                {
                    logger.LogWarning("No title found");
                    return null;
                }

                title = title.Trim();

                HtmlNode? brandNode = document.DocumentNode.QuerySelector(".maker_name");
                string brand = brandNode?.InnerText == null ? "" : brandNode.InnerText.Trim();

                HtmlNode? releaseDateNode =
                    document.DocumentNode.QuerySelector("#work_outline tr:nth-child(1) td a");
                string releaseDate = releaseDateNode?.InnerText == null ? "" : releaseDateNode.InnerText.Trim();

                HtmlNode? imgNode = document.DocumentNode.QuerySelector(".work_slider .slider_item img");
                string img = imgNode?.Attributes["srcset"] == null ? "" : "https:" + imgNode.Attributes["srcset"].Value;

                gameInfo.GameName = title;
                gameInfo.GameInfoId = gameId;
                gameInfo.Developer = brand;
                gameInfo.CoverPath = img;
                bool dateParseSuccess = DateTime.TryParse(releaseDate, out DateTime time);
                gameInfo.DateTime = dateParseSuccess ? time : null;
                return gameInfo;
            }

            string errorMessage = await HttpContentHelper.DecompressContent(response.Content);

            logger.LogError("Failed to fetch data from VNDB with code : {StatusCode} \n{Message}",
                response.StatusCode,
                errorMessage);
            throw new Exception(
                $"Failed to fetch data from VNDB with code : {response.StatusCode} \n{errorMessage}");
        }

        private string BuildQueryString(string searchText, int itemPerPage = 10, int pageNum = 1)
        {
            return "language/jp/keyword/" + HttpUtility.UrlEncode(searchText) + "/per_page/" + itemPerPage + "/page/" +
                   pageNum;
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

        private async Task<HttpResponseMessage> Request(string requestUrl)
        {
            HttpRequestMessage request = new(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            return response;
        }
    }
}