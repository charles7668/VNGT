using GameManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using ZipFile = System.IO.Compression.ZipFile;

namespace GameManager.Components.Pages
{
    public partial class Help
    {
        [Inject]
        private IAppPathService AppPathService { get; set; } = null!;

        [Inject]
        private ILogger<Help> Logger { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        public async Task OnExportLogClick()
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads,
                SuggestedFileName = $"Log {DateTime.UtcNow:MM-dd-yyyy HH-mm-ss}"
            };
            savePicker.FileTypeChoices.Add("Zip", [".zip"]);
            Window? currWin = Application.Current?.Windows.FirstOrDefault();
            IntPtr? hwnd = (currWin?.Handler?.PlatformView as MauiWinUIWindow)?.WindowHandle;
            if (hwnd is null) { return; }

            InitializeWithWindow.Initialize(savePicker, hwnd.Value);
            string tempDir = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            try
            {
                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file == null)
                {
                    return;
                }

                string logDir = AppPathService.LogDirPath;
                // copy file to temp directory , avoid file is being used
                Directory.CreateDirectory(tempDir);
                string[] files = Directory.GetFiles(logDir);
                foreach (string logFile in files)
                {
                    File.Copy(logFile, Path.Combine(tempDir, Path.GetFileName(logFile)), true);
                }

                // delete before zip , because save picker will create the file
                File.Delete(file.Path);
                ZipFile.CreateFromDirectory(tempDir, file.Path);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to export log");
                Snackbar.Add("Failed to export log", Severity.Error);
            }
            finally
            {
                try
                {
                    Directory.Delete(tempDir);
                }
                catch
                {
                    // ignore
                }
            }
        }

        private async Task OnBackupSettingsClick()
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads,
                SuggestedFileName = $"{DateTime.UtcNow:MM-dd-yyyy HH-mm-ss}"
            };
            savePicker.FileTypeChoices.Add("json", [".json"]);
            Window? currWin = Application.Current?.Windows.FirstOrDefault();
            IntPtr? hwnd = (currWin?.Handler?.PlatformView as MauiWinUIWindow)?.WindowHandle;
            if (hwnd is null)
                return;

            InitializeWithWindow.Initialize(savePicker, hwnd.Value);
            try
            {
                StorageFile? file = await savePicker.PickSaveFileAsync();
                if (file == null)
                    return;

                IConfigService configService = App.ServiceProvider.GetRequiredService<IConfigService>();
                await configService.BackupSettings(file.Path);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to backup settings");
                Snackbar.Add("Failed to backup settings", Severity.Error);
            }
        }

        private async Task OnRestoreSettingsClick()
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, [".json"] }
            });

            var options = new PickOptions
            {
                PickerTitle = "Please select json file",
                FileTypes = customFileType
            };

            try
            {
                FileResult? result = await FilePicker.PickAsync(options);
                if (result == null)
                {
                    return;
                }

                IConfigService configService = App.ServiceProvider.GetRequiredService<IConfigService>();
                await configService.RestoreSettings(result.FullPath);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to restore settings");
                Snackbar.Add("Failed to restore settings", Severity.Error);
                return;
            }

            Snackbar.Add("Restore settings success", Severity.Info);
        }
    }
}