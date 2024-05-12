using GameManager.Components.Pages.components;
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

        private async Task OnDelete()
        {
            DialogParameters<DialogConfirm> parameters = new()
            {
                { x => x.Content, "Are you sure you want to delete?" }
            };
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogConfirm>("Warning", parameters,
                new DialogOptions
                {
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult.Canceled)
                return;

            int id = Libraries[SelectionIndex].Id;
            try
            {
                await UnitOfWork.LibraryRepository.DeleteByIdAsync(id);
            }
            catch (Exception e)
            {
                await DialogService.ShowMessageBox("Error", e.Message, "cancel");
                return;
            }

            Libraries.RemoveAt(SelectionIndex);
            StateHasChanged();
        }
    }
}