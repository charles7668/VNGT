using GameManager.DB.Models;
using GameManager.Models.TaskManager;
using GameManager.Services;
using Hangfire.Annotations;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GameManager.Components.Layout
{
    public partial class MainLayout : IAsyncDisposable
    {
        [Inject]
        [UsedImplicitly]
        private ITaskManager TaskManager { get; set; } = null!;

        [Inject]
        [UsedImplicitly]
        private IConfigService ConfigService { get; set; } = null!;

        public ValueTask DisposeAsync()
        {
            TaskManager.CancelTask(App.SyncTaskJobName);
            TaskExecutor.OnSyncTaskFailed -= HandleSyncFailed;
            return ValueTask.CompletedTask;
        }

        private void HandleSyncFailed(object? sender, EventArgs args)
        {
            Snackbar.Add("Failed to sync", Severity.Error);
        }

        protected override void OnInitialized()
        {
            AppSetting appSetting = ConfigService.GetAppSetting();
            if (appSetting.EnableSync)
            {
                TaskManager.StartBackgroundIntervalTask(App.SyncTaskJobName, () => TaskExecutor.SyncTask(),
                    TaskExecutor.CancelSyncTask, appSetting.SyncInterval, true);
                TaskExecutor.OnSyncTaskFailed += HandleSyncFailed;
            }

            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
            Snackbar.Configuration.PreventDuplicates = false;
            Snackbar.Configuration.VisibleStateDuration = 5000;
        }
    }
}