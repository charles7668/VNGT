using GameManager.DB.Models;
using GameManager.Enums;
using System.Linq.Expressions;

namespace GameManager.Services
{
    public interface IConfigService
    {
        Task<string> AddCoverImage(string srcFile);

        Task AddGameInfoAsync(GameInfo info);

        Task<GameInfo?> GetGameInfoAsync(Expression<Func<GameInfo, bool>> queryExpression);

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

        Task<List<string>> GetGameTagsAsync(int gameId);

        Task<List<Library>> GetLibrariesAsync(CancellationToken cancellationToken);

        Task<List<string>> GetTagsAsync();

        Task ReplaceCoverImage(string? srcFile, string? coverName);

        Task<TextMapping?> SearchTextMappingByOriginalText(string original);

        Task UpdateAppSettingAsync(AppSetting setting);

        Task UpdateLastPlayedByIdAsync(int id, DateTime time);

        Task BackupSettings(string path);

        Task RestoreSettings(string path);

        Task UpdateGameInfoBackgroundImageAsync(int gameInfoId, string? backgroundImage);

        Task<IEnumerable<StaffRole>> GetStaffRolesAsync();

        Task<Staff?> GetStaffAsync(Expression<Func<Staff, bool>> query);

        Task<List<Staff>> GetGameInfoStaffs(Expression<Func<GameInfo, bool>> query);

        Task<List<Character>> GetGameInfoCharacters(Expression<Func<GameInfo, bool>> query);

        Task<List<ReleaseInfo>> GetGameInfoReleaseInfos(Expression<Func<GameInfo, bool>> query);

        Task<List<RelatedSite>> GetGameInfoRelatedSites(Expression<Func<GameInfo, bool>> query);
        
        Task RemoveScreenshotAsync(int gameInfoId, string url);
        
        Task AddScreenshotsAsync(int gameInfoId, List<string> urls);
    }
}