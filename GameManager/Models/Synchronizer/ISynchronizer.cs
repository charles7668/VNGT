namespace GameManager.Models.Synchronizer
{
    public interface ISynchronizer
    {
        public Task SyncAppSetting(CancellationToken cancellationToken);
        
        public Task SyncGameInfos(CancellationToken cancellationToken);
    }
}