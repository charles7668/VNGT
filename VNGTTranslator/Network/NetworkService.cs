using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace VNGTTranslator.Network
{
    public class NetworkService : INetworkService
    {
        public NetworkService()
        {
            DefaultHttpClient = new HttpClient();
            DefaultHttpClient.DefaultRequestHeaders.Add("User-Agent", "VNGT");
            DefaultHttpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            DefaultHttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            DefaultHttpClient.DefaultRequestHeaders.Add("Accept-Language", "*");
        }

        public HttpClient DefaultHttpClient { get; }

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
    }
}