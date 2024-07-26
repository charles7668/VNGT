using GameManager.DB;
using Microsoft.EntityFrameworkCore;

namespace GameManager.Database
{
    internal class GameInfoTagRepository(AppDbContext context) : IGameInfoTagRepository
    {
        public Task<bool> CheckGameHasTag(int tagId, int gameId)
        {
            return context.GameInfoTag.AnyAsync(x => x.TagId == tagId && x.GameInfoId == gameId);
        }
    }
}