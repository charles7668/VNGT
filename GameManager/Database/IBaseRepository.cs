using JetBrains.Annotations;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public interface IBaseRepository<TEntity>
    {
        [UsedImplicitly]
        Task<IEnumerable<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> query);

        [UsedImplicitly]
        Task<IQueryable<TEntity>> GetAsQueryableAsync(Expression<Func<TEntity, bool>> query);

        [UsedImplicitly]
        Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> query);
        
        [UsedImplicitly]
        Task<TEntity?> GetAsync(int id);
        
        [UsedImplicitly]
        Task<TEntity> AddAsync(TEntity entity);

        [UsedImplicitly]
        Task AddManyAsync(List<TEntity> entities);

        [UsedImplicitly]
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> query);

        [UsedImplicitly]
        Task<TEntity> UpdateAsync(TEntity originEntity, TEntity newEntity);

        [UsedImplicitly]
        Task<TEntity?> DeleteAsync(int id);

        [UsedImplicitly]
        Task<TEntity?> DeleteAsync(Expression<Func<TEntity, bool>> query);

        [UsedImplicitly]
        Task UpdatePropertiesAsync(TEntity entity, params Expression<Func<TEntity, object?>>[] properties);

        [UsedImplicitly]
        Task<int> CountAsync(Expression<Func<TEntity, bool>> queryExpression);

        [UsedImplicitly]
        Task<int> CountAsync();
    }
}