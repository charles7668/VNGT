using GameManager.DB.Models;

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

        Task DeleteGameById(int id);

        Task AddGameInfo(GameInfo info);

        string GetDbPath();

        Task EditGameInfo(GameInfo info);

        AppSetting GetAppSetting();
    }
}