using GameManager.Models.Synchronizer;
using Microsoft.Extensions.Logging;

namespace GameManager.Models.TaskManager
{
    public static class TaskExecutor
    {
        private static CancellationTokenSource _SyncTaskCts = new();

        public static event EventHandler? OnSyncTaskFailed;

        private static void OnSyncFailed(object? sender, EventArgs args)
        {
            OnSyncTaskFailed?.Invoke(sender, args);
        }

        public static void SyncTask()
        {
            Task.Run(async () =>
            {
                await _SyncTaskCts.CancelAsync();
                ILoggerFactory loggerFactory = App.ServiceProvider.GetRequiredService<ILoggerFactory>();
                ILogger logger = loggerFactory.CreateLogger(nameof(TaskExecutor));
                logger.LogInformation("Start sync task");
                _SyncTaskCts = new CancellationTokenSource();
                IServiceProvider serviceProvider = App.ServiceProvider;
                ISynchronizer synchronizer = serviceProvider.GetRequiredService<ISynchronizer>();
                try
                {
                    await synchronizer.SyncAppSetting(_SyncTaskCts.Token);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to sync");
                    OnSyncFailed(null, EventArgs.Empty);
                }
                finally
                {
                    logger.LogInformation("End sync task");
                }
            }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static void CancelSyncTask()
        {
            _SyncTaskCts.Cancel();
        }
    }
}