using JetBrains.Annotations;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public interface IBaseRepository<TEntity>
    {
        [UsedImplicitly]
        Task<IEnumerable<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> query,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc = null);

        [UsedImplicitly]
        Task<IEnumerable<TSelect>> GetManyAsync<TSelect>(Expression<Func<TEntity, bool>> query,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includeFunc,
            Func<IQueryable<TEntity>, IEnumerable<TSelect>> selectFunc);


        [UsedImplicitly]
        Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> query,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc = null);

        [UsedImplicitly]
        Task<TSelect?> GetAsync<TSelect>(Expression<Func<TEntity, bool>> query,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc,
            Func<IQueryable<TEntity>, IQueryable<TSelect>> selectFunc);

        [UsedImplicitly]
        Task<TEntity?> GetAsync(int id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc = null);

        [UsedImplicitly]
        Task<TSelect?> GetAsync<TSelect>(int id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc,
            Func<IQueryable<TEntity>, IQueryable<TSelect>> selectFunc);

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
    }
}