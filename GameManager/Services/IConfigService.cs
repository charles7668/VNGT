using GameManager.DB.Models;
using GameManager.DTOs;
using GameManager.Enums;
using System.Linq.Expressions;

namespace GameManager.Services
{
    public interface IConfigService
    {
        Task<string> AddCoverImage(string srcFile);

        Task<GameInfoDTO> AddGameInfoAsync(GameInfoDTO dto, bool generateUniqueId = true);

        Task<GameInfoDTO?> GetGameInfoDTOAsync(Expression<Func<GameInfo, bool>> queryExpression,
            Func<IQueryable<GameInfo>, IQueryable<GameInfo>>? includeQuery = null);

        /// <summary>
        /// Get game info dto with just launch option include
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<GameInfoDTO?> GetGameInfoBaseDTOAsync(int id);

        Task<List<int>> GetGameInfoIdCollectionAsync(Expression<Func<GameInfo, bool>> queryExpression);

        Task<List<GameInfoDTO>> GetGameInfoDTOsAsync(List<int> ids, int start, int count , Func<IQueryable<GameInfo>, IQueryable<GameInfo>>? includeFunc = null);

        Task<List<GameInfoDTO>> GetGameInfoIncludeAllDTOsAsync(List<int> ids, int start, int count);

        Task<GameInfoDTO> UpdateGameInfoAsync(GameInfoDTO dto);

        Task AddLibraryAsync(LibraryDTO library);

        Task<bool> CheckExePathExist(string path);

        Task<bool> CheckGameInfoHasTag(int gameId, string tagName);

        Task DeleteCoverImage(string? coverName);

        Task DeleteGameInfoByIdAsync(int id);

        public Task DeleteGameInfoByIdListAsync(IEnumerable<int> idList, CancellationToken cancellationToken,
            Action<int> onDeleteCallback);

        Task DeleteLibraryByIdAsync(int id);

        Task<bool> HasGameInfo(Expression<Func<GameInfo, bool>> queryExpression);

        Task<List<string>> GetUniqueIdCollection(Expression<Func<GameInfo, bool>> queryExpression, int start,
            int count);

        Task<int> GetGameInfoCountAsync(Expression<Func<GameInfo, bool>> queryExpression);

        Task UpdateGameInfoFavoriteAsync(int id, bool isFavorite);

        Task UpdateGameInfoSyncStatusAsync(List<int> ids, bool isSyncEnable);

        AppSettingDTO GetAppSettingDTO();

        Task<string?> GetCoverFullPath(string? coverName);
        
        Task<string?> GetScreenShotsDirPath(int gameId);

        Task GetGameInfoForEachAsync(Action<GameInfoDTO> action, CancellationToken cancellationToken);

        Task<List<string>> GetGameTagsAsync(int gameId);

        Task<List<LibraryDTO>> GetLibrariesAsync(CancellationToken cancellationToken);

        Task<List<string>> GetTagsAsync();

        Task ReplaceCoverImage(string? srcFile, string? coverName);

        Task<TextMappingDTO?> SearchTextMappingByOriginalText(string original);

        Task UpdateAppSettingAsync(AppSettingDTO settingDto);

        Task UpdateLastPlayedByIdAsync(int id, DateTime time);

        Task UpdatePlayTimeAsync(int gameId, TimeSpan timeToAdd);

        Task BackupSettings(string path);

        Task RestoreSettings(string path);

        Task UpdateGameInfoBackgroundImageAsync(int gameInfoId, string? backgroundImage);

        Task<IEnumerable<StaffRoleDTO>> GetStaffRolesAsync();

        Task<StaffDTO?> GetStaffAsync(Expression<Func<Staff, bool>> query);

        Task<List<StaffDTO>> GetGameInfoStaffDTOs(Expression<Func<GameInfo, bool>> query);

        Task<List<CharacterDTO>> GetGameInfoCharacters(Expression<Func<GameInfo, bool>> query);

        Task<List<ReleaseInfoDTO>> GetGameInfoReleaseInfos(Expression<Func<GameInfo, bool>> query);

        Task<List<RelatedSiteDTO>> GetGameInfoRelatedSites(Expression<Func<GameInfo, bool>> query);

        Task RemoveScreenshotsAsync(int gameInfoId, List<string> urls);

        Task AddScreenshotsAsync(int gameInfoId, List<string> urls);
        
        Task AddScreenshotsFromFilesAsync(int gameInfoId, List<string> filePaths);

        Task<List<PendingGameInfoDeletionDTO>> GetPendingGameInfoDeletionUniqueIdsAsync();

        Task RemovePendingGameInfoDeletionsAsync(List<PendingGameInfoDeletionDTO> pendingGameInfoDeletionDTOs);
    }
}