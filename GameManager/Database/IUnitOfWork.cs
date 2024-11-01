namespace GameManager.Database
{
    public interface IUnitOfWork
    {
        IGameInfoRepository GameInfoRepository { get; }

        IStaffRoleRepository StaffRoleRepository { get; }

        IStaffRepository StaffRepository { get; }

        ILibraryRepository LibraryRepository { get; }

        IAppSettingRepository AppSettingRepository { get; }

        ITagRepository TagRepository { get; }

        IGameInfoTagRepository GameInfoTagRepository { get; }

        ILaunchOptionRepository LaunchOptionRepository { get; }

        Task<int> SaveChangesAsync();

        void DetachEntity<TEntity>(TEntity entity) where TEntity : class;
    }
}