using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace GameManager.Database
{
    internal class GameInfoTagRepository(AppDbContext context) : IGameInfoTagRepository
    {
        public Task<bool> CheckGameHasTag(int tagId, int gameId)
        {
            return context.GameInfoTags.AnyAsync(x => x.TagId == tagId && x.GameInfoId == gameId);
        }

        public async Task RemoveGameInfoTagAsync(int tagId, int gameId)
        {
            GameInfoTag? item = await context.GameInfoTags.Where(x => x.GameInfoId == gameId && x.TagId == tagId)
                .FirstOrDefaultAsync();
            if (item != null)
            {
                context.GameInfoTags.Remove(item);
            }
        }
    }
}