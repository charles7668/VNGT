using Microsoft.AspNetCore.Components;

namespace GameManager.Components.Pages.components
{
    public partial class ProgressDialog
    {
        [Parameter]
        public string ProgressText { get; set; } = "Waiting...";

        [Parameter]
        public Task RunningTask { get; set; } = Task.CompletedTask;
    }
}