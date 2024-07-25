using GameManager.DB;

namespace GameManager.Database
{
    public class UnitOfWork(AppDbContext context) : IUnitOfWork
    {
        public IGameInfoRepository GameInfoRepository { get; } = new GameInfoRepository(context);

        public ILibraryRepository LibraryRepository { get; } = new LibraryRepository(context);

        public IAppSettingRepository AppSettingRepository { get; } = new AppSettingRepository(context);

        public ITagRepository TagRepository { get; } = new TagRepository(context);

        public Task<int> SaveChangesAsync()
        {
            return context.SaveChangesAsync();
        }

        public Task ClearChangeTrackerAsync()
        {
            context.ChangeTracker.Clear();
            return Task.CompletedTask;
        }
    }
}