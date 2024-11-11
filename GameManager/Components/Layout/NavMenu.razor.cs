using GameManager.Models.TaskManager;
using Microsoft.AspNetCore.Components;

namespace GameManager.Components.Layout
{
    public partial class NavMenu
    {
        private string _displaySyncMessage = "Running";
        private TaskExecutor.TaskStatus _lastSyncStatus = TaskExecutor.TaskStatus.SUCCESS;
        private DateTime _lastSyncTime = DateTime.Now;

        [Inject]
        private ITaskManager TaskManager { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            TaskExecutor.TaskStatus status = await TaskManager.GetTaskStatus(App.SyncTaskJobName);
            if (status == TaskExecutor.TaskStatus.RUNNING)
            {
                _lastSyncStatus = TaskExecutor.TaskStatus.RUNNING;
            }

            TaskExecutor.OnSyncTaskStart += HandleSyncStart;
            TaskExecutor.OnSyncTaskEnd += HandleSyncEnd;
            TaskExecutor.OnSyncTaskFailed += HandleSyncFailed;
            _ = base.OnInitializedAsync();
        }

        private void HandleSyncFailed(object? sender, TaskExecutor.TaskEventArgs e)
        {
            _lastSyncStatus = TaskExecutor.TaskStatus.FAILED;
            _lastSyncTime = DateTime.Now;
            _displaySyncMessage = "Sync Failed at " + _lastSyncTime + "\n" + e.ErrorMessage;
            _ = InvokeAsync(StateHasChanged);
        }

        private void HandleSyncEnd(object? sender, TaskExecutor.TaskEventArgs e)
        {
            _lastSyncStatus = TaskExecutor.TaskStatus.SUCCESS;
            _lastSyncTime = DateTime.Now;
            _displaySyncMessage = "Last Sync at " + _lastSyncTime;
            _ = InvokeAsync(StateHasChanged);
        }

        private void HandleSyncStart(object? sender, TaskExecutor.TaskEventArgs e)
        {
            _lastSyncStatus = TaskExecutor.TaskStatus.RUNNING;
            _displaySyncMessage = "Syncing...";
            _ = InvokeAsync(StateHasChanged);
        }
    }
}