using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using VNGTTranslator.Configs;
using VNGTTranslator.Models;
using VNGTTranslator.Network;
using VNGTTranslator.TranslateProviders.SettingPages;
using MessageBox = System.Windows.Forms.MessageBox;
using Window = System.Windows.Window;

namespace VNGTTranslator.TranslateProviders.Google
{
    [Export(typeof(ITranslateProvider))]
    public class GoogleTranslateProvider : ITranslateProvider
    {
        private readonly IAppConfigProvider _appConfigProvider =
            Program.ServiceProvider.GetRequiredService<IAppConfigProvider>();

        private readonly INetworkService _networkService =
            Program.ServiceProvider.GetRequiredService<INetworkService>();

        public string ProviderName { get; } = "Google";

        public async Task<string> TranslateAsync(string text, LanguageConstant.Language sourceLanguage,
            LanguageConstant.Language targetLanguage)
        {
            Dictionary<string, object> setting = _appConfigProvider.GetTranslatorProviderConfig(ProviderName);
            bool isProxyUse = !setting.TryGetValue("IsProxyUse", out object? obj) || (bool)obj;
            HttpClient? httpClient = isProxyUse ? _networkService.ProxyHttpClient : _networkService.DefaultHttpClient;
            if (httpClient == null)
            {
                throw new Exception("HttpClient is null");
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                "https://translate.google.com/_/TranslateWebserverUi/data/batchexecute");
            requestMessage.Headers.Add("Origin", "https://translate.google.com");
            requestMessage.Headers.Add("Referer", "https://translate.google.com");
            requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");
            requestMessage.Headers.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.0.0 Safari/537.36");
            string sourceLanguageCode = LanguageConstant.GetLanguageCode(sourceLanguage);
            string targetLangCode = LanguageConstant.GetLanguageCode(targetLanguage);
            List<List<object>> temp = [[text, sourceLanguageCode, targetLangCode], [1]];
            List<List<List<object>>> temp2 = [[["MkEWBc", JsonSerializer.Serialize(temp), null!, "generic"]]];
            var reqData = new Dictionary<string, object>
            {
                { "f.req", JsonSerializer.Serialize(temp2) }
            };
            var contentList =
                (from item in reqData
                    let key = HttpUtility.UrlEncode(item.Key)
                    let value = HttpUtility.UrlEncode(item.Value.ToString())
                    select $"{key}={value}").ToList();

            string reqDataStr = string.Join('&', contentList);
            requestMessage.Content = new StringContent(reqDataStr, Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response =
                await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return $"{response.StatusCode}";
            }

            string responseString = await _networkService.UnzipAsync(response.Content);
            JsonArray? responseJArray = JsonSerializer.Deserialize<JsonArray>(responseString[6..]);
            string? tempString = responseJArray?[0]?[2]?.ToString().Trim('\"');
            if (tempString == null)
                return "";
            JsonNode? tempResponseItem = JsonSerializer.Deserialize<JsonNode>(tempString);
            JsonNode? parseJsonNode = tempResponseItem?[1]?[0]?[0]?[5] ?? tempResponseItem?[1]?[0];
            if (parseJsonNode == null)
                return "";
            JsonArray resultArray = parseJsonNode.AsArray();
            string result = string.Empty;
            foreach (JsonNode? o in resultArray)
            {
                result += " ";
                result += o?[0]?.GetValue<string>();
            }

            return result.Trim();
        }

        public PopupWindow GetSettingWindow(Window parent)
        {
            Dictionary<string, object> setting = _appConfigProvider.GetTranslatorProviderConfig(ProviderName);
            bool isProxyUse = !setting.TryGetValue("IsProxyUse", out object? obj) || (bool)obj;
            BaseSettingPage.SettingParams settingParams = new()
            {
                IsUseProxy = isProxyUse
            };
            Page settingPage = new BaseSettingPage(ref settingParams);
            var window = new PopupWindow
            {
                PopupElement = settingPage,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None,
                MinWidth = 0,
                MinHeight = 0,
                Title = ProviderName,
                Owner = parent
            };
            window.Closing += (_, _) =>
            {
                bool hasChange = false;
                if (settingParams.IsUseProxy != isProxyUse)
                {
                    setting["IsProxyUse"] = settingParams.IsUseProxy;
                    hasChange = true;
                }

                if (!hasChange) return;
                Result saveSuccess = _appConfigProvider.SaveTranslatorProviderConfig(ProviderName, setting);
                if (!saveSuccess)
                    MessageBox.Show(saveSuccess.ErrorMessage);
            };
            return window;
        }
    }
}