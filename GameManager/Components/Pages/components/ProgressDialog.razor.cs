using Microsoft.AspNetCore.Components;
using WinRT.Interop;

namespace GameManager.Components.Pages.components
{
    public partial class ProgressDialog
    {
        private int _progressValue;

        [Parameter]
        public string ProgressText { get; set; } = "Waiting...";

        [Parameter]
        public Task RunningTask { get; set; } = Task.CompletedTask;

        [Parameter]
        public bool IsDeterminateProgress { get; set; }

        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                if (value == _progressValue) return;
                _progressValue = int.Clamp(value, 0, 100);
                ProgressText = $"{_progressValue} %";
                Application.Current?.Dispatcher.Dispatch(StateHasChanged);
            }
        }
    }
}