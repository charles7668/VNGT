using GameManager.DB;
using GameManager.DB.Models;
using System.Linq.Expressions;

namespace GameManager.Database
{
    internal class LibraryRepository(AppDbContext dbContext) : ILibraryRepository
    {
        private readonly BaseRepoController<Library> _baseRepoController = new(dbContext.Set<Library>());

        public Task<IEnumerable<Library>> GetManyAsync(Expression<Func<Library, bool>> query)
        {
            return _baseRepoController.GetManyAsync(query);
        }

        public Task<IQueryable<Library>> GetAsQueryableAsync(Expression<Func<Library, bool>> query)
        {
            return _baseRepoController.GetAsQueryableAsync(query);
        }

        public Task<Library?> GetAsync(Expression<Func<Library, bool>> query)
        {
            return _baseRepoController.GetAsync(query);
        }

        public Task<Library?> GetAsync(int id)
        {
            return _baseRepoController.GetAsync(id);
        }

        public Task<Library> AddAsync(Library entity)
        {
            return _baseRepoController.AddAsync(entity);
        }

        public Task AddManyAsync(List<Library> entities)
        {
            return _baseRepoController.AddManyAsync(entities);
        }

        public Task<bool> AnyAsync(Expression<Func<Library, bool>> query)
        {
            return _baseRepoController.AnyAsync(query);
        }

        public Task<Library> UpdateAsync(Library originEntity, Library newEntity)
        {
            return _baseRepoController.UpdateAsync(originEntity, newEntity);
        }

        public Task<Library?> DeleteAsync(int id)
        {
            return _baseRepoController.DeleteAsync(id);
        }

        public Task<Library?> DeleteAsync(Expression<Func<Library, bool>> query)
        {
            return _baseRepoController.DeleteAsync(query);
        }

        public Task UpdatePropertiesAsync(Library entity, params Expression<Func<Library, object?>>[] properties)
        {
            return _baseRepoController.UpdatePropertiesAsync(entity, properties);
        }

        public Task<int> CountAsync(Expression<Func<Library, bool>> queryExpression)
        {
            return _baseRepoController.CountAsync(queryExpression);
        }

        public Task<int> CountAsync()
        {
            return _baseRepoController.CountAsync();
        }
    }
}