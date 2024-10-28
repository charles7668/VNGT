using GameManager.DB;
using GameManager.DB.Models;
using GameManager.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

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

        public async Task<GameInfo> AddAsync(GameInfo info)
        {
            info.UploadTime = DateTime.UtcNow;
            EntityEntry<GameInfo> entityEntry = await context.GameInfos.AddAsync(info);
            return entityEntry.Entity;
        }

        public Task<GameInfo?> GetAsync(Expression<Func<GameInfo, bool>> query)
        {
            return context.GameInfos
                .AsNoTracking()
                .Include(info => info.LaunchOption)
                .FirstOrDefaultAsync(query);
        }

        public Task<bool> AnyAsync(Expression<Func<GameInfo, bool>> query)
        {
            return context.GameInfos.AnyAsync(query);
        }

        public Task EditAsync(GameInfo info)
        {
            context.GameInfos.Update(info);
            return Task.CompletedTask;
        }

        public Task<GameInfo?> DeleteByIdAsync(int id)
        {
            GameInfo? item = context.GameInfos
                .Include(x => x.LaunchOption)
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
            if (item == null || context.Entry(item).State == EntityState.Deleted)
                return Task.FromResult<GameInfo?>(null);
            context.GameInfos.Remove(item);
            return Task.FromResult<GameInfo?>(item);
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

        public async Task UpdateStaffsAsync(Expression<Func<GameInfo, bool>> query, IEnumerable<Staff> staffs)
        {
            GameInfo? gameInfo = await context.GameInfos
                .Include(x => x.Staffs)
                .ThenInclude(x => x.StaffRole)
                .FirstOrDefaultAsync(query);
            if (gameInfo == null)
                return;
            gameInfo.Staffs.RemoveAll(_ => true);
            Dictionary<int, HashSet<string>> cache = new();
            foreach (Staff staff in staffs)
            {
                int staffId = staff.StaffRole.Id;
                string staffName = staff.Name;
                if (cache.TryGetValue(staffId, out HashSet<string>? secondHash) &&
                    secondHash.Contains(staffName))
                    continue;
                if (!cache.ContainsKey(staffId))
                    cache.Add(staffId, []);
                cache[staffId].Add(staffName);
                Staff existStaff =
                    await context.Staffs.FirstOrDefaultAsync(x =>
                        x.Name == staffName && x.StaffRoleId == staffId) ?? (await context.Staffs.AddAsync(
                        new Staff
                        {
                            Name = staffName,
                            StaffRoleId = staffId
                        })).Entity;
                gameInfo.Staffs.Add(existStaff);
            }

            context.GameInfos.Update(gameInfo);
        }

        public async Task UpdateCharactersAsync(Expression<Func<GameInfo, bool>> query,
            IEnumerable<Character> characters)
        {
            GameInfo? gameInfo = await context.GameInfos
                .Include(x => x.Characters)
                .FirstOrDefaultAsync(query);
            if (gameInfo == null)
                return;
            gameInfo.Characters.RemoveAll(_ => true);
            gameInfo.Characters.AddRange(characters);
            context.GameInfos.Update(gameInfo);
        }

        public async Task UpdateReleaseInfosAsync(Expression<Func<GameInfo, bool>> query,
            IEnumerable<ReleaseInfo> releaseInfos)
        {
            GameInfo? gameInfo = await context.GameInfos
                .Include(x => x.ReleaseInfos)
                .ThenInclude(x => x.ExternalLinks)
                .FirstOrDefaultAsync(query);
            if (gameInfo == null)
                return;
            foreach (ReleaseInfo releaseInfo in gameInfo.ReleaseInfos)
            {
                releaseInfo.ExternalLinks.RemoveAll(_ => true);
            }

            gameInfo.ReleaseInfos.RemoveAll(_ => true);
            gameInfo.ReleaseInfos.AddRange(releaseInfos);
            context.GameInfos.Update(gameInfo);
        }

        public async Task UpdateReltedSitesAsync(Expression<Func<GameInfo, bool>> query,
            IEnumerable<RelatedSite> relatedSites)
        {
            GameInfo? gameInfo = await context.GameInfos
                .Include(x => x.RelatedSites)
                .FirstOrDefaultAsync(query);
            if (gameInfo == null)
                return;
            gameInfo.RelatedSites.RemoveAll(_ => true);
            gameInfo.RelatedSites.AddRange(relatedSites);
            context.GameInfos.Update(gameInfo);
        }

        public async Task<IEnumerable<Staff>> GetStaffsAsync(Expression<Func<GameInfo, bool>> query)
        {
            List<Staff> result = await context.GameInfos
                .Include(x => x.Staffs)
                .ThenInclude(x => x.StaffRole)
                .AsNoTracking()
                .Where(query)
                .SelectMany(x => x.Staffs)
                .ToListAsync();
            return result;
        }

        public async Task<IEnumerable<Character>> GetCharactersAsync(Expression<Func<GameInfo, bool>> query)
        {
            return await context.GameInfos
                .Include(x => x.Characters)
                .AsNoTracking()
                .Where(query)
                .SelectMany(x => x.Characters)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReleaseInfo>> GetGameInfoReleaseInfos(Expression<Func<GameInfo, bool>> query)
        {
            return await context.GameInfos
                .Include(x => x.ReleaseInfos)
                .ThenInclude(x => x.ExternalLinks)
                .AsNoTracking()
                .Where(query)
                .SelectMany(x => x.ReleaseInfos)
                .ToListAsync();
        }

        public async Task<IEnumerable<RelatedSite>> GetGameInfoRelatedSites(Expression<Func<GameInfo, bool>> query)
        {
            return await context.GameInfos
                .Include(x => x.RelatedSites)
                .AsNoTracking()
                .Where(query)
                .SelectMany(x => x.RelatedSites)
                .ToListAsync();
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