using GameManager.Models.TaskManager;
using Microsoft.AspNetCore.Components;

namespace GameManager.Components.Layout
{
    public partial class NavMenu
    {
        private string _displaySyncMessage = "Running";
        private string _lastSyncStatus = "Success";

        private DateTime _lastSyncTime = DateTime.Now;

        [Inject]
        private ITaskManager TaskManager { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            string status = await TaskManager.GetTaskStatus(App.SyncTaskJobName);
            if (status == "Running")
            {
                _lastSyncStatus = "Running";
            }

            TaskExecutor.OnSyncTaskStart += HandleSyncStart;
            TaskExecutor.OnSyncTaskEnd += HandleSyncEnd;
            TaskExecutor.OnSyncTaskFailed += HandleSyncFailed;
            _ = base.OnInitializedAsync();
        }

        private void HandleSyncFailed(object? sender, EventArgs e)
        {
            _lastSyncStatus = "Failed";
            _lastSyncTime = DateTime.Now;
            _displaySyncMessage = "Sync Failed at " + _lastSyncTime;
            _ = InvokeAsync(StateHasChanged);
        }

        private void HandleSyncEnd(object? sender, EventArgs e)
        {
            _lastSyncStatus = "Success";
            _lastSyncTime = DateTime.Now;
            _displaySyncMessage = "Last Sync at " + _lastSyncTime;
            _ = InvokeAsync(StateHasChanged);
        }

        private void HandleSyncStart(object? sender, EventArgs e)
        {
            _lastSyncStatus = "Running";
            _displaySyncMessage = "Syncing...";
            _ = InvokeAsync(StateHasChanged);
        }
    }
}