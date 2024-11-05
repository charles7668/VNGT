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

        public async Task UpdateStaffsAsync(Expression<Func<GameInfo, bool>> query, List<Staff> staffs)
        {
            GameInfo? gameInfo = await context.GameInfos
                .Include(x => x.Staffs)
                .FirstOrDefaultAsync(query);
            if (gameInfo == null)
                return;
            var staffNames = staffs.Select(s => s.Name).Distinct().ToList();
            var staffRoleIds = staffs.Select(s => s.StaffRole.Id).Distinct().ToList();
            List<Staff> existingStaffs = await context.Staffs
                .Where(s => staffNames.Contains(s.Name) && staffRoleIds.Contains(s.StaffRoleId))
                .ToListAsync();

            var existingStaffDict = existingStaffs
                .GroupBy(s => new
                {
                    s.Name,
                    s.StaffRoleId
                })
                .ToDictionary(g => g.Key, g => g.First());
            var newStaffs = new List<Staff>();

            foreach (Staff staff in staffs)
            {
                var key = new
                {
                    staff.Name,
                    StaffRoleId = staff.StaffRole.Id
                };
                if (existingStaffDict.ContainsKey(key))
                    continue;
                var newStaff = new Staff
                {
                    Name = staff.Name,
                    StaffRoleId = staff.StaffRole.Id
                };
                newStaffs.Add(newStaff);
                existingStaffDict[key] = newStaff;
            }

            if (newStaffs.Count != 0)
            {
                await context.Staffs.AddRangeAsync(newStaffs);
            }

            gameInfo.Staffs.Clear();
            foreach (var key in staffs.Select(staff => new
                     {
                         staff.Name,
                         StaffRoleId = staff.StaffRole.Id
                     }))
            {
                if (existingStaffDict.TryGetValue(key, out Staff? existingStaff))
                {
                    gameInfo.Staffs.Add(existingStaff);
                }
            }

            context.GameInfos.Update(gameInfo);
        }

        public async Task UpdateCharactersAsync(Expression<Func<GameInfo, bool>> query,
            List<Character> characters)
        {
            GameInfo? gameInfo = await context.GameInfos
                .Include(x => x.Characters)
                .FirstOrDefaultAsync(query);
            if (gameInfo == null)
                return;
            string concurrencyStamp = Guid.NewGuid().ToString();
            foreach (Character character in characters)
            {
                character.Id = 0;
                character.GameInfoId = gameInfo.Id;
                character.ConcurrencyStamp = concurrencyStamp;
                gameInfo.Characters.Add(character);
            }

            gameInfo.Characters.RemoveAll(x => x.ConcurrencyStamp != concurrencyStamp);
            context.GameInfos.Update(gameInfo);
        }

        public async Task UpdateReleaseInfosAsync(Expression<Func<GameInfo, bool>> query,
            List<ReleaseInfo> releaseInfos)
        {
            GameInfo? gameInfo = await context.GameInfos
                .Include(x => x.ReleaseInfos)
                .ThenInclude(x => x.ExternalLinks)
                .FirstOrDefaultAsync(query);
            if (gameInfo == null)
                return;
            string concurrencyStamp = Guid.NewGuid().ToString();
            foreach (ReleaseInfo releaseInfo in releaseInfos)
            {
                releaseInfo.Id = 0;
                releaseInfo.GameInfoId = gameInfo.Id;
                releaseInfo.ConcurrencyStamp = concurrencyStamp;
                foreach (ExternalLink externalLink in releaseInfo.ExternalLinks)
                {
                    externalLink.Id = 0;
                    externalLink.ConcurrencyStamp = concurrencyStamp;
                }

                gameInfo.ReleaseInfos.Add(releaseInfo);
            }

            string? oldConcurrencyStamp = gameInfo.ReleaseInfos
                .FirstOrDefault(x => x.ConcurrencyStamp != concurrencyStamp)?.ConcurrencyStamp;

            await context.ExternalLinks.Where(x => x.ConcurrencyStamp == oldConcurrencyStamp).ExecuteDeleteAsync();
            await context.ReleaseInfos.Where(x => x.ConcurrencyStamp == oldConcurrencyStamp).ExecuteDeleteAsync();
        }

        public async Task UpdateRelatedSitesAsync(Expression<Func<GameInfo, bool>> query,
            List<RelatedSite> relatedSites)
        {
            GameInfo? gameInfo = await context.GameInfos
                .Include(x => x.RelatedSites)
                .FirstOrDefaultAsync(query);
            if (gameInfo == null)
                return;
            string concurrencyStamp = Guid.NewGuid().ToString();
            foreach (RelatedSite relatedSite in relatedSites)
            {
                relatedSite.Id = 0;
                relatedSite.GameInfoId = gameInfo.Id;
                relatedSite.ConcurrencyStamp = concurrencyStamp;
                gameInfo.RelatedSites.Add(relatedSite);
            }

            string? oldConcurrencyStamp = gameInfo.RelatedSites
                .FirstOrDefault(x => x.ConcurrencyStamp != concurrencyStamp)?.ConcurrencyStamp;
            await context.RelatedSites.Where(x => x.ConcurrencyStamp == oldConcurrencyStamp).ExecuteDeleteAsync();
            context.GameInfos.Update(gameInfo);
        }

        public async Task UpdateTagsAsync(Expression<Func<GameInfo, bool>> query, List<Tag> tags)
        {
            GameInfo? gameInfo = await context.GameInfos
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(query);
            if (gameInfo == null)
                return;
            var tagNames = tags.Select(t => t.Name).Distinct().ToList();
            Dictionary<string, Tag> existingTags = await context.Tags
                .Where(t => tagNames.Contains(t.Name))
                .ToDictionaryAsync(key => key.Name, value => value);
            foreach (Tag tag in tags)
            {
                if (existingTags.ContainsKey(tag.Name))
                    continue;
                var newTag = new Tag
                {
                    Name = tag.Name
                };
                existingTags[tag.Name] = newTag;
            }

            gameInfo.Tags.Clear();
            foreach (Tag tag in tags)
            {
                if (existingTags.TryGetValue(tag.Name, out Tag? existingTag))
                {
                    gameInfo.Tags.Add(existingTag);
                }
            }
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

        public async Task<List<ReleaseInfo>> GetGameInfoReleaseInfos(Expression<Func<GameInfo, bool>> query)
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

        public Task<GameInfo> UpdateBackgroundImageAsync(int id, string? backgroundImage)
        {
            var entity = new GameInfo
            {
                Id = id
            };
            context.Attach(entity);
            entity.BackgroundImageUrl = backgroundImage;
            context.Entry(entity).Property(x => x.BackgroundImageUrl).IsModified = true;
            return Task.FromResult(entity);
        }

        public async Task<GameInfo?> RemoveScreenshotAsync(int id, string url)
        {
            GameInfo? entity = await context.GameInfos.FindAsync(id);
            if (entity == null)
                return null;
            entity.ScreenShots.RemoveAll(x => x == url);
            return entity;
        }

        public async Task<GameInfo?> AddScreenshotsAsync(int id, List<string> urls)
        {
            GameInfo? entity = await context.GameInfos.FindAsync(id);
            if (entity == null)
                return null;
            entity.ScreenShots.AddRange(urls);
            entity.ScreenShots = entity.ScreenShots.Distinct().ToList();
            context.GameInfos.Update(entity);
            return entity;
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