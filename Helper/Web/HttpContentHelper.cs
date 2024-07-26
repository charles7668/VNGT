using System.IO.Compression;

namespace Helper.Web
{
    public static class HttpContentHelper
    {
        public static async Task<string> DecompressContent(System.Net.Http.HttpContent content)
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