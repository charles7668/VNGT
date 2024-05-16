using GameManager.DB;
using GameManager.DB.Models;
using GameManager.Enums;
using Microsoft.EntityFrameworkCore;

namespace GameManager.Database
{
    public class GameInfoRepository(AppDbContext context) : IGameInfoRepository
    {
        public Task GetGameInfoForEachAsync(Action<GameInfo> action, SortOrder order = SortOrder.UPLOAD_TIME)
        {
            if (order == SortOrder.UPLOAD_TIME)
                return context.GameInfos.Include(info => info.LaunchOption)
                    .OrderByDescending(x => x.UploadTime)
                    .ForEachAsync(action);
            return context.GameInfos.Include(info => info.LaunchOption)
                .OrderBy(x => x.GameName).ForEachAsync(action);
        }

        public Task<List<GameInfo>> GetGameInfos(SortOrder order)
        {
            if (order == SortOrder.UPLOAD_TIME)
                return context.GameInfos.Include(info => info.LaunchOption)
                    .OrderByDescending(x => x.UploadTime)
                    .ToListAsync();
            return context.GameInfos.Include(info => info.LaunchOption)
                .OrderBy(x => x.GameName).ToListAsync();
        }

        public Task<string?> GetCoverById(int id)
        {
            return context.GameInfos.Where(x => x.Id == id).Select(x => x.CoverPath).FirstOrDefaultAsync();
        }

        public Task AddAsync(GameInfo info)
        {
            if (context.GameInfos.Any(x => x.ExePath == info.ExePath))
                throw new InvalidOperationException("Game already exists");
            info.UploadTime = DateTime.Now;
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