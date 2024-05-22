using GameManager.DB.Models;
using GameManager.Enums;

namespace GameManager.Database
{
    public interface IGameInfoRepository
    {
        Task GetGameInfoForEachAsync(Action<GameInfo> action,CancellationToken cancellationToken,SortOrder order = SortOrder.UPLOAD_TIME);

        Task<List<GameInfo>> GetGameInfos(SortOrder order = SortOrder.UPLOAD_TIME);

        Task<string?> GetCoverById(int id);

        Task AddAsync(GameInfo info);

        Task EditAsync(GameInfo info);

        Task DeleteByIdAsync(int id);

        Task<bool> CheckExePathExist(string path);

        Task UpdateLastPlayedByIdAsync(int id , DateTime time);
    }
}