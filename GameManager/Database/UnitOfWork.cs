using GameManager.DB;
using Microsoft.EntityFrameworkCore;

namespace GameManager.Database
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(IDbContextFactory<AppDbContext> context)
        {
            Context = context.CreateDbContext();
            GameInfoRepository = new GameInfoRepository(Context);
            StaffRoleRepository = new StaffRoleRepository(Context);
            StaffRepository = new StaffRepository(Context);
            LibraryRepository = new LibraryRepository(Context);
            AppSettingRepository = new AppSettingRepository(Context);
            TagRepository = new TagRepository(Context);
            GameInfoTagRepository = new GameInfoTagRepository(Context);
            PendingGameInfoDeletionRepository = new PendingGameInfoDeletionRepository(Context);
        }

        public AppDbContext Context { get; }

        public IGameInfoRepository GameInfoRepository { get; }

        public IStaffRoleRepository StaffRoleRepository { get; }

        public IStaffRepository StaffRepository { get; }

        public ILibraryRepository LibraryRepository { get; }

        public IAppSettingRepository AppSettingRepository { get; }

        public ITagRepository TagRepository { get; }

        public IGameInfoTagRepository GameInfoTagRepository { get; }

        public IPendingGameInfoDeletionRepository PendingGameInfoDeletionRepository { get; }

        public Task<int> SaveChangesAsync()
        {
            return Context.SaveChangesAsync();
        }

        public void BeginTransaction()
        {
            Context.Database.BeginTransaction();
        }

        public void RollbackTransaction()
        {
            Context.Database.RollbackTransaction();
        }

        public void DetachEntity<TEntity>(TEntity entity) where TEntity : class
        {
            Context.Entry(entity).State = EntityState.Detached;
        }

        public void CommitTransaction()
        {
            Context.Database.CommitTransaction();
        }
    }
}