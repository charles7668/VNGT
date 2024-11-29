using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public class PendingGameInfoDeletionRepository(AppDbContext context) : IPendingGameInfoDeletionRepository
    {
        public Task<IEnumerable<PendingGameInfoDeletion>> GetManyAsync(
            Expression<Func<PendingGameInfoDeletion, bool>> query,
            Func<IQueryable<PendingGameInfoDeletion>, IQueryable<PendingGameInfoDeletion>>? includeFunc = null)
        {
            IQueryable<PendingGameInfoDeletion> queryable = context.PendingGameInfoDeletions
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult<IEnumerable<PendingGameInfoDeletion>>(queryable);
        }

        public Task<IEnumerable<TSelect>> GetManyAsync<TSelect>(Expression<Func<PendingGameInfoDeletion, bool>> query,
            Func<IQueryable<PendingGameInfoDeletion>, IQueryable<PendingGameInfoDeletion>> includeFunc,
            Func<IQueryable<PendingGameInfoDeletion>, IEnumerable<TSelect>> selectFunc)
        {
            IQueryable<PendingGameInfoDeletion> queryable = context.PendingGameInfoDeletions
                .AsNoTracking();
            queryable = queryable.Where(query);
            queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable));
        }

        public Task<PendingGameInfoDeletion?> GetAsync(Expression<Func<PendingGameInfoDeletion, bool>> query,
            Func<IQueryable<PendingGameInfoDeletion>, IQueryable<PendingGameInfoDeletion>>? includeFunc = null)
        {
            IQueryable<PendingGameInfoDeletion> queryable = context.PendingGameInfoDeletions
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<TSelect?> GetAsync<TSelect>(Expression<Func<PendingGameInfoDeletion, bool>> query,
            Func<IQueryable<PendingGameInfoDeletion>, IQueryable<PendingGameInfoDeletion>>? includeFunc,
            Func<IQueryable<PendingGameInfoDeletion>, IQueryable<TSelect>> selectFunc)
        {
            IQueryable<PendingGameInfoDeletion> queryable = context.PendingGameInfoDeletions
                .AsNoTracking();
            queryable = queryable.Where(query);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable).FirstOrDefault());
        }

        public Task<PendingGameInfoDeletion?> GetAsync(int id,
            Func<IQueryable<PendingGameInfoDeletion>, IQueryable<PendingGameInfoDeletion>>? includeFunc = null)
        {
            IQueryable<PendingGameInfoDeletion> queryable = context.PendingGameInfoDeletions
                .AsNoTracking();
            queryable = queryable.Where(x => x.Id == id);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<TSelect?> GetAsync<TSelect>(int id,
            Func<IQueryable<PendingGameInfoDeletion>, IQueryable<PendingGameInfoDeletion>>? includeFunc,
            Func<IQueryable<PendingGameInfoDeletion>, IQueryable<TSelect>> selectFunc)
        {
            IQueryable<PendingGameInfoDeletion> queryable = context.PendingGameInfoDeletions
                .AsNoTracking();
            queryable = queryable.Where(x => x.Id == id);
            if (includeFunc != null)
                queryable = includeFunc(queryable);
            return Task.FromResult(selectFunc(queryable).FirstOrDefault());
        }

        public Task<PendingGameInfoDeletion> AddAsync(PendingGameInfoDeletion entity)
        {
            EntityEntry<PendingGameInfoDeletion> entry = context.PendingGameInfoDeletions.Add(entity);
            return Task.FromResult(entry.Entity);
        }

        public Task AddManyAsync(List<PendingGameInfoDeletion> entities)
        {
            context.PendingGameInfoDeletions.AddRange(entities);
            return Task.CompletedTask;
        }

        public Task<bool> AnyAsync(Expression<Func<PendingGameInfoDeletion, bool>> query)
        {
            return context.PendingGameInfoDeletions.AnyAsync(query);
        }

        public Task<PendingGameInfoDeletion> UpdateAsync(PendingGameInfoDeletion originEntity,
            PendingGameInfoDeletion info)
        {
            context.Entry(originEntity).CurrentValues.SetValues(info);
            return Task.FromResult(originEntity);
        }

        public Task<PendingGameInfoDeletion?> DeleteAsync(int id)
        {
            PendingGameInfoDeletion? entity = context.PendingGameInfoDeletions
                .FirstOrDefault(x => x.Id == id);
            if (entity != null)
                context.PendingGameInfoDeletions.Remove(entity);
            return Task.FromResult(entity);
        }

        public Task<PendingGameInfoDeletion?> DeleteAsync(Expression<Func<PendingGameInfoDeletion, bool>> query)
        {
            PendingGameInfoDeletion? entity = context.PendingGameInfoDeletions
                .FirstOrDefault(query);
            if (entity != null)
                context.PendingGameInfoDeletions.Remove(entity);
            return Task.FromResult(entity);
        }

        public Task UpdatePropertiesAsync(PendingGameInfoDeletion entity,
            params Expression<Func<PendingGameInfoDeletion, object?>>[] properties)
        {
            context.Attach(entity);
            foreach (Expression<Func<PendingGameInfoDeletion, object?>> property in properties)
                context.Entry(entity).Property(property).IsModified = true;
            return Task.CompletedTask;
        }

        public Task<int> CountAsync(Expression<Func<PendingGameInfoDeletion, bool>> queryExpression)
        {
            return context.PendingGameInfoDeletions.CountAsync(queryExpression);
        }
    }
}