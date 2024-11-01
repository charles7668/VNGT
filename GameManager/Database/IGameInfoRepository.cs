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

        Task<string?> GetCoverById(int id);

        Task<GameInfo> AddAsync(GameInfo info);

        Task<GameInfo?> GetAsync(Expression<Func<GameInfo, bool>> query);

        Task<bool> AnyAsync(Expression<Func<GameInfo, bool>> query);

        Task EditAsync(GameInfo info);

        Task<GameInfo?> DeleteByIdAsync(int id);

        Task<bool> CheckExePathExist(string path);

        Task UpdateLastPlayedByIdAsync(int id, DateTime time);

        Task<IEnumerable<Tag>> GetTagsByIdAsync(int id);

        Task AddTagAsync(int id, Tag tag);

        Task UpdateStaffsAsync(Expression<Func<GameInfo, bool>> query, IEnumerable<Staff> staffs);

        Task UpdateCharactersAsync(Expression<Func<GameInfo, bool>> query, IEnumerable<Character> characters);

        Task UpdateReleaseInfosAsync(Expression<Func<GameInfo, bool>> query, IEnumerable<ReleaseInfo> releaseInfos);

        Task UpdateReltedSitesAsync(Expression<Func<GameInfo, bool>> query, IEnumerable<RelatedSite> relatedSites);

        Task<IEnumerable<Staff>> GetStaffsAsync(Expression<Func<GameInfo, bool>> query);

        Task<IEnumerable<Character>> GetCharactersAsync(Expression<Func<GameInfo, bool>> query);

        Task<IEnumerable<ReleaseInfo>> GetGameInfoReleaseInfos(Expression<Func<GameInfo, bool>> query);
        
        Task<IEnumerable<RelatedSite>> GetGameInfoRelatedSites(Expression<Func<GameInfo, bool>> query);
        
        Task<GameInfo> UpdateBackgroundImageAsync(int id, string? backgroundImage);
    }
}