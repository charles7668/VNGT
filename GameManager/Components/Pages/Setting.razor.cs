using GameManager.Database;
using GameManager.DB.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Win32;
using MudBlazor;

namespace GameManager.Components.Pages
{
    public partial class Setting
    {
        private AppSetting AppSetting { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private IUnitOfWork UnitOfWork { get; set; } = null!;

        protected override void OnInitialized()
        {
            AppSetting = UnitOfWork.AppSettingRepository.GetAppSettingAsync().Result;
            base.OnInitialized();
        }

        private async Task UpdateSetting()
        {
            try
            {
                await UnitOfWork.AppSettingRepository.UpdateAppSettingAsync(AppSetting);
            }
            catch (Exception e)
            {
                await DialogService.ShowMessageBox("Error", e.Message);
                return;
            }

            await DialogService.ShowMessageBox("Complete", "Update Succeeded");
        }

        private Task ScanLocaleEmulator()
        {
            RegistryKey key = Registry.ClassesRoot;
            const string subKeyPath = @"CLSID\{C52B9871-E5E9-41FD-B84D-C5ACADBEC7AE}\InprocServer32";
            RegistryKey? subKey = key.OpenSubKey(subKeyPath);

            if (subKey?.GetValue("CodeBase") is not string codeBase)
            {
                return Task.CompletedTask;
            }

            var uri = new Uri(codeBase);
            string? lePath = Path.GetDirectoryName(uri.LocalPath);
            if (lePath == null)
            {
                return Task.CompletedTask;
            }

            AppSetting.LocaleEmulatorPath = lePath;
            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}