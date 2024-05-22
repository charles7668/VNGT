using GameManager.DB;
using GameManager.DB.Models;
using GameManager.Enums;
using Microsoft.EntityFrameworkCore;

namespace GameManager.Database
{
    public class GameInfoRepository(AppDbContext context) : IGameInfoRepository
    {
        public async Task<List<GameInfo>> GetGameInfos(SortOrder order)
        {
            if (order == SortOrder.UPLOAD_TIME)
                return await context.GameInfos.AsNoTracking().Include(info => info.LaunchOption)
                    .OrderByDescending(x => x.UploadTime)
                    .ToListAsync();
            return await context.GameInfos.AsNoTracking().Include(info => info.LaunchOption)
                .OrderBy(x => x.GameName).ToListAsync();
        }

        public Task<string?> GetCoverById(int id)
        {
            return context.GameInfos.AsNoTracking().Where(x => x.Id == id).Select(x => x.CoverPath)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(GameInfo info)
        {
            if (context.GameInfos.AsNoTracking().Any(x => x.ExePath == info.ExePath))
                throw new InvalidOperationException("Game already exists");
            info.UploadTime = DateTime.Now;
            context.GameInfos.Add(info);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        public async Task EditAsync(GameInfo info)
        {
            context.GameInfos.Update(info);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        public async Task DeleteByIdAsync(int id)
        {
            GameInfo? item = context.GameInfos.AsNoTracking().FirstOrDefault(x => x.Id == id);
            if (item == null || context.Entry(item).State == EntityState.Deleted)
                return;
            context.GameInfos.Remove(item);
            await context.SaveChangesAsync();
        }

        public async Task<bool> CheckExePathExist(string path)
        {
            return await context.GameInfos.AsNoTracking().AnyAsync(x => x.ExePath == path);
        }

        public async Task UpdateLastPlayedByIdAsync(int id, DateTime time)
        {
            GameInfo? info = await context.GameInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if(info == null)
                return;
            info.LastPlayed = time;
            context.GameInfos.Update(info);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        public async Task GetGameInfoForEachAsync(Action<GameInfo> action, CancellationToken cancellationToken,
            SortOrder order = SortOrder.UPLOAD_TIME)
        {
            IOrderedQueryable<GameInfo> queryable;
            if (order == SortOrder.UPLOAD_TIME)
            {
                queryable = context.GameInfos
                    .AsNoTracking()
                    .Include(info => info.LaunchOption)
                    .OrderByDescending(x => x.UploadTime);
                await queryable.ForEachAsync(action, cancellationToken);
            }
            else
            {
                queryable = context.GameInfos
                    .AsNoTracking()
                    .Include(info => info.LaunchOption)
                    .OrderBy(x => x.GameName);
                await queryable.ForEachAsync(action, cancellationToken);
            }
        }
    }
}