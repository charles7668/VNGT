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
                return await context.GameInfos
                    .AsNoTracking()
                    .Include(info => info.LaunchOption)
                    .OrderByDescending(x => x.UploadTime)
                    .ToListAsync();
            return await context.GameInfos
                .AsNoTracking()
                .Include(info => info.LaunchOption)
                .OrderBy(x => x.GameName).ToListAsync();
        }

        public Task<string?> GetCoverById(int id)
        {
            return context.GameInfos
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => x.CoverPath)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(GameInfo info)
        {
            if (await context.GameInfos
                    .AsNoTracking()
                    .AnyAsync(x => x.ExePath == info.ExePath))
                throw new InvalidOperationException("Game already exists");
            info.UploadTime = DateTime.UtcNow;
            await context.GameInfos.AddAsync(info);
        }

        public Task EditAsync(GameInfo info)
        {
            context.GameInfos.Update(info);
            return Task.CompletedTask;
        }

        public Task DeleteByIdAsync(int id)
        {
            GameInfo? item = context.GameInfos
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
            if (item == null || context.Entry(item).State == EntityState.Deleted)
                return Task.CompletedTask;
            context.GameInfos.Remove(item);
            return Task.CompletedTask;
        }

        public Task<bool> CheckExePathExist(string path)
        {
            return context.GameInfos
                .AnyAsync(x => x.ExePath == path);
        }

        public async Task UpdateLastPlayedByIdAsync(int id, DateTime time)
        {
            GameInfo? info = await context.GameInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (info == null)
                return;
            info.LastPlayed = time;
            context.GameInfos.Update(info);
        }

        public async Task<IEnumerable<Tag>> GetTagsByIdAsync(int id)
        {
            GameInfo? gameInfo = await context.GameInfos
                .Where(x => x.Id == id)
                .Include(x => x.Tags)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return gameInfo == null ? [] : gameInfo.Tags;
        }

        public async Task AddTagAsync(int id, Tag tag)
        {
            GameInfo? info = await context.GameInfos
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (info == null)
                return;
            if (info.Tags.Any(x => x.Name == tag.Name))
                return;
            info.Tags.Add(tag);
            context.GameInfos.Update(info);
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