using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace GameManager.Database
{
    internal class LibraryRepository(AppDbContext dbContext) : ILibraryRepository
    {
        public Task<IEnumerable<Library>> GetManyAsync(Expression<Func<Library, bool>> query,
            Func<IQueryable<Library>, IQueryable<Library>>? includeFunc = null)
        {
            IQueryable<Library> queryable = dbContext.Libraries
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult<IEnumerable<Library>>(queryable);
        }

        public Task<IEnumerable<TSelect>> GetManyAsync<TSelect>(Expression<Func<Library, bool>> query,
            Func<IQueryable<Library>, IQueryable<Library>> includeFunc,
            Func<IQueryable<Library>, IEnumerable<TSelect>> selectFunc)
        {
            IQueryable<Library> queryable = dbContext.Libraries
                .AsNoTracking();
            queryable = queryable.Where(query);
            queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable));
        }

        public Task<Library?> GetAsync(Expression<Func<Library, bool>> query,
            Func<IQueryable<Library>, IQueryable<Library>>? includeFunc = null)
        {
            IQueryable<Library> queryable = dbContext.Libraries
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<TSelect?> GetAsync<TSelect>(Expression<Func<Library, bool>> query,
            Func<IQueryable<Library>, IQueryable<Library>>? includeFunc,
            Func<IQueryable<Library>, IQueryable<TSelect>> selectFunc)
        {
            IQueryable<Library> queryable = dbContext.Libraries
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable).FirstOrDefault());
        }

        public Task<Library?> GetAsync(int id, Func<IQueryable<Library>, IQueryable<Library>>? includeFunc = null)
        {
            IQueryable<Library> queryable = dbContext.Libraries
                .AsNoTracking();
            queryable = queryable.Where(x => x.Id == id);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<TSelect?> GetAsync<TSelect>(int id, Func<IQueryable<Library>, IQueryable<Library>>? includeFunc,
            Func<IQueryable<Library>, IQueryable<TSelect>> selectFunc)
        {
            IQueryable<Library> queryable = dbContext.Libraries
                .AsNoTracking();
            queryable = queryable.Where(x => x.Id == id);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable).FirstOrDefault());
        }

        public Task<Library> AddAsync(Library entity)
        {
            entity.Id = 0;
            EntityEntry<Library> entry = dbContext.Libraries.Add(entity);
            return Task.FromResult(entry.Entity);
        }

        public Task AddManyAsync(List<Library> entities)
        {
            dbContext.Libraries.AddRange(entities);
            return Task.CompletedTask;
        }

        public Task<bool> AnyAsync(Expression<Func<Library, bool>> query)
        {
            return dbContext.Libraries.AnyAsync(query);
        }

        public Task<Library> UpdateAsync(Library originEntity, Library info)
        {
            dbContext.Entry(originEntity).CurrentValues.SetValues(info);
            dbContext.Entry(originEntity).State = EntityState.Modified;
            return Task.FromResult(originEntity);
        }

        public Task<Library?> DeleteAsync(int id)
        {
            Library? item = dbContext.Libraries
                .FirstOrDefault(x => x.Id == id);
            if (item == null)
                return Task.FromResult<Library?>(null);
            dbContext.Libraries.Remove(item);
            return Task.FromResult<Library?>(item);
        }

        public Task<Library?> DeleteAsync(Expression<Func<Library, bool>> query)
        {
            Library? item = dbContext.Libraries
                .FirstOrDefault(query);
            if (item == null)
                return Task.FromResult<Library?>(null);
            dbContext.Libraries.Remove(item);
            return Task.FromResult<Library?>(item);
        }

        public Task UpdatePropertiesAsync(Library entity, params Expression<Func<Library, object?>>[] properties)
        {
            dbContext.Attach(entity);
            foreach (Expression<Func<Library, object?>> property in properties)
                dbContext.Entry(entity).Property(property).IsModified = true;
            return Task.CompletedTask;
        }

        public Task<int> CountAsync(Expression<Func<Library, bool>> queryExpression)
        {
            return dbContext.Libraries.CountAsync(queryExpression);
        }
    }
}