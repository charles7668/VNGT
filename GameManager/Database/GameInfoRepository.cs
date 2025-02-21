using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public class GameInfoRepository(AppDbContext context) : IGameInfoRepository
    {
        public async Task<GameInfo> AddAsync(GameInfo entity)
        {
            EntityEntry<GameInfo> entityEntry = await context.GameInfos.AddAsync(entity);
            return entityEntry.Entity;
        }

        public Task AddManyAsync(List<GameInfo> entities)
        {
            context.GameInfos.AddRange(entities);
            return Task.CompletedTask;
        }

        public Task<bool> AnyAsync(Expression<Func<GameInfo, bool>> query)
        {
            return context.GameInfos.AnyAsync(query);
        }

        public Task<GameInfo> UpdateAsync(GameInfo originEntity, GameInfo info)
        {
            context.Entry(originEntity).CurrentValues.SetValues(info);
            context.Entry(originEntity).State = EntityState.Modified; // 強制標記為已修改

            return Task.FromResult(originEntity);
        }

        public async Task<GameInfo?> DeleteAsync(int id)
        {
            GameInfo? entity = await GetAsync(id,
                q =>
                {
                    return q.Include(x => x.LaunchOption);
                });
            if (entity == null)
                return null;

            context.GameInfos.Remove(entity);
            if (entity.LaunchOption != null)
                context.LaunchOptions.Remove(entity.LaunchOption);
            return entity;
        }

        public Task<GameInfo?> DeleteAsync(Expression<Func<GameInfo, bool>> query)
        {
            GameInfo? entity = context.GameInfos.FirstOrDefault(query);
            if (entity == null)
                return Task.FromResult<GameInfo?>(null);
            context.GameInfos.Remove(entity);
            return Task.FromResult<GameInfo?>(entity);
        }

        public Task UpdatePropertiesAsync(GameInfo entity,
            params Expression<Func<GameInfo, object?>>[] properties)
        {
            context.Attach(entity);
            foreach (Expression<Func<GameInfo, object?>> prop in properties)
            {
                context.Entry(entity).Property(prop).IsModified = true;
            }

            return Task.CompletedTask;
        }

        public async Task UpdateStaffsAsync(GameInfo entity, List<Staff> staffs)
        {
            GameInfo gameInfo = context.Entry(entity).Entity;
            await context.Entry(entity).Collection(x => x.Staffs).LoadAsync();
            List<Staff> existingStaffs = gameInfo.Staffs;
            var staffRoles = context.StaffRoles
                .ToDictionary(x => x.Id, x => x);

            var existingStaffDict = existingStaffs
                .GroupBy(s => new
                {
                    s.Name,
                    s.StaffRoleId
                })
                .ToDictionary(g => g.Key, g => g.First());

            await context.GameInfoStaffs.Where(x => x.GameInfoId == gameInfo.Id)
                .ExecuteDeleteAsync();
            gameInfo.Staffs.Clear();
            foreach (Staff staff in staffs)
            {
                var key = new
                {
                    staff.Name,
                    StaffRoleId = staff.StaffRole.Id
                };
                Staff? staffInDb = await context.Staffs
                    .Include(x => x.StaffRole)
                    .FirstOrDefaultAsync(x => x.Name == staff.Name && x.StaffRoleId == staff.StaffRole.Id);
                if (staffInDb != null)
                {
                    gameInfo.Staffs.Add(staffInDb);
                    existingStaffDict.Remove(key);
                    continue;
                }

                // if already exists in the list, skip
                if (gameInfo.Staffs.Any(x => x.Name == staff.Name && x.StaffRole.Id == staff.StaffRole.Id))
                    continue;

                var newStaff = new Staff
                {
                    Id = 0,
                    Name = staff.Name,
                    StaffRole = staffRoles[staff.StaffRole.Id],
                    StaffRoleId = staff.StaffRole.Id
                };
                gameInfo.Staffs.Add(newStaff);
            }

            foreach (var (_, value) in existingStaffDict)
            {
                await context.GameInfoStaffs.Where(x => x.GameInfoId == gameInfo.Id && x.StaffId == value.Id)
                    .ExecuteDeleteAsync();
            }
        }

        public async Task UpdateCharactersAsync(GameInfo entity,
            List<Character> characters)
        {
            GameInfo gameInfo = context.Entry(entity).Entity;
            await context.Entry(entity).Collection(x => x.Characters).LoadAsync();
            string concurrencyStamp = Guid.NewGuid().ToString();
            await context.Characters.Where(x => x.GameInfoId == gameInfo.Id).ExecuteDeleteAsync();
            gameInfo.Characters.Clear();
            foreach (Character character in characters)
            {
                character.Id = 0;
                character.GameInfoId = gameInfo.Id;
                character.ConcurrencyStamp = concurrencyStamp;
                EntityEntry<Character> entry = context.Characters.Add(character);
                gameInfo.Characters.Add(entry.Entity);
            }
        }

        public async Task UpdateReleaseInfosAsync(GameInfo entity,
            List<ReleaseInfo> releaseInfos)
        {
            GameInfo gameInfo = context.Entry(entity).Entity;
            await context.Entry(entity).Collection(x => x.ReleaseInfos)
                .LoadAsync();
            string concurrencyStamp = Guid.NewGuid().ToString();
            foreach (ReleaseInfo releaseInfo in gameInfo.ReleaseInfos)
            {
                await context.ExternalLinks.Where(x => x.ReleaseInfoId == releaseInfo.Id)
                    .ExecuteDeleteAsync();
            }

            await context.ReleaseInfos.Where(x => x.GameInfoId == gameInfo.Id)
                .ExecuteDeleteAsync();
            gameInfo.ReleaseInfos.Clear();
            foreach (ReleaseInfo releaseInfo in releaseInfos)
            {
                releaseInfo.Id = 0;
                releaseInfo.GameInfoId = gameInfo.Id;
                releaseInfo.ConcurrencyStamp = concurrencyStamp;
                var externalLinks = new List<EntityEntry<ExternalLink>>();
                foreach (ExternalLink externalLink in releaseInfo.ExternalLinks)
                {
                    externalLink.Id = 0;
                    externalLink.ConcurrencyStamp = concurrencyStamp;
                    externalLinks.Add(await context.ExternalLinks.AddAsync(externalLink));
                }

                releaseInfo.ExternalLinks = externalLinks.Select(x => x.Entity).ToList();
                EntityEntry<ReleaseInfo> entry = context.ReleaseInfos.Add(releaseInfo);

                gameInfo.ReleaseInfos.Add(entry.Entity);
            }
        }

        public async Task UpdateRelatedSitesAsync(GameInfo entity,
            List<RelatedSite> relatedSites)
        {
            GameInfo gameInfo = context.Entry(entity).Entity;
            await context.Entry(entity).Collection(x => x.RelatedSites).LoadAsync();
            string concurrencyStamp = Guid.NewGuid().ToString();
            await context.RelatedSites.Where(x => x.GameInfoId == gameInfo.Id)
                .ExecuteDeleteAsync();
            gameInfo.RelatedSites.Clear();
            foreach (RelatedSite relatedSite in relatedSites)
            {
                relatedSite.Id = 0;
                relatedSite.GameInfoId = gameInfo.Id;
                relatedSite.ConcurrencyStamp = concurrencyStamp;
                EntityEntry<RelatedSite> entry = await context.RelatedSites.AddAsync(relatedSite);
                gameInfo.RelatedSites.Add(entry.Entity);
            }
        }

        public async Task UpdateTagsAsync(GameInfo entity, List<Tag> tags)
        {
            GameInfo gameInfo = context.Entry(entity).Entity;
            var tagNames = tags.Select(t => t.Name).Distinct().ToHashSet();
            var existingTags = gameInfo.Tags
                .ToDictionary(key => key.Name, value => value);
            var existInDb = context.Tags.AsNoTracking()
                .Where(x => tagNames.Contains(x.Name))
                .ToDictionary(x => x.Name, x => x);
            foreach (string tag in tagNames)
            {
                if (existingTags.ContainsKey(tag))
                    continue;
                if (existInDb.TryGetValue(tag, out Tag? existingTag))
                {
                    gameInfo.Tags.Add(existingTag);
                    existingTags[tag] = existingTag;
                    continue;
                }

                var newTag = new Tag
                {
                    Name = tag
                };
                EntityEntry<Tag> entry = context.Tags.Add(newTag);
                gameInfo.Tags.Add(entry.Entity);
                existingTags[tag] = entry.Entity;
            }

            await context.GameInfoTags.Where(x => x.GameInfoId == gameInfo.Id)
                .ExecuteDeleteAsync();
            await context.GameInfoTags.AddRangeAsync(gameInfo.Tags.Select(tag =>
                new GameInfoTag
                {
                    GameInfoId = gameInfo.Id,
                    TagId = tag.Id
                }));
        }

        public Task UpdateLaunchOption(GameInfo entity, LaunchOption launchOption)
        {
            GameInfo gameInfo = context.Entry(entity).Entity;
            context.Entry(gameInfo).Reference(x => x.LaunchOption).Load();
            if (gameInfo.LaunchOption == null)
            {
                // set to 0 to indicate it's a new launch option
                launchOption.Id = 0;
                EntityEntry<LaunchOption> optionEntry = context.LaunchOptions.Add(launchOption);
                gameInfo.LaunchOption = optionEntry.Entity;
                return Task.CompletedTask;
            }

            launchOption.Id = gameInfo.LaunchOption.Id;
            context.Entry(gameInfo.LaunchOption).CurrentValues.SetValues(launchOption);
            context.Entry(gameInfo.LaunchOption).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Staff>> GetStaffs(int gameInfoId)
        {
            IQueryable<int> staffList = context.GameInfoStaffs
                .Where(x => x.GameInfoId == gameInfoId).Select(x => x.StaffId);
            return Task.FromResult<IEnumerable<Staff>>(context.Staffs.Include(x => x.StaffRole).Where(x => staffList.Contains(x.Id)));
        }

        public Task<IEnumerable<Character>> GetCharacters(int gameInfoId)
        {
            return Task.FromResult<IEnumerable<Character>>(context.Characters.Where(x => x.GameInfoId == gameInfoId));
        }

        public Task<IEnumerable<ReleaseInfo>> GetReleaseInfos(int gameInfoId)
        {
            return Task.FromResult<IEnumerable<ReleaseInfo>>(context.ReleaseInfos
                .Include(x => x.ExternalLinks)
                .Where(x => x.GameInfoId == gameInfoId));
        }

        public Task<IEnumerable<RelatedSite>> GetRelatedSites(int gameInfoId)
        {
            return Task.FromResult<IEnumerable<RelatedSite>>(
                context.RelatedSites.Where(x => x.GameInfoId == gameInfoId));
        }

        public Task<IEnumerable<Tag>> GetTags(int gameInfoId)
        {
            IQueryable<int> tagIds = context.GameInfoTags
                .Where(x => x.GameInfoId == gameInfoId)
                .Select(x => x.TagId);
            return Task.FromResult<IEnumerable<Tag>>(context.Tags.Where(x => tagIds.Contains(x.Id)));
        }

        public Task<int> CountAsync(Expression<Func<GameInfo, bool>> queryExpression)
        {
            return context.GameInfos.CountAsync(queryExpression);
        }

        public Task<IEnumerable<GameInfo>> GetManyAsync(Expression<Func<GameInfo, bool>> query,
            Func<IQueryable<GameInfo>, IQueryable<GameInfo>>? includeFunc = null)
        {
            IQueryable<GameInfo> queryable = context.GameInfos
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            queryable = queryable.OrderByDescending(x => x.UploadTime);
            return Task.FromResult(queryable.AsEnumerable());
        }

        public Task<IEnumerable<TEntity>> GetManyAsync<TEntity>(Expression<Func<GameInfo, bool>> query,
            Func<IQueryable<GameInfo>, IQueryable<GameInfo>> includeFunc,
            Func<IQueryable<GameInfo>, IEnumerable<TEntity>> selectFunc)
        {
            IQueryable<GameInfo> queryable = context.GameInfos
                .AsNoTracking();
            queryable = queryable.Where(query);
            queryable = includeFunc(queryable);
            queryable = queryable.OrderByDescending(x => x.UploadTime);
            return Task.FromResult(selectFunc(queryable));
        }

        public Task<GameInfo?> GetAsync(Expression<Func<GameInfo, bool>> query,
            Func<IQueryable<GameInfo>, IQueryable<GameInfo>>? includeFunc = null)
        {
            IQueryable<GameInfo> queryable = context.GameInfos
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return queryable.FirstOrDefaultAsync();
        }

        public Task<TEntity?> GetAsync<TEntity>(Expression<Func<GameInfo, bool>> query,
            Func<IQueryable<GameInfo>, IQueryable<GameInfo>>? includeFunc,
            Func<IQueryable<GameInfo>, IQueryable<TEntity>> selectFunc)
        {
            IQueryable<GameInfo> queryable = context.GameInfos
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable).FirstOrDefault());
        }

        public Task<GameInfo?> GetAsync(int id, Func<IQueryable<GameInfo>, IQueryable<GameInfo>>? includeFunc = null)
        {
            IQueryable<GameInfo> queryable = context.GameInfos
                .AsNoTracking();
            queryable = queryable.Where(x => x.Id == id);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return queryable.FirstOrDefaultAsync();
        }

        public Task<TEntity?> GetAsync<TEntity>(int id,
            Func<IQueryable<GameInfo>, IQueryable<GameInfo>>? includeFunc,
            Func<IQueryable<GameInfo>, IQueryable<TEntity>> selectFunc)
        {
            IQueryable<GameInfo> queryable = context.GameInfos
                .AsNoTracking();
            queryable = queryable.Where(x => x.Id == id);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable).FirstOrDefault());
        }
    }
}