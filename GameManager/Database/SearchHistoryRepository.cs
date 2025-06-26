using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public class SearchHistoryRepository(AppDbContext dbContext) : ISearchHistoryRepository
    {
        public Task<IEnumerable<SearchHistory>> GetManyAsync(Expression<Func<SearchHistory, bool>> query,
            Func<IQueryable<SearchHistory>, IQueryable<SearchHistory>>? includeFunc = null)
        {
            IQueryable<SearchHistory> queryable = dbContext.SearchHistories
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult<IEnumerable<SearchHistory>>(queryable);
        }

        public Task<IEnumerable<TSelect>> GetManyAsync<TSelect>(Expression<Func<SearchHistory, bool>> query,
            Func<IQueryable<SearchHistory>, IQueryable<SearchHistory>> includeFunc,
            Func<IQueryable<SearchHistory>, IEnumerable<TSelect>> selectFunc)
        {
            IQueryable<SearchHistory> queryable = dbContext.SearchHistories
                .AsNoTracking();
            queryable = queryable.Where(query);
            queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable));
        }

        public Task<SearchHistory?> GetAsync(Expression<Func<SearchHistory, bool>> query,
            Func<IQueryable<SearchHistory>, IQueryable<SearchHistory>>? includeFunc = null)
        {
            IQueryable<SearchHistory> queryable = dbContext.SearchHistories
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<TSelect?> GetAsync<TSelect>(Expression<Func<SearchHistory, bool>> query,
            Func<IQueryable<SearchHistory>, IQueryable<SearchHistory>>? includeFunc,
            Func<IQueryable<SearchHistory>, IQueryable<TSelect>> selectFunc)
        {
            IQueryable<SearchHistory> queryable = dbContext.SearchHistories
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable).FirstOrDefault());
        }

        public Task<SearchHistory?> GetAsync(int id,
            Func<IQueryable<SearchHistory>, IQueryable<SearchHistory>>? includeFunc = null)
        {
            IQueryable<SearchHistory> queryable = dbContext.SearchHistories
                .AsNoTracking();
            queryable = queryable.Where(x => x.Id == id);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<TSelect?> GetAsync<TSelect>(int id,
            Func<IQueryable<SearchHistory>, IQueryable<SearchHistory>>? includeFunc,
            Func<IQueryable<SearchHistory>, IQueryable<TSelect>> selectFunc)
        {
            IQueryable<SearchHistory> queryable = dbContext.SearchHistories
                .AsNoTracking();
            queryable = queryable.Where(x => x.Id == id);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable).FirstOrDefault());
        }

        public Task<SearchHistory> AddAsync(SearchHistory entity)
        {
            entity.Id = 0;
            EntityEntry<SearchHistory> entry = dbContext.SearchHistories.Add(entity);
            return Task.FromResult(entry.Entity);
        }

        public Task AddManyAsync(List<SearchHistory> entities)
        {
            dbContext.SearchHistories.AddRange(entities);
            return Task.CompletedTask;
        }

        public Task<bool> AnyAsync(Expression<Func<SearchHistory, bool>> query)
        {
            return dbContext.SearchHistories.AnyAsync(query);
        }

        public Task<SearchHistory> UpdateAsync(SearchHistory originEntity, SearchHistory info)
        {
            dbContext.Entry(originEntity).CurrentValues.SetValues(info);
            dbContext.Entry(originEntity).State = EntityState.Modified;
            return Task.FromResult(originEntity);
        }

        public Task<SearchHistory?> DeleteAsync(int id)
        {
            SearchHistory? item = dbContext.SearchHistories
                .FirstOrDefault(x => x.Id == id);
            if (item == null)
                return Task.FromResult<SearchHistory?>(null);
            dbContext.SearchHistories.Remove(item);
            return Task.FromResult<SearchHistory?>(item);
        }

        public Task<SearchHistory?> DeleteAsync(Expression<Func<SearchHistory, bool>> query)
        {
            SearchHistory? item = dbContext.SearchHistories
                .FirstOrDefault(query);
            if (item == null)
                return Task.FromResult<SearchHistory?>(null);
            dbContext.SearchHistories.Remove(item);
            return Task.FromResult<SearchHistory?>(item);
        }

        public Task UpdatePropertiesAsync(SearchHistory entity,
            params Expression<Func<SearchHistory, object?>>[] properties)
        {
            dbContext.Attach(entity);
            foreach (Expression<Func<SearchHistory, object?>> property in properties)
                dbContext.Entry(entity).Property(property).IsModified = true;
            return Task.CompletedTask;
        }

        public Task<int> CountAsync(Expression<Func<SearchHistory, bool>> queryExpression)
        {
            return dbContext.SearchHistories.CountAsync(queryExpression);
        }
    }
}