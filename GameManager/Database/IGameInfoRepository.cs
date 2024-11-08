using GameManager.DB.Models;
using GameManager.Enums;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public interface IGameInfoRepository
    {
        Task GetGameInfoForEachAsync(Action<GameInfo> action, CancellationToken cancellationToken,
            SortOrder order = SortOrder.UPLOAD_TIME);

        Task<List<GameInfo>> GetGameInfos(SortOrder order = SortOrder.UPLOAD_TIME);

        Task<IQueryable<GameInfo>> GetGameInfosIncludeAllAsync(Expression<Func<GameInfo, bool>> query, int start,
            int count);

        Task<List<int>> GetIdCollectionAsync(Expression<Func<GameInfo, bool>> query);

        Task<string?> GetCoverById(int id);

        Task<GameInfo> AddAsync(GameInfo info);

        Task<GameInfo?> GetAsync(Expression<Func<GameInfo, bool>> query);

        Task<GameInfo?> GetAsync(int id, bool includeAll = false);

        Task<bool> AnyAsync(Expression<Func<GameInfo, bool>> query);

        Task EditAsync(GameInfo info);

        Task<GameInfo?> DeleteByIdAsync(int id);

        Task<bool> CheckExePathExist(string path);

        Task UpdateLastPlayedByIdAsync(int id, DateTime time);

        Task<IEnumerable<Tag>> GetTagsByIdAsync(int id);

        Task UpdateStaffsAsync(Expression<Func<GameInfo, bool>> query, List<Staff> staffs);

        Task UpdateCharactersAsync(Expression<Func<GameInfo, bool>> query, List<Character> characters);

        Task UpdateReleaseInfosAsync(Expression<Func<GameInfo, bool>> query, List<ReleaseInfo> releaseInfos);

        Task UpdateRelatedSitesAsync(Expression<Func<GameInfo, bool>> query, List<RelatedSite> relatedSites);

        Task UpdateTagsAsync(Expression<Func<GameInfo, bool>> query, List<Tag> tags);

        Task<IEnumerable<Staff>> GetStaffsAsync(Expression<Func<GameInfo, bool>> query);

        Task<IEnumerable<Character>> GetCharactersAsync(Expression<Func<GameInfo, bool>> query);

        Task<List<ReleaseInfo>> GetGameInfoReleaseInfos(Expression<Func<GameInfo, bool>> query);

        Task<IEnumerable<RelatedSite>> GetGameInfoRelatedSites(Expression<Func<GameInfo, bool>> query);

        Task<GameInfo> UpdateBackgroundImageAsync(int id, string? backgroundImage);

        Task<GameInfo?> RemoveScreenshotAsync(int id, string url);

        Task<GameInfo?> AddScreenshotsAsync(int id, List<string> urls);
        
        Task<int> CountAsync(Expression<Func<GameInfo, bool>> queryExpression);
        
        Task<IQueryable<string>> GetUniqueIdCollectionAsync(Expression<Func<GameInfo, bool>> queryExpression, int start,
            int count);
    }
}