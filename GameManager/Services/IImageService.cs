namespace GameManager.Services
{
    public interface IImageService
    {
        public Task<Stream?> GetImageStreamAsync(string? uri);
        public string UriResolve(string? uri, string fallback = "/images/no-image.webp");
    }
}