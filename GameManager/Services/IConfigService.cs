using GameManager.DB.Models;
using GameManager.Enums;

namespace GameManager.Services
{
    public interface IConfigService
    {
        Task<string> AddCoverImage(string srcFile);

        Task AddGameInfoAsync(GameInfo info);

        Task AddLibraryAsync(Library library);

        Task<bool> CheckExePathExist(string path);

        Task<bool> CheckGameInfoHasTag(int gameId, string tagName);

        Task DeleteCoverImage(string? coverName);

        Task DeleteGameInfoByIdAsync(int id);

        public Task DeleteGameInfoByIdListAsync(IEnumerable<int> idList, CancellationToken cancellationToken,
            Action<int> onDeleteCallback);

        Task DeleteLibraryByIdAsync(int id);

        Task EditGameInfo(GameInfo info);

        AppSetting GetAppSetting();

        Task<string?> GetCoverFullPath(string? coverName);

        Task GetGameInfoForEachAsync(Action<GameInfo> action, CancellationToken cancellationToken,
            SortOrder order = SortOrder.UPLOAD_TIME);

        Task<IEnumerable<string>> GetGameTagsAsync(int gameId);

        Task<List<Library>> GetLibrariesAsync(CancellationToken cancellationToken);

        Task<IEnumerable<string>> GetTagsAsync();

        Task ReplaceCoverImage(string? srcFile, string? coverName);

        Task<TextMapping?> SearchTextMappingByOriginalText(string original);

        Task UpdateAppSettingAsync(AppSetting setting);

        Task UpdateGameInfoTags(int gameId, IEnumerable<string> tags);

        Task UpdateLastPlayedByIdAsync(int id, DateTime time);
    }
}