using GameManager.DB;
using GameManager.DB.Models;
using System.Linq.Expressions;

namespace GameManager.Database
{
    internal class TagRepository(AppDbContext context) : ITagRepository
    {
        private readonly BaseRepoController<Tag> _baseRepoController = new(context.Set<Tag>());

        public Task<IEnumerable<Tag>> GetManyAsync(Expression<Func<Tag, bool>> query)
        {
            return _baseRepoController.GetManyAsync(query);
        }

        public Task<IQueryable<Tag>> GetAsQueryableAsync(Expression<Func<Tag, bool>> query)
        {
            return _baseRepoController.GetAsQueryableAsync(query);
        }

        public Task<Tag?> GetAsync(Expression<Func<Tag, bool>> query)
        {
            return _baseRepoController.GetAsync(query);
        }

        public Task<Tag?> GetAsync(int id)
        {
            return _baseRepoController.GetAsync(id);
        }

        public Task<Tag> AddAsync(Tag entity)
        {
            return _baseRepoController.AddAsync(entity);
        }

        public Task AddManyAsync(List<Tag> entities)
        {
            return _baseRepoController.AddManyAsync(entities);
        }

        public Task<bool> AnyAsync(Expression<Func<Tag, bool>> query)
        {
            return _baseRepoController.AnyAsync(query);
        }

        public Task<Tag> UpdateAsync(Tag originEntity, Tag newEntity)
        {
            return _baseRepoController.UpdateAsync(originEntity, newEntity);
        }

        public Task<Tag?> DeleteAsync(int id)
        {
            return _baseRepoController.DeleteAsync(id);
        }

        public Task<Tag?> DeleteAsync(Expression<Func<Tag, bool>> query)
        {
            return _baseRepoController.DeleteAsync(query);
        }

        public Task UpdatePropertiesAsync(Tag entity, params Expression<Func<Tag, object?>>[] properties)
        {
            return _baseRepoController.UpdatePropertiesAsync(entity);
        }

        public Task<int> CountAsync(Expression<Func<Tag, bool>> queryExpression)
        {
            return _baseRepoController.CountAsync(queryExpression);
        }

        public Task<int> CountAsync()
        {
            return _baseRepoController.CountAsync(x => true);
        }
    }
}