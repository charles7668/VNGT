using GameManager.Models.Synchronizer;
using Microsoft.Extensions.Logging;

namespace GameManager.Models.TaskManager
{
    public static class TaskExecutor
    {
        private static CancellationTokenSource _SyncTaskCts = new();

        public static event EventHandler? OnSyncTaskFailed;

        public static event EventHandler? OnSyncTaskStart;

        public static event EventHandler? OnSyncTaskEnd;

        private static void OnSyncFailed(object? sender, EventArgs args)
        {
            OnSyncTaskFailed?.Invoke(sender, args);
        }

        private static void OnSyncStart(object? sender, EventArgs args)
        {
            OnSyncTaskStart?.Invoke(sender, args);
        }

        private static void OnSyncEnd(object? sender, EventArgs args)
        {
            OnSyncTaskEnd?.Invoke(sender, args);
        }

        public static void SyncTask()
        {
            Task.Run(async () =>
            {
                OnSyncStart(null, EventArgs.Empty);
                await _SyncTaskCts.CancelAsync();
                ILoggerFactory loggerFactory = App.ServiceProvider.GetRequiredService<ILoggerFactory>();
                ILogger logger = loggerFactory.CreateLogger(nameof(TaskExecutor));
                logger.LogInformation("Start sync task");
                _SyncTaskCts = new CancellationTokenSource();
                IServiceProvider serviceProvider = App.ServiceProvider;
                ISynchronizer synchronizer = serviceProvider.GetRequiredService<ISynchronizer>();
                bool hasError = false;
                try
                {
                    logger.LogInformation("Start sync app settings");
                    await synchronizer.SyncAppSetting(_SyncTaskCts.Token);
                }
                catch (TaskCanceledException)
                {
                    logger.LogInformation("Sync app settings task canceled");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to sync app setting");
                    hasError = true;
                }

                try
                {
                    logger.LogInformation("Start sync game infos");
                    await synchronizer.SyncGameInfos(_SyncTaskCts.Token);
                }
                catch (TaskCanceledException)
                {
                    logger.LogInformation("Sync game infos task canceled");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to sync game infos");
                    hasError = true;
                }

                if (hasError)
                    OnSyncFailed(null, EventArgs.Empty);
                else
                    OnSyncEnd(null, EventArgs.Empty);
                logger.LogInformation("End sync task");
            }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static void CancelSyncTask()
        {
            _SyncTaskCts.Cancel();
        }
    }
}