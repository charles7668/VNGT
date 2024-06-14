using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using VNGTTranslator.Configs;

namespace VNGTTranslator.Network
{
    public class NetworkService : INetworkService
    {
        public NetworkService()
        {
            _appConfig = Program.ServiceProvider.GetRequiredService<IAppConfigProvider>().GetAppConfig();
            DefaultHttpClient = new HttpClient();
            DefaultHttpClient.DefaultRequestHeaders.Add("User-Agent", "VNGT");
            DefaultHttpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            DefaultHttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            DefaultHttpClient.DefaultRequestHeaders.Add("Accept-Language", "*");
            UpdateProxySettingUsingAppConfig();
        }

        private readonly AppConfig _appConfig;
        private IWebProxy? _proxy;

        public HttpClient DefaultHttpClient { get; }
        public HttpClient? ProxyHttpClient { get; private set; }

        public async Task<string> UnzipAsync(HttpContent content)
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

        public void UpdateProxySettingUsingAppConfig()
        {
            if (_appConfig.IsUseSystemProxy)
            {
                _proxy = WebRequest.DefaultWebProxy;
                _proxy = null;
            }
            else
            {
                try
                {
                    _proxy = string.IsNullOrEmpty(_appConfig.ProxyAddress)
                        ? null
                        : new WebProxy(_appConfig.ProxyAddress, int.Parse(_appConfig.ProxyPort));
                }
                catch (Exception)
                {
                    _proxy = null;
                }
            }

            var httpClientHandler = new HttpClientHandler
            {
                Proxy = _proxy,
                UseProxy = true
            };

            ProxyHttpClient?.Dispose();
            ProxyHttpClient = new HttpClient(httpClientHandler);
            ProxyHttpClient.DefaultRequestHeaders.Add("User-Agent", "VNGT");
            ProxyHttpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            ProxyHttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            ProxyHttpClient.DefaultRequestHeaders.Add("Accept-Language", "*");
        }
    }
}