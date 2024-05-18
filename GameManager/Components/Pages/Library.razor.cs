using GameManager.Components.Pages.components;
using GameManager.Database;
using GameManager.DB.Models;
using GameManager.GameInfoProvider;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using DBLibraryModel = GameManager.DB.Models.Library;

namespace GameManager.Components.Pages
{
    public partial class Library : IDisposable
    {
        private readonly HashSet<string> _patHashSet = new();

        private readonly CancellationTokenSource _loadLibraryCancellationTokenSource = new();

        [Inject]
        private IUnitOfWork UnitOfWork { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        private List<DBLibraryModel> Libraries { get; set; } = [];

        private int SelectionIndex { get; set; }

        private static Task? ScanTask { get; set; }

        [Inject]
        private IGameInfoProvider InfoGameInfoProvider { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        private ILogger<Library> Logger { get; set; } = null!;

        public void Dispose()
        {
            _loadLibraryCancellationTokenSource.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
            Libraries = await UnitOfWork.LibraryRepository.GetLibrariesAsync(_loadLibraryCancellationTokenSource.Token);

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
            if (Libraries.Count <= 0)
                return;
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

            if (Libraries[SelectionIndex].FolderPath != null)
                _patHashSet.Remove(Libraries[SelectionIndex].FolderPath!);
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
                Logger.LogInformation("Scan Start");
                const int searchLevel = 8;
                Queue<string> queue = new();
                foreach (DBLibraryModel library in Libraries)
                {
                    queue.Enqueue(library.FolderPath ??= "");
                }

                int curLevel = 1;
                while (queue.Count > 0)
                {
                    if (curLevel > searchLevel)
                        break;
                    int count = queue.Count;
                    for (int i = 0; i < count; i++)
                    {
                        string folder = queue.Dequeue();
                        Logger.LogInformation("Scanning {Folder}", folder);
                        if (string.IsNullOrEmpty(folder))
                            continue;
                        if (CheckExeFileExist(folder))
                        {
                            Logger.LogInformation("{Folder} contain exe file", folder);
                            var info = new GameInfo
                            {
                                GameName = Path.GetFileName(folder),
                                ExePath = folder
                            };
                            if (await UnitOfWork.GameInfoRepository.CheckExePathExist(folder))
                            {
                                Logger.LogInformation("{Folder} already exist", folder);
                                continue;
                            }

                            try
                            {
                                try
                                {
                                    Logger.LogInformation("Start fetch information");
                                    (List<GameInfo>? infoList, bool hasMore) searchList =
                                        await InfoGameInfoProvider.FetchGameSearchListAsync(info.GameName, 1, 1);
                                    if (searchList.infoList?.Count > 0)
                                    {
                                        string? id = searchList.infoList[0].GameInfoId;
                                        Logger.LogInformation("Scan result id : {Id}", id);
                                        if (id == null)
                                            continue;
                                        GameInfo? tempInfo = await InfoGameInfoProvider.FetchGameDetailByIdAsync(id);
                                        Logger.LogInformation("Scan result {Info}", tempInfo);
                                        if (tempInfo == null)
                                            continue;
                                        tempInfo.ExePath = folder;
                                        info = tempInfo;
                                    }
                                    else
                                    {
                                        Logger.LogInformation("No result found");
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.LogError("Scan failed : {Err}", e.Message);
                                }

                                await UnitOfWork.GameInfoRepository.AddAsync(info);
                            }
                            catch (Exception e)
                            {
                                Logger.LogError("{Err}", e.Message);
                            }
                        }
                        else
                        {
                            Logger.LogInformation("Execution file not found");
                            string[] folderInfos = Directory.GetDirectories(folder);
                            foreach (string folderInfo in folderInfos)
                            {
                                queue.Enqueue(folderInfo);
                            }
                        }
                    }

                    curLevel++;
                }

                Logger.LogInformation("Scan Complete");
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