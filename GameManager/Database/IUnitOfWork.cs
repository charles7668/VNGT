namespace GameManager.Database
{
    public interface IUnitOfWork
    {
        IGameInfoRepository GameInfoRepository { get; }
    }
}