using GameManager.Modules.Synchronizer;
using GameManager.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace GameManager.Modules.TaskManager
{
    public static class TaskExecutor
    {
        public enum TaskStatus
        {
            RUNNING,
            SUCCESS,
            FAILED,
            NOT_FOUND,
            EXECUTE_PENDING
        }

        private static CancellationTokenSource _SyncTaskCts = new();

        /// <summary>
        /// Event triggered when a synchronization task fails.
        /// </summary>
        public static event EventHandler<TaskEventArgs>? OnSyncTaskFailed;

        /// <summary>
        /// Event triggered when a synchronization task starts.
        /// </summary>
        public static event EventHandler<TaskEventArgs>? OnSyncTaskStart;

        /// <summary>
        /// Event triggered when a synchronization task ends.
        /// </summary>
        public static event EventHandler<TaskEventArgs>? OnSyncTaskEnd;

        private static void OnSyncFailed(TaskEventArgs args)
        {
            OnSyncTaskFailed?.Invoke(null, args);
        }

        private static void OnSyncStart(TaskEventArgs args)
        {
            OnSyncTaskStart?.Invoke(null, args);
        }

        private static void OnSyncEnd(TaskEventArgs args)
        {
            OnSyncTaskEnd?.Invoke(null, args);
        }

        /// <summary>
        /// Generates a new <see cref="CancellationTokenSource" /> and disposes the old one.
        /// </summary>
        /// <returns>A new <see cref="CancellationTokenSource" />.</returns>
        private static CancellationTokenSource GenerateNewCts()
        {
            CancellationTokenSource oldCts = _SyncTaskCts;
            _SyncTaskCts = new CancellationTokenSource();
            oldCts.Dispose();
            return _SyncTaskCts;
        }

        public static void SyncTask()
        {
            const string taskName = "SyncTask";
            Task.Run(async () =>
            {
                OnSyncStart(new TaskEventArgs(taskName, TaskStatus.RUNNING));
                CancelSyncTask();
                ILoggerFactory loggerFactory = App.ServiceProvider.GetRequiredService<ILoggerFactory>();
                ILogger logger = loggerFactory.CreateLogger(nameof(TaskExecutor));
                logger.LogInformation("Start sync task");
                _SyncTaskCts = GenerateNewCts();
                IServiceProvider serviceProvider = App.ServiceProvider;
                ISynchronizer synchronizer = serviceProvider.GetRequiredService<ISynchronizer>();
                bool hasError = false;
                string errorMessage = string.Empty;
                try
                {
                    logger.LogInformation("Start sync app settings");
                    await synchronizer.SyncAppSetting(_SyncTaskCts.Token).ConfigureAwait(false);
                    await synchronizer.SyncGameInfos(_SyncTaskCts.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException ex)
                {
                    logger.LogInformation(ex, "Sync app settings task canceled");
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.LogError(ex, "Unauthorized access to sync app setting");
                    errorMessage = "Unauthorized access to remote";
                    hasError = true;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to sync app setting");
                    errorMessage = e.Message;
                    hasError = true;
                }

                if (hasError)
                    OnSyncFailed(new TaskEventArgs(taskName, TaskStatus.FAILED, errorMessage));
                else
                    OnSyncEnd(new TaskEventArgs(taskName, TaskStatus.SUCCESS));
                logger.LogInformation("End sync task");
            }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static void CancelSyncTask()
        {
            _SyncTaskCts.Cancel();
        }

        public static void UpdateGamePlayTimeTask(int gameId, string gameName, TimeSpan playTime)
        {
            IConfigService configService = App.ServiceProvider.GetRequiredService<IConfigService>();
            try
            {
                configService.UpdatePlayTimeAsync(gameId, playTime);
            }
            catch (Exception ex)
            {
                ILogger logger = App.ServiceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, "Update play time error with Id : {GameId} , GameName : {GameName}",
                    gameId, gameName);
            }
        }

        public class TaskEventArgs(string taskName, TaskStatus taskStatus, string errorMessage = "")
            : EventArgs
        {
            [UsedImplicitly]
            public string TaskName { get; set; } = taskName;

            [UsedImplicitly]
            public TaskStatus TaskStatus { get; set; } = taskStatus;

            [UsedImplicitly]
            public string ErrorMessage { get; set; } = errorMessage;
        }
    }
}