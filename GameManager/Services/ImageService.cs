using Helper;
using Helper.Image;

namespace GameManager.Services
{
    public class ImageService(IConfigService configService, IWebService webService) : IImageService
    {
        public string UriResolve(string? uri, string fallback)
        {
            if (uri == null)
                return fallback;
            if (uri.IsHttpLink() || uri.StartsWith("cors://"))
            {
                string? temp = ResolveInternal(uri);
                return temp ?? fallback;
            }

            string absolutePath = configService.GetCoverFullPath(uri).Result!;
            return ResolveInternal(absolutePath) ?? fallback;
        }

        public string ScreenShotsUriResolve(int gameId, string? uri)
        {
            if (uri == null)
                return "";
            if (uri.IsHttpLink() || uri.StartsWith("cors://"))
            {
                string? temp = ResolveInternal(uri);
                return temp ?? "";
            }

            string screenshotsDir = configService.GetScreenShotsDirPath(gameId).Result!;
            string absolutePath = Path.Combine(screenshotsDir, uri);
            return ResolveInternal(absolutePath) ?? "";
        }

        private string? ResolveInternal(string uri)
        {
            if (uri.IsHttpLink())
            {
                return uri;
            }

            if (!uri.StartsWith("cors://"))
                return ImageHelper.GetDisplayUrl(uri);

            try
            {
                // if image has cors
                HttpClient httpClient = webService.GetDefaultHttpClient();
                HttpResponseMessage response = httpClient.GetAsync(uri.Replace("cors://", "")).Result;
                byte[] content = response.Content.ReadAsByteArrayAsync().Result;
                string base64 = "data:image/png;base64, " + Convert.ToBase64String(content);
                return !response.IsSuccessStatusCode
                    ? null
                    : base64;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}