using GameManager.DB.Models;
using GameManager.Enums;

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

        Task AddGameInfoAsync(GameInfo info);

        Task GetGameInfoForEachAsync(Action<GameInfo> action,CancellationToken cancellationToken,SortOrder order = SortOrder.UPLOAD_TIME);

        string GetDbPath();

        string GetLogPath();

        Task EditGameInfo(GameInfo info);

        AppSetting GetAppSetting();

        Task UpdateAppSettingAsync(AppSetting setting);

        Task<List<Library>> GetLibrariesAsync(CancellationToken cancellationToken);

        Task AddLibraryAsync(Library library);

        Task DeleteLibraryByIdAsync(int id);

        Task<bool> CheckExePathExist(string path);
    }
}