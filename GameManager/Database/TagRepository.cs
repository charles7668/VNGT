using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace GameManager.Database
{
    internal class TagRepository(AppDbContext context) : ITagRepository
    {
        public Task<IEnumerable<Tag>> GetManyAsync(Expression<Func<Tag, bool>> query,
            Func<IQueryable<Tag>, IQueryable<Tag>>? includeFunc = null)
        {
            IQueryable<Tag> queryable = context.Tags
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult<IEnumerable<Tag>>(queryable);
        }

        public Task<IEnumerable<TSelect>> GetManyAsync<TSelect>(Expression<Func<Tag, bool>> query,
            Func<IQueryable<Tag>, IQueryable<Tag>> includeFunc, Func<IQueryable<Tag>, IEnumerable<TSelect>> selectFunc)
        {
            IQueryable<Tag> queryable = context.Tags
                .AsNoTracking();
            queryable = queryable.Where(query);
            queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable));
        }

        public Task<Tag?> GetAsync(Expression<Func<Tag, bool>> query,
            Func<IQueryable<Tag>, IQueryable<Tag>>? includeFunc = null)
        {
            IQueryable<Tag> queryable = context.Tags
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<TSelect?> GetAsync<TSelect>(Expression<Func<Tag, bool>> query,
            Func<IQueryable<Tag>, IQueryable<Tag>>? includeFunc, Func<IQueryable<Tag>, IQueryable<TSelect>> selectFunc)
        {
            IQueryable<Tag> queryable = context.Tags
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable).FirstOrDefault());
        }

        public Task<Tag?> GetAsync(int id, Func<IQueryable<Tag>, IQueryable<Tag>>? includeFunc = null)
        {
            IQueryable<Tag> queryable = context.Tags
                .AsNoTracking();
            queryable = queryable.Where(x => x.Id == id);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<TSelect?> GetAsync<TSelect>(int id, Func<IQueryable<Tag>, IQueryable<Tag>>? includeFunc,
            Func<IQueryable<Tag>, IQueryable<TSelect>> selectFunc)
        {
            IQueryable<Tag> queryable = context.Tags
                .AsNoTracking();
            queryable = queryable.Where(x => x.Id == id);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable).FirstOrDefault());
        }

        public Task<Tag> AddAsync(Tag entity)
        {
            Tag? existingTag = context.Tags.FirstOrDefault(x => x.Name == entity.Name);
            if (existingTag != null)
                return Task.FromResult(existingTag);
            entity.Id = 0;
            EntityEntry<Tag> entityEntry = context.Tags.Add(entity);
            return Task.FromResult(entityEntry.Entity);
        }

        public Task AddManyAsync(List<Tag> entities)
        {
            var addPending = new List<Tag>();
            foreach (Tag entity in entities)
            {
                bool exist = context.Tags.Any(x => x.Name == entity.Name);
                if (exist)
                    continue;
                addPending.Add(new Tag
                {
                    Name = entity.Name
                });
            }

            context.Tags.AddRange(addPending);
            return Task.CompletedTask;
        }

        public Task<bool> AnyAsync(Expression<Func<Tag, bool>> query)
        {
            return Task.FromResult(context.Tags.Any(query));
        }

        public Task<Tag> UpdateAsync(Tag originEntity, Tag newEntity)
        {
            context.Entry(originEntity).CurrentValues.SetValues(newEntity);
            context.Entry(originEntity).State = EntityState.Modified;
            return Task.FromResult(originEntity);
        }

        public Task<Tag?> DeleteAsync(int id)
        {
            Tag? entity = context.Tags.FirstOrDefault(x => x.Id == id);
            if (entity == null)
                return Task.FromResult<Tag?>(null);
            context.Tags.Remove(entity);
            return Task.FromResult<Tag?>(entity);
        }

        public Task<Tag?> DeleteAsync(Expression<Func<Tag, bool>> query)
        {
            Tag? entity = context.Tags.FirstOrDefault(query);
            if (entity == null)
                return Task.FromResult<Tag?>(null);
            context.Tags.Remove(entity);
            return Task.FromResult<Tag?>(entity);
        }

        public Task UpdatePropertiesAsync(Tag entity, params Expression<Func<Tag, object?>>[] properties)
        {
            context.Attach(entity);
            foreach (Expression<Func<Tag, object?>> property in properties)
                context.Entry(entity).Property(property).IsModified = true;
            return Task.CompletedTask;
        }

        public Task<int> CountAsync(Expression<Func<Tag, bool>> queryExpression)
        {
            return Task.FromResult(context.Tags.Count(queryExpression));
        }
    }
}