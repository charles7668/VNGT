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

        Task<bool> AnyAsync(Expression<Func<GameInfo, bool>> query);

        Task EditAsync(GameInfo info);

        Task<GameInfo?> DeleteByIdAsync(int id);

        Task<bool> CheckExePathExist(string path);

        Task UpdateLastPlayedByIdAsync(int id, DateTime time);

        Task<IEnumerable<Tag>> GetTagsByIdAsync(int id);

        Task AddTagAsync(int id, Tag tag);
    }
}