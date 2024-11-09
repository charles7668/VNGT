using GameManager.Models.TaskManager;
using Microsoft.AspNetCore.Components;

namespace GameManager.Components.Layout
{
    public partial class NavMenu
    {
        private string _lastSyncStatus = "Success";

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
            _ = InvokeAsync(StateHasChanged);
        }

        private void HandleSyncEnd(object? sender, EventArgs e)
        {
            _lastSyncStatus = "Success";
            _ = InvokeAsync(StateHasChanged);
        }

        private void HandleSyncStart(object? sender, EventArgs e)
        {
            _lastSyncStatus = "Running";
            _ = InvokeAsync(StateHasChanged);
        }
    }
}