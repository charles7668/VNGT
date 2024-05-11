namespace GameManager.Database
{
    public class UnitOfWork(IGameInfoRepository gameInfoRepository) : IUnitOfWork
    {
        public IGameInfoRepository GameInfoRepository { get; } = gameInfoRepository;
    }
}