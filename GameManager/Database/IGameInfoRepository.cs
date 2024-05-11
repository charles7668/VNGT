using GameManager.DB.Models;

namespace GameManager.Database
{
    public interface IGameInfoRepository
    {
        Task<List<GameInfo>> GetGameInfos();

        Task<string?> GetCoverById(int id);

        Task AddAsync(GameInfo info);

        Task EditAsync(GameInfo info);

        Task DeleteByIdAsync(int id);
    }
}