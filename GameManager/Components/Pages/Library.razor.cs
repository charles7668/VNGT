using GameManager.Database;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using DBLibraryModel = GameManager.DB.Models.Library;

namespace GameManager.Components.Pages
{
    public partial class Library
    {
        [Inject]
        private IUnitOfWork UnitOfWork { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        private List<DBLibraryModel> Libraries { get; set; } = [];

        private int SelectionIndex { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Libraries = await UnitOfWork.LibraryRepository.GetLibrariesAsync();
            await base.OnInitializedAsync();
        }

        private async Task OnLibraryAdd()
        {
            var picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            picker.FileTypeFilter.Add("*");
            // Get the current window's HWND by passing in the Window object
            IntPtr hwnd = ((MauiWinUIWindow?)Application.Current?.Windows[0].Handler?.PlatformView!).WindowHandle;

            // Associate the HWND with the file picker
            InitializeWithWindow.Initialize(picker, hwnd);

            StorageFolder? folder = await picker.PickSingleFolderAsync();
            if (folder == null)
            {
                return;
            }

            try
            {
                await UnitOfWork.LibraryRepository.AddAsync(new DBLibraryModel
                {
                    FolderPath = folder.Path
                });
            }
            catch (Exception e)
            {
                await DialogService.ShowMessageBox("Error", e.Message, "cancel");
            }

            Libraries.Add(new DBLibraryModel
            {
                FolderPath = folder.Path
            });
            StateHasChanged();
        }

        private Task OnDelete()
        {
            int id = Libraries[SelectionIndex].Id;
            try
            {
                UnitOfWork.LibraryRepository.DeleteByIdAsync(id);
            }
            catch (Exception e)
            {
                DialogService.ShowMessageBox("Error", e.Message, "cancel");
                return Task.CompletedTask;
            }

            Libraries.RemoveAt(SelectionIndex);
            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}