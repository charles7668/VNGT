using GameManager.DB;
using GameManager.DB.Models;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public class SearchHistoryRepository(AppDbContext context) : ISearchHistoryRepository
    {
        private readonly BaseRepoController<SearchHistory> _baseRepoController = new(context.Set<SearchHistory>());

        public Task<IEnumerable<SearchHistory>> GetManyAsync(Expression<Func<SearchHistory, bool>> query)
        {
            return _baseRepoController.GetManyAsync(query);
        }

        public Task<IQueryable<SearchHistory>> GetAsQueryableAsync(Expression<Func<SearchHistory, bool>> query)
        {
            return _baseRepoController.GetAsQueryableAsync(query);
        }

        public Task<SearchHistory?> GetAsync(Expression<Func<SearchHistory, bool>> query)
        {
            return _baseRepoController.GetAsync(query);
        }

        public Task<SearchHistory?> GetAsync(int id)
        {
            return _baseRepoController.GetAsync(id);
        }

        public Task<SearchHistory> AddAsync(SearchHistory entity)
        {
            return _baseRepoController.AddAsync(entity);
        }

        public Task AddManyAsync(List<SearchHistory> entities)
        {
            return _baseRepoController.AddManyAsync(entities);
        }

        public Task<bool> AnyAsync(Expression<Func<SearchHistory, bool>> query)
        {
            return _baseRepoController.AnyAsync(query);
        }

        public Task<SearchHistory> UpdateAsync(SearchHistory originEntity, SearchHistory newEntity)
        {
            return _baseRepoController.UpdateAsync(originEntity, newEntity);
        }

        public Task<SearchHistory?> DeleteAsync(int id)
        {
            return _baseRepoController.DeleteAsync(id);
        }

        public Task<SearchHistory?> DeleteAsync(Expression<Func<SearchHistory, bool>> query)
        {
            return _baseRepoController.DeleteAsync(query);
        }

        public Task UpdatePropertiesAsync(SearchHistory entity,
            params Expression<Func<SearchHistory, object?>>[] properties)
        {
            return _baseRepoController.UpdatePropertiesAsync(entity, properties);
        }

        public Task<int> CountAsync(Expression<Func<SearchHistory, bool>> queryExpression)
        {
            return _baseRepoController.CountAsync(queryExpression);
        }

        public Task<int> CountAsync()
        {
            return _baseRepoController.CountAsync();
        }
    }
}