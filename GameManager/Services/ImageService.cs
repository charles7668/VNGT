using Helper;
using Helper.Image;

namespace GameManager.Services
{
    public class ImageService(IConfigService configService, IWebService webService) : IImageService
    {
        public async Task<Stream?> GetImageStreamAsync(string? uri)
        {
            if (string.IsNullOrEmpty(uri))
                return null;
            if (uri.StartsWith("cors://"))
            {
                uri = uri.Replace("cors://", "");
            }

            if (uri.IsHttpLink())
            {
                HttpClient client = webService.GetDefaultHttpClient();
                return await client.GetStreamAsync(uri);
            }

            string? fullPath = await configService.GetCoverFullPath(uri);
            if (!File.Exists(fullPath))
                return null;
            try
            {
                return File.OpenRead(fullPath);
            }
            catch
            {
                return null;
            }
        }

        public string UriResolve(string? uri, string fallback)
        {
            if (string.IsNullOrEmpty(uri))
                return fallback;
            if (uri.IsHttpLink())
            {
                return uri;
            }

            if (!uri.StartsWith("cors://"))
                return ImageHelper.GetDisplayUrl(configService.GetCoverFullPath(uri).Result!);

            try
            {
                // if image has cors
                HttpClient httpClient = webService.GetDefaultHttpClient();
                HttpResponseMessage response = httpClient.GetAsync(uri.Replace("cors://", "")).Result;
                byte[] content = response.Content.ReadAsByteArrayAsync().Result;
                string base64 = "data:image/png;base64, " + Convert.ToBase64String(content);
                return !response.IsSuccessStatusCode
                    ? fallback
                    : base64;
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }
}