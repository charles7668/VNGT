using GameManager.Models;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace GameManager.Services
{
    public class PickFolderService : IPickFolderService
    {
        public async Task<Result> PickFolderAsync(Action<string>? onSuccessCallback)
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            IElementHandler? elementHandler = Application.Current?.Windows[0].Handler;
            IntPtr? handle = ((MauiWinUIWindow?)elementHandler?.PlatformView)?.WindowHandle;
            if (handle != null)
            {
                InitializeWithWindow.Initialize(folderPicker, (IntPtr)handle);
                StorageFolder? result = await folderPicker.PickSingleFolderAsync();
                if (result == null)
                    return Result.Failure("No folder selected");
                onSuccessCallback?.Invoke(result.Path);
            }

            return Result.Ok();
        }
    }
}