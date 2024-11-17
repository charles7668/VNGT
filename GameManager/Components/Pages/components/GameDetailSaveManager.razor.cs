using GameManager.DB.Models;
using GameManager.Models;
using GameManager.Modules.SaveDataManager;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GameManager.Components.Pages.components
{
    public partial class GameDetailSaveManager
    {
        private List<string> _backupList = [];

        private bool _isRunning = false;
        private bool IsLoading { get; set; } = true;

        [Parameter]
        [EditorRequired]
        public GameInfo GameInfo { get; set; } = null!;

        [Inject]
        private ISaveDataManager SaveDataManager { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (IsLoading)
            {
                _ = Task.Run(async () =>
                {
                    _backupList = await SaveDataManager.GetBackupListAsync(GameInfo);
                    IsLoading = false;
                    _ = InvokeAsync(StateHasChanged);
                });
            }

            return base.OnAfterRenderAsync(firstRender);
        }


        private void ShowSnackbarFromResult<T>(Result<T> result)
        {
            if (result.Success)
                return;
            Severity severity = Severity.Warning;
            if (result.Exception != null)
                severity = Severity.Error;
            Snackbar.Add(result.Message, severity);
        }

        private async Task OnRestoreClick(string backupFileName)
        {
            if (_isRunning)
                return;
            try
            {
                _isRunning = true;
                Result result = await SaveDataManager.RestoreSaveFileAsync(GameInfo, backupFileName);
                ShowSnackbarFromResult(result);
            }
            finally
            {
                _isRunning = false;
                _ = InvokeAsync(StateHasChanged);
            }
        }

        private async Task OnDeleteClick(string backupFileName)
        {
            if (_isRunning)
                return;
            try
            {
                _isRunning = true;
                Result result = await SaveDataManager.DeleteSaveFileAsync(GameInfo, backupFileName);
                ShowSnackbarFromResult(result);
                if (result.Success)
                    _backupList.Remove(backupFileName);
            }
            finally
            {
                _isRunning = false;
                _ = InvokeAsync(StateHasChanged);
            }
        }

        private async Task OnBackupClick()
        {
            if (_isRunning)
                return;
            _isRunning = true;
            try
            {
                Result<string> result = await SaveDataManager.BackupSaveFileAsync(GameInfo);
                if (!result.Success)
                {
                    ShowSnackbarFromResult(result);
                    return;
                }

                if (_backupList.Count >= SaveDataManager.MaxBackupCount)
                {
                    _backupList.RemoveAt(_backupList.Count - 1);
                }

                _backupList.Insert(0, result.Value!);
            }
            finally
            {
                _isRunning = false;
                _ = InvokeAsync(StateHasChanged);
            }
        }
    }
}