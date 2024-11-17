using GameManager.DB.Models;
using GameManager.Modules.TaskManager;
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
            return ValueTask.CompletedTask;
        }

        protected override void OnInitialized()
        {
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
            Snackbar.Configuration.PreventDuplicates = false;
            Snackbar.Configuration.VisibleStateDuration = 5000;
        }
    }
}