using GameManager.DB.Models;

namespace GameManager.GameInfoProvider
{
    internal interface IProvider
    {
        Task<(List<GameInfo>? infoList, bool hasMore)> FetchGameSearchListAsync(string searchText, int itemPerPage,
            int pageNum);

        Task<GameInfo?> FetchGameDetailByIdAsync(string gameId);
    }
}