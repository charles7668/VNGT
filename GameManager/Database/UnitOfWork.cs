namespace GameManager.Database
{
    public class UnitOfWork(
        IGameInfoRepository gameInfoRepository,
        ILibraryRepository libraryRepository,
        IAppSettingRepository appSettingRepository) : IUnitOfWork
    {
        public IGameInfoRepository GameInfoRepository { get; } = gameInfoRepository;

        public ILibraryRepository LibraryRepository { get; } = libraryRepository;

        public IAppSettingRepository AppSettingRepository { get; } = appSettingRepository;
    }
}