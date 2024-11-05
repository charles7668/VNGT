using GameManager.DB;
using Microsoft.EntityFrameworkCore;

namespace GameManager.Database
{
    public class UnitOfWork(AppDbContext context) : IUnitOfWork
    {
        public IGameInfoRepository GameInfoRepository { get; } = new GameInfoRepository(context);

        public IStaffRoleRepository StaffRoleRepository { get; } = new StaffRoleRepository(context);

        public IStaffRepository StaffRepository { get; } = new StaffRepository(context);

        public ILibraryRepository LibraryRepository { get; } = new LibraryRepository(context);

        public IAppSettingRepository AppSettingRepository { get; } = new AppSettingRepository(context);

        public ITagRepository TagRepository { get; } = new TagRepository(context);

        public IGameInfoTagRepository GameInfoTagRepository { get; } = new GameInfoTagRepository(context);

        public ILaunchOptionRepository LaunchOptionRepository { get; } = new LaunchOptionRepository(context);

        public Task<int> SaveChangesAsync()
        {
            return context.SaveChangesAsync();
        }

        public void BeginTransaction()
        {
            context.Database.BeginTransaction();
        }

        public void RollbackTransaction()
        {
            context.Database.RollbackTransaction();
        }

        public void DetachEntity<TEntity>(TEntity entity) where TEntity : class
        {
            context.Entry(entity).State = EntityState.Detached;
        }

        public void CommitTransaction()
        {
            context.Database.CommitTransaction();
        }
    }
}