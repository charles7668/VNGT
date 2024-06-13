using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using VNGTTranslator.Network;

namespace VNGTTranslator.TranslateProviders.Google
{
    [Export(typeof(ITranslateProvider))]
    public class GoogleTranslateProvider : ITranslateProvider
    {
        private readonly INetworkService _networkService =
            Program.ServiceProvider.GetRequiredService<INetworkService>();

        public string ProviderName { get; } = "Google";

        public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                "https://translate.google.com/_/TranslateWebserverUi/data/batchexecute");
            requestMessage.Headers.Add("Origin", "https://translate.google.com");
            requestMessage.Headers.Add("Referer", "https://translate.google.com");
            requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");
            requestMessage.Headers.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.0.0 Safari/537.36");
            List<List<object>> temp = [["test", "ja", "zh-CN"], [1]];
            List<List<List<object>>> temp2 = [[["MkEWBc", JsonSerializer.Serialize(temp), null, "generic"]]];
            var reqData = new Dictionary<string, object>()
            {
                { "f.req", JsonSerializer.Serialize(temp2) }
            };
            var contentList = new List<string>();
            foreach (var item in reqData)
            {
                var key = HttpUtility.UrlEncode(item.Key);
                var value = HttpUtility.UrlEncode(item.Value.ToString());
                contentList.Add($"{key}={value}");
            }

            var reqDataStr = string.Join('&', contentList);
            requestMessage.Content = new StringContent(reqDataStr, Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = _networkService.DefaultHttpClient.SendAsync(requestMessage).Result;
            if (!response.IsSuccessStatusCode)
            {
                return $"{response.StatusCode}";
            }

            string responseString = await _networkService.UnzipAsync(response.Content);
            string responseJson = responseString.Substring(6);
            responseJson = HttpUtility.UrlDecode(responseJson);
            JsonArray? jContent = JsonSerializer.Deserialize<JsonArray>(responseJson);
            var responseItem = jContent?[0]?[2];
            if (responseItem == null)
                return "response parse failed";
            var tempResponseItem = JsonSerializer.Deserialize<JsonNode>(responseItem);
            var obj = tempResponseItem?[1]?[0]?[0]?[5] ?? tempResponseItem?[1]?[0];
            if (obj == null)
                return "response parse failed";
            var resultArray = obj.AsArray();
            var result = string.Empty;
            foreach (var o in resultArray)
            {
                result += " ";
                result += o?[0]?.GetValue<string>();
            }

            return result;
        }
    }
}