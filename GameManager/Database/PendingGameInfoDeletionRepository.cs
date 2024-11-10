using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace GameManager.Database
{
    public class PendingGameInfoDeletionRepository(AppDbContext context) : IPendingGameInfoDeletionRepository
    {
        public Task<List<PendingGameInfoDeletion>> GetAsync()
        {
            return context.PendingGameInfoDeletions.ToListAsync();
        }

        public Task AddAsync(PendingGameInfoDeletion pendingGameInfoDeletion)
        {
            context.PendingGameInfoDeletions.Add(pendingGameInfoDeletion);
            return Task.CompletedTask;
        }

        public Task AddAsync(List<PendingGameInfoDeletion> pendingGameInfoDeletion)
        {
            context.PendingGameInfoDeletions.AddRange(pendingGameInfoDeletion);
            return Task.CompletedTask;
        }

        public Task<List<PendingGameInfoDeletion>> RemoveAsync(List<string> uniqueIds)
        {
            var pendingGameInfoDeletions = context.PendingGameInfoDeletions
                .Where(x => uniqueIds.Contains(x.GameInfoUniqueId)).ToList();
            context.PendingGameInfoDeletions.RemoveRange(pendingGameInfoDeletions);
            return Task.FromResult(pendingGameInfoDeletions);
        }
    }
}