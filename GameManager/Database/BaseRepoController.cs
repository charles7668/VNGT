using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public class BaseRepoController<TEntity>(DbSet<TEntity> dbSet) : IBaseRepository<TEntity> where TEntity : class
    {
        public Task<IEnumerable<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> query)
        {
            return Task.FromResult<IEnumerable<TEntity>>(dbSet.AsNoTracking().Where(query));
        }

        public Task<IQueryable<TEntity>> GetAsQueryableAsync(Expression<Func<TEntity, bool>> query)
        {
            return Task.FromResult(dbSet.AsNoTracking().Where(query));
        }

        public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> query)
        {
            return Task.FromResult(dbSet.AsNoTracking().FirstOrDefault(query));
        }

        public Task<TEntity?> GetAsync(int id)
        {
            return Task.FromResult(dbSet.Find(id));
        }

        public Task<TEntity> AddAsync(TEntity entity)
        {
            EntityEntry<TEntity> entry = dbSet.Add(entity);
            return Task.FromResult(entry.Entity);
        }

        public Task AddManyAsync(List<TEntity> entities)
        {
            return dbSet.AddRangeAsync(entities);
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> query)
        {
            return Task.FromResult(dbSet.Any(query));
        }

        public Task<TEntity> UpdateAsync(TEntity originEntity, TEntity newEntity)
        {
            dbSet.Entry(originEntity).CurrentValues.SetValues(newEntity);
            dbSet.Entry(originEntity).State = EntityState.Modified;
            return Task.FromResult(originEntity);
        }

        public Task<TEntity?> DeleteAsync(int id)
        {
            TEntity? entity = dbSet.Find(id);
            if (entity != null)
            {
                dbSet.Remove(entity);
            }

            return Task.FromResult(entity);
        }

        public Task<TEntity?> DeleteAsync(Expression<Func<TEntity, bool>> query)
        {
            TEntity? entity = dbSet.FirstOrDefault(query);
            if (entity != null)
            {
                dbSet.Remove(entity);
            }

            return Task.FromResult(entity);
        }

        public Task UpdatePropertiesAsync(TEntity entity, params Expression<Func<TEntity, object?>>[] properties)
        {
            dbSet.Attach(entity);
            foreach (Expression<Func<TEntity, object?>> property in properties)
                dbSet.Entry(entity).Property(property).IsModified = true;
            return Task.CompletedTask;
        }

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> queryExpression)
        {
            return Task.FromResult(dbSet.Count(queryExpression));
        }

        public Task<int> CountAsync()
        {
            return Task.FromResult(dbSet.Count());
        }
    }
}