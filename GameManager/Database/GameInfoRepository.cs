using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace GameManager.Database
{
    public class GameInfoRepository(AppDbContext context) : IGameInfoRepository
    {
        public Task<List<GameInfo>> GetGameInfos()
        {
            return context.GameInfos.Include(info => info.LaunchOption).ToListAsync();
        }

        public Task<string?> GetCoverById(int id)
        {
            return context.GameInfos.Where(x => x.Id == id).Select(x => x.CoverPath).FirstOrDefaultAsync();
        }

        public Task AddAsync(GameInfo info)
        {
            if (context.GameInfos.Any(x => x.ExePath == info.ExePath))
                return Task.CompletedTask;
            context.GameInfos.Add(info);
            return context.SaveChangesAsync();
        }

        public Task EditAsync(GameInfo info)
        {
            context.GameInfos.Update(info);
            return context.SaveChangesAsync();
        }

        public Task DeleteByIdAsync(int id)
        {
            GameInfo? item = context.GameInfos.FirstOrDefault(x => x.Id == id);
            if (item == null)
                return Task.CompletedTask;
            context.GameInfos.Remove(item);
            return context.SaveChangesAsync();
        }
    }
}