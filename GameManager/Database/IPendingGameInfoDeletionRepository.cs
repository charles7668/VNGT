using GameManager.DB.Models;

namespace GameManager.Database
{
    public interface IPendingGameInfoDeletionRepository
    {
        public Task<List<PendingGameInfoDeletion>> GetAsync();

        public Task AddAsync(PendingGameInfoDeletion pendingGameInfoDeletion);

        public Task AddAsync(List<PendingGameInfoDeletion> pendingGameInfoDeletion);

        public Task<List<PendingGameInfoDeletion>> RemoveAsync(List<string> uniqueIds);
    }
}