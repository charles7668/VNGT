using GameManager.Models.Synchronizer;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace GameManager.Models.TaskManager
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

        public static event EventHandler<TaskEventArgs>? OnSyncTaskFailed;

        public static event EventHandler<TaskEventArgs>? OnSyncTaskStart;

        public static event EventHandler<TaskEventArgs>? OnSyncTaskEnd;

        private static void OnSyncFailed(object? sender, TaskEventArgs args)
        {
            OnSyncTaskFailed?.Invoke(sender, args);
        }

        private static void OnSyncStart(object? sender, TaskEventArgs args)
        {
            OnSyncTaskStart?.Invoke(sender, args);
        }

        private static void OnSyncEnd(object? sender, TaskEventArgs args)
        {
            OnSyncTaskEnd?.Invoke(sender, args);
        }

        public static void SyncTask()
        {
            const string taskName = "SyncTask";
            Task.Run(async () =>
            {
                OnSyncStart(null, new TaskEventArgs(taskName, TaskStatus.RUNNING));
                await _SyncTaskCts.CancelAsync();
                ILoggerFactory loggerFactory = App.ServiceProvider.GetRequiredService<ILoggerFactory>();
                ILogger logger = loggerFactory.CreateLogger(nameof(TaskExecutor));
                logger.LogInformation("Start sync task");
                _SyncTaskCts = new CancellationTokenSource();
                IServiceProvider serviceProvider = App.ServiceProvider;
                ISynchronizer synchronizer = serviceProvider.GetRequiredService<ISynchronizer>();
                bool hasError = false;
                string errorMessage = string.Empty;
                try
                {
                    logger.LogInformation("Start sync app settings");
                    await synchronizer.SyncAppSetting(_SyncTaskCts.Token);
                    await synchronizer.SyncGameInfos(_SyncTaskCts.Token);
                }
                catch (TaskCanceledException)
                {
                    logger.LogInformation("Sync app settings task canceled");
                }
                catch (UnauthorizedAccessException)
                {
                    logger.LogError("Unauthorized access to sync app setting");
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
                    OnSyncFailed(null, new TaskEventArgs(taskName, TaskStatus.FAILED, errorMessage));
                else
                    OnSyncEnd(null, new TaskEventArgs(taskName, TaskStatus.SUCCESS));
                logger.LogInformation("End sync task");
            }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static void CancelSyncTask()
        {
            _SyncTaskCts.Cancel();
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