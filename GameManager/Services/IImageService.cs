namespace GameManager.Services
{
    public interface IImageService
    {
        public string UriResolve(string? uri, string fallback = "/images/no-image.webp");
        public string ScreenShotsUriResolve(int gameId, string? uri);
    }
}