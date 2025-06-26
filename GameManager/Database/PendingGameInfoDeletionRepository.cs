using GameManager.DB;
using GameManager.DB.Models;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public class PendingGameInfoDeletionRepository(AppDbContext context) : IPendingGameInfoDeletionRepository
    {
        private readonly BaseRepoController<PendingGameInfoDeletion> _baseRepoController =
            new(context.Set<PendingGameInfoDeletion>());

        public Task<IEnumerable<PendingGameInfoDeletion>> GetManyAsync(
            Expression<Func<PendingGameInfoDeletion, bool>> query)
        {
            return _baseRepoController.GetManyAsync(query);
        }

        public Task<IQueryable<PendingGameInfoDeletion>> GetAsQueryableAsync(
            Expression<Func<PendingGameInfoDeletion, bool>> query)
        {
            return _baseRepoController.GetAsQueryableAsync(query);
        }

        public Task<PendingGameInfoDeletion?> GetAsync(Expression<Func<PendingGameInfoDeletion, bool>> query)
        {
            return _baseRepoController.GetAsync(query);
        }

        public Task<PendingGameInfoDeletion?> GetAsync(int id)
        {
            return _baseRepoController.GetAsync(id);
        }

        public Task<PendingGameInfoDeletion> AddAsync(PendingGameInfoDeletion entity)
        {
            return _baseRepoController.AddAsync(entity);
        }

        public Task AddManyAsync(List<PendingGameInfoDeletion> entities)
        {
            return _baseRepoController.AddManyAsync(entities);
        }

        public Task<bool> AnyAsync(Expression<Func<PendingGameInfoDeletion, bool>> query)
        {
            return _baseRepoController.AnyAsync(query);
        }

        public Task<PendingGameInfoDeletion> UpdateAsync(PendingGameInfoDeletion originEntity,
            PendingGameInfoDeletion newEntity)
        {
            return _baseRepoController.UpdateAsync(originEntity, newEntity);
        }

        public Task<PendingGameInfoDeletion?> DeleteAsync(int id)
        {
            return _baseRepoController.DeleteAsync(id);
        }

        public Task<PendingGameInfoDeletion?> DeleteAsync(Expression<Func<PendingGameInfoDeletion, bool>> query)
        {
            return _baseRepoController.DeleteAsync(query);
        }

        public Task UpdatePropertiesAsync(PendingGameInfoDeletion entity,
            params Expression<Func<PendingGameInfoDeletion, object?>>[] properties)
        {
            return _baseRepoController.UpdatePropertiesAsync(entity, properties);
        }

        public Task<int> CountAsync(Expression<Func<PendingGameInfoDeletion, bool>> queryExpression)
        {
            return _baseRepoController.CountAsync(queryExpression);
        }

        public Task<int> CountAsync()
        {
            return _baseRepoController.CountAsync();
        }
    }
}