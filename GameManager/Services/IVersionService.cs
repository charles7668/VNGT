namespace GameManager.Services
{
    public interface IVersionService
    {
        public Task<string?> DetectNewestVersion();
    }
}