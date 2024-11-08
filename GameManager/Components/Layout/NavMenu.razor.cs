using GameManager.Models.TaskManager;

namespace GameManager.Components.Layout
{
    public partial class NavMenu
    {
        private string _lastSyncStatus = "Success";

        protected override void OnInitialized()
        {
            TaskExecutor.OnSyncTaskStart += HandleSyncStart;
            TaskExecutor.OnSyncTaskEnd += HandleSyncEnd;
            TaskExecutor.OnSyncTaskFailed += HandleSyncFailed;
            base.OnInitialized();
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