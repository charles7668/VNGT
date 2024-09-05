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
                SuggestedFileName = $"Log {DateTime.Now:MM-dd-yyyy HH-mm-ss}"
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
                Directory.Delete(tempDir);
            }
        }
    }
}