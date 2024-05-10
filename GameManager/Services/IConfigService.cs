namespace GameManager.Services
{
    public interface IConfigService
    {
        string ConfigFolder { get; }

        void CreateConfigFolderIfNotExistAsync();

        Task<string> AddCoverImage(string srcFile);

        Task ReplaceCoverImage(string? srcFile, string? coverName);

        Task<string?> GetCoverFullPath(string? coverName);

        Task DeleteCoverImage(string? coverName);
    }
}