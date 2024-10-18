using Helper;
using Helper.Image;

namespace GameManager.Services
{
    public class ImageService(IConfigService configService, IWebService webService) : IImageService
    {
        public string UriResolve(string? uri)
        {
            if (string.IsNullOrEmpty(uri))
                return "/images/no-image.webp";
            if (uri.IsHttpLink())
            {
                return uri;
            }

            if (!uri.StartsWith("cors://"))
                return ImageHelper.GetDisplayUrl(configService.GetCoverFullPath(uri).Result!);

            // if image has cors
            HttpClient httpClient = webService.GetDefaultHttpClient();
            HttpResponseMessage response = httpClient.GetAsync(uri.Replace("cors://", "")).Result;
            byte[] content = response.Content.ReadAsByteArrayAsync().Result;
            string base64 = "data:image/png;base64, " + Convert.ToBase64String(content);
            return !response.IsSuccessStatusCode
                ? "/images/no-image.webp"
                : base64;
        }
    }
}