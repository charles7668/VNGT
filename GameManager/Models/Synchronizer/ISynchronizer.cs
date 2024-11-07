namespace GameManager.Models.Synchronizer
{
    public interface ISynchronizer
    {
        public Task SyncAppSetting(CancellationToken cancellationToken);
    }
}