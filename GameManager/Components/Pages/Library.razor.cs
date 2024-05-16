using GameManager.Components.Pages.components;
using GameManager.Database;
using GameManager.DB.Models;
using GameManager.GameInfoProvider;
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
        private readonly HashSet<string> _patHashSet = new();

        [Inject]
        private IUnitOfWork UnitOfWork { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        private List<DBLibraryModel> Libraries { get; set; } = [];

        private int SelectionIndex { get; set; }

        private static Task? ScanTask { get; set; }

        [Inject]
        private IProvider InfoProvider { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            Libraries = await UnitOfWork.LibraryRepository.GetLibrariesAsync();
            await base.OnInitializedAsync();
            foreach (DBLibraryModel library in Libraries)
            {
                if (library.FolderPath == null)
                    continue;
                _patHashSet.Add(library.FolderPath);
            }
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

            if (_patHashSet.Contains(folder.Path))
            {
                await DialogService.ShowMessageBox("Error", "This folder has been added", "cancel");
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
                return;
            }

            Libraries.Add(new DBLibraryModel
            {
                FolderPath = folder.Path
            });
            _patHashSet.Add(folder.Path);
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

            _patHashSet.Remove(Libraries[SelectionIndex].FolderPath);
            Libraries.RemoveAt(SelectionIndex);
            StateHasChanged();
        }

        private Task OnScan()
        {
            if (ScanTask is { IsCompleted: false })
            {
                return Task.CompletedTask;
            }

            ScanTask = Task.Run(async () =>
            {
                Snackbar.Add("Scan Start", Severity.Info);
                const int searchLevel = 8;
                Queue<string> queue = new();
                foreach (DBLibraryModel library in Libraries)
                {
                    queue.Enqueue(library.FolderPath ??= "");
                }

                await File.WriteAllTextAsync("test.txt", "");
                int curLevel = 1;
                while (queue.Count > 0)
                {
                    if (curLevel > searchLevel)
                        break;
                    int count = queue.Count;
                    for (int i = 0; i < count; i++)
                    {
                        string folder = queue.Dequeue();
                        if (string.IsNullOrEmpty(folder))
                            continue;
                        if (CheckExeFileExist(folder))
                        {
                            var info = new GameInfo
                            {
                                GameName = Path.GetFileName(folder),
                                ExePath = folder
                            };
                            if (await UnitOfWork.GameInfoRepository.CheckExePathExist(folder))
                                continue;

                            try
                            {
                                try
                                {
                                    (List<GameInfo>? infoList, bool hasMore) searchList =
                                        await InfoProvider.FetchGameSearchListAsync(info.GameName, 1, 1);
                                    if (searchList.infoList?.Count > 0)
                                    {
                                        string? id = searchList.infoList[0].GameInfoId;
                                        if (id == null)
                                            continue;
                                        GameInfo? tempInfo = await InfoProvider.FetchGameDetailByIdAsync(id);
                                        if (tempInfo == null)
                                            continue;
                                        tempInfo.ExePath = folder;
                                        info = tempInfo;
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignore
                                }

                                await UnitOfWork.GameInfoRepository.AddAsync(info);
                            }
                            catch (Exception)
                            {
                                // ignore
                            }
                        }
                        else
                        {
                            string[] folderInfos = Directory.GetDirectories(folder);
                            foreach (string folderInfo in folderInfos)
                            {
                                queue.Enqueue(folderInfo);
                            }
                        }
                    }

                    curLevel++;
                }

                Snackbar.Add("Scan Complete", Severity.Info);
                _ = InvokeAsync(StateHasChanged);
            });

            StateHasChanged();

            return Task.CompletedTask;

            bool CheckExeFileExist(string folderPath)
            {
                try
                {
                    string[] exeFiles = Directory.GetFiles(folderPath, "*.exe", SearchOption.TopDirectoryOnly);

                    return exeFiles.Length > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}