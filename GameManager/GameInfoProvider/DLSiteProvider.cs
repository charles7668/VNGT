using GameManager.DB.Enums;
using GameManager.DB.Models;
using GameManager.Enums;
using GameManager.Services;
using Helper.Web;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Web;

namespace GameManager.GameInfoProvider
{
    public class DLSiteProvider(ILogger<DLSiteProvider> logger, IStaffService staffService) : IGameInfoProvider
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
                        GameInfoFetchId = id,
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
                IList<HtmlNode>? infoNodes = document.DocumentNode.QuerySelectorAll("#work_outline tr");
                List<Staff> staffList = await GetStaffListAsync(infoNodes.ToList());
                bool dateParseSuccess = DateTime.TryParse(releaseDate, out DateTime time);
                IList<HtmlNode>? deviceTableNodes =
                    document.DocumentNode.QuerySelectorAll("#work_device_guide .work_device_table tr");
                ReleaseInfo releaseInfo =
                    GetReleaseInfosAsync(infoNodes.ToList(), deviceTableNodes.ToList(), title).Result;
                RelatedSite relatedSite = new()
                {
                    Name = "DLSite",
                    Url = queryString
                };
                List<string> screenShots = await GetScreenShot(document);

                gameInfo.GameName = title;
                gameInfo.GameInfoFetchId = gameId;
                gameInfo.Developer = brand;
                gameInfo.CoverPath = img;
                gameInfo.Staffs = staffList;
                gameInfo.DateTime = dateParseSuccess ? time : null;
                gameInfo.ReleaseInfos = [releaseInfo];
                gameInfo.RelatedSites = [relatedSite];
                gameInfo.ScreenShots = screenShots;
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

        private Task<ReleaseInfo> GetReleaseInfosAsync(List<HtmlNode> infoNodes, List<HtmlNode> deviceTableNodes,
            string gameName)
        {
            var releaseInfo = new ReleaseInfo
            {
                ReleaseName = gameName
            };
            HtmlNode? releaseTimeNode = infoNodes.FirstOrDefault(x => x.QuerySelector("th")?.InnerText == "販売日");
            if (releaseTimeNode != null)
            {
                string releaseTime = releaseTimeNode.QuerySelector("td")?.InnerText ?? "";
                bool dateParseSuccess = DateTime.TryParse(releaseTime, out DateTime time);
                releaseInfo.ReleaseDate = dateParseSuccess ? time : DateTime.MinValue;
            }

            HtmlNode? ageRatingNode = infoNodes.FirstOrDefault(x => x.QuerySelector("th")?.InnerText == "年齢指定");
            if (ageRatingNode != null)
            {
                string ageRatingString = ageRatingNode.QuerySelector("td span")?.InnerText ?? "";
                releaseInfo.AgeRating = ResolveAgeRating(ageRatingString);
            }

            HtmlNode? pcSupportNode = deviceTableNodes.FirstOrDefault(x => x.QuerySelector("td.icon_pc") != null);
            HtmlNode? phoneSupportNode = deviceTableNodes.FirstOrDefault(x => x.QuerySelector("td.icon_sp") != null);
            releaseInfo.Platforms = [];
            if (pcSupportNode?.QuerySelector("td:nth-child(2) > span")?.HasClass("dev_play") is true)
            {
                releaseInfo.Platforms.Add(PlatformEnum.WINDOWS);
            }

            if (phoneSupportNode?.QuerySelector("td:nth-child(2) > span")?.HasClass("dev_play") is true)
            {
                releaseInfo.Platforms.Add(PlatformEnum.WINDOWS);
            }

            return Task.FromResult(releaseInfo);
        }

        private Task<List<string>> GetScreenShot(HtmlDocument htmlDocument)
        {
            var result = new List<string>();
            IList<HtmlNode>? imageNodes =
                htmlDocument.DocumentNode.QuerySelectorAll("#work_left .product-slider-data div");
            foreach (HtmlNode? imageNode in imageNodes)
            {
                string? imageUrl = imageNode.Attributes["data-src"]?.Value;
                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    result.Add("https:" + imageUrl);
                }
            }

            return Task.FromResult(result);
        }

        private int ResolveAgeRating(string ageRatingString)
        {
            return ageRatingString switch
            {
                "全年齢" => 0,
                "18禁" => 18,
                "15禁" => 15,
                "R15" => 15,
                "R18" => 18,
                _ => 0
            };
        }

        private Task<List<Staff>> GetStaffListAsync(List<HtmlNode> nodes)
        {
            List<Staff> staffList = [];
            HtmlNode? senarioNode = nodes.FirstOrDefault(x => x.QuerySelector("th")?.InnerText == "シナリオ");
            if (senarioNode != null)
            {
                IEnumerable<string> senarios = senarioNode.QuerySelectorAll("td a").Select(x => x.InnerText);
                staffList.AddRange(senarios.Select(x => new Staff
                {
                    Name = x,
                    StaffRole = staffService.GetStaffRoleAsync(StaffRoleEnum.SCENARIO).Result
                }));
            }

            HtmlNode? illustratorNode = nodes.FirstOrDefault(x => x.QuerySelector("th")?.InnerText == "イラスト");
            if (illustratorNode != null)
            {
                IEnumerable<string> illustrators = illustratorNode.QuerySelectorAll("td a").Select(x => x.InnerText);
                staffList.AddRange(illustrators.Select(x => new Staff
                {
                    Name = x,
                    StaffRole = staffService.GetStaffRoleAsync(StaffRoleEnum.ARTIST).Result
                }));
            }

            HtmlNode? authorNode = nodes.FirstOrDefault(x => x.QuerySelector("th")?.InnerText == "作者");
            if (authorNode != null)
            {
                IEnumerable<string> authors = authorNode.QuerySelectorAll("td a").Select(x => x.InnerText);
                staffList.AddRange(authors.Select(x => new Staff
                {
                    Name = x,
                    StaffRole = staffService.GetStaffRoleAsync(StaffRoleEnum.DIRECTOR).Result
                }));
            }

            return Task.FromResult(staffList);
        }

        private async Task<HttpResponseMessage> Request(string requestUrl)
        {
            HttpRequestMessage request = new(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            return response;
        }
    }
}