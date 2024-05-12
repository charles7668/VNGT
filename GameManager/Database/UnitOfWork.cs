namespace GameManager.Database
{
    public class UnitOfWork(IGameInfoRepository gameInfoRepository, ILibraryRepository libraryRepository) : IUnitOfWork
    {
        public IGameInfoRepository GameInfoRepository { get; } = gameInfoRepository;

        public ILibraryRepository LibraryRepository { get; } = libraryRepository;
    }
}