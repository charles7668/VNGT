using GameManager.DB;

namespace GameManager.Database
{
    public interface IUnitOfWork
    {
        AppDbContext Context { get; }
        
        IGameInfoRepository GameInfoRepository { get; }

        IStaffRoleRepository StaffRoleRepository { get; }

        IStaffRepository StaffRepository { get; }

        ILibraryRepository LibraryRepository { get; }

        IAppSettingRepository AppSettingRepository { get; }

        ITagRepository TagRepository { get; }

        IGameInfoTagRepository GameInfoTagRepository { get; }

        ILaunchOptionRepository LaunchOptionRepository { get; }

        Task<int> SaveChangesAsync();

        void BeginTransaction();

        void CommitTransaction();

        void RollbackTransaction();

        void DetachEntity<TEntity>(TEntity entity) where TEntity : class;
    }
}