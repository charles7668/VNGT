using GameManager.DTOs;

namespace GameManager.GameInfoProvider
{
    internal interface IGameInfoProvider
    {
        string ProviderName { get; }

        Task<(List<GameInfoDTO>? infoList, bool hasMore)> FetchGameSearchListAsync(string searchText, int itemPerPage,
            int pageNum);

        Task<GameInfoDTO?> FetchGameDetailByIdAsync(string gameId);
    }
}