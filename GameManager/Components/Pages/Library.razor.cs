using GameManager.Components.Pages.components;
using GameManager.DTOs;
using GameManager.GameInfoProvider;
using GameManager.Properties;
using GameManager.Services;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace GameManager.Components.Pages
{
    public partial class Library : IDisposable
    {
        private readonly CancellationTokenSource _loadLibraryCancellationTokenSource = new();
        private readonly HashSet<string> _patHashSet = new();

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        private List<LibraryDTO> Libraries { get; set; } = [];

        private int SelectionIndex { get; set; }

        private static Task? ScanTask { get; set; }

        [Inject]
        private GameInfoProviderFactory GameInfoProviderFactory { get; set; } = null!;

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
            Libraries = await ConfigService.GetLibrariesAsync(_loadLibraryCancellationTokenSource.Token);

            await base.OnInitializedAsync();
            foreach (LibraryDTO library in Libraries)
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
                await ConfigService.AddLibraryAsync(new LibraryDTO
                {
                    FolderPath = folder.Path
                });
            }
            catch (Exception e)
            {
                await DialogService.ShowMessageBox("Error", e.Message, "cancel");
                return;
            }

            Libraries.Add(new LibraryDTO
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
            IDialogReference dialogReference = await DialogService.ShowAsync<DialogConfirm>("Warning", parameters,
                new DialogOptions
                {
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult is null or { Canceled: true })
                return;

            int id = Libraries[SelectionIndex].Id;
            try
            {
                await ConfigService.DeleteLibraryByIdAsync(id);
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

        private async Task<GameInfoDTO?> FetchGameInformation(string gameName)
        {
            try
            {
                Logger.LogInformation("Start fetch information with GameName : {GameName}", gameName);
                IGameInfoProvider provider = GameInfoProviderFactory.GetProvider("VNDB") ??
                                             throw new ArgumentException(
                                                 "Game info provider not found : VNDB");
                (List<GameInfoDTO>? infoList, bool hasMore) searchList =
                    await provider.FetchGameSearchListAsync(gameName, 1, 1);
                if (searchList.infoList?.Count > 0)
                {
                    string? id = searchList.infoList[0].GameInfoFetchId;
                    Logger.LogInformation("Scan result id : {Id}", id);
                    if (id != null)
                    {
                        GameInfoDTO? tempInfo =
                            await provider.FetchGameDetailByIdAsync(id);
                        Logger.LogInformation("Scan result {@Info}", tempInfo);
                        return tempInfo;
                    }
                }
                else
                {
                    Logger.LogInformation("No result found");
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Fetch Game Info failed : {Err}", e.Message);
            }

            return null;
        }

        private async Task<string[]> ScanFolder(string folder, bool autoFetchInfo)
        {
            try
            {
                Logger.LogInformation("Scanning {Folder}", folder);
                if (string.IsNullOrWhiteSpace(folder))
                    return [];
                if (CheckExeFileExist(folder))
                {
                    Logger.LogInformation("find exe file");
                    var info = new GameInfoDTO
                    {
                        GameName = Path.GetFileName(folder),
                        ExePath = folder
                    };
                    if (await ConfigService.CheckExePathExist(folder))
                    {
                        Logger.LogInformation("{Folder} already exist in database", folder);
                        return [];
                    }

                    if (autoFetchInfo)
                    {
                        GameInfoDTO? tempInfo = await FetchGameInformation(info.GameName);
                        if (tempInfo != null)
                        {
                            tempInfo.ExePath = folder;
                            info = tempInfo;
                        }
                    }

                    await ConfigService.AddGameInfoAsync(info);
                    return [];
                }

                Logger.LogInformation("Execution file not found");
                string[] folderInfos = [];
                try
                {
                    folderInfos = Directory.GetDirectories(folder);
                }
                catch (UnauthorizedAccessException e)
                {
                    Logger.LogError(e, "Unauthorized access to {Folder}", folder);
                }


                return folderInfos;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occur when scanning {Folder} , Err : {Err}", folder, e.Message);
            }

            return [];

            bool CheckExeFileExist(string folderPath)
            {
                try
                {
                    IEnumerable<string> enumerableFiles =
                        Directory.EnumerateFiles(folderPath, "*.exe", SearchOption.TopDirectoryOnly);

                    return enumerableFiles.Any();
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        [UsedImplicitly]
        private Task OnScanClick()
        {
            if (ScanTask is { IsCompleted: false })
            {
                return Task.CompletedTask;
            }

            ScanTask = Task.Run(async () =>
            {
                Snackbar.Add(Resources.Library_StartScan, Severity.Info);
                Logger.LogInformation("Library Scan Start");
                const int searchLevel = 8;
                Queue<string> queue = new();
                foreach (LibraryDTO library in Libraries)
                {
                    library.FolderPath ??= "";
                    queue.Enqueue(library.FolderPath);
                }

                int curLevel = 1;
                AppSettingDTO appSetting = ConfigService.GetAppSettingDTO();
                while (queue.Count > 0)
                {
                    if (curLevel > searchLevel)
                        break;
                    int count = queue.Count;
                    for (int i = 0; i < count; i++)
                    {
                        string folder = queue.Dequeue();
                        string[] appendScanList = await ScanFolder(folder, appSetting.IsAutoFetchInfoEnabled);
                        foreach (string appendFolder in appendScanList)
                        {
                            queue.Enqueue(appendFolder);
                        }
                    }

                    curLevel++;
                }

                Logger.LogInformation("Library Scan Complete");
                Snackbar.Add(Resources.Library_ScanComplete, Severity.Info);
            }).ContinueWith(_ => InvokeAsync(StateHasChanged));

            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}