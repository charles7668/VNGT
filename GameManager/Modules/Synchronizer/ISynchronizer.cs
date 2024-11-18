namespace GameManager.Modules.Synchronizer
{
    public interface ISynchronizer
    {
        /// <summary>
        /// Synchronizes the application settings.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        public Task SyncAppSetting(CancellationToken cancellationToken);

        /// <summary>
        /// Synchronizes the game information.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        public Task SyncGameInfos(CancellationToken cancellationToken);
    }
}