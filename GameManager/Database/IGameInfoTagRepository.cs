namespace GameManager.Database
{
    public interface IGameInfoTagRepository
    {
        Task<bool> CheckGameHasTag(int tagId, int gameId);

        Task RemoveGameInfoTagAsync(int tagId, int gameId);
    }
}