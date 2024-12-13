using GameManager.Components.Pages.components;
using GameManager.DB.Models;
using GameManager.DTOs;
using GameManager.Enums;
using GameManager.Modules.TaskManager;
using GameManager.Properties;
using GameManager.Services;
using Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Utilities;
using System.Diagnostics;
using System.Web;

namespace GameManager.Components.Pages
{
    public partial class Home : IDisposable
    {
        private static bool _IsVersionChecked;

        private SortOrder _currentOrder = SortOrder.UPLOAD_TIME;
        private CancellationTokenSource _deleteTaskCancellationTokenSource = new();
        private CancellationTokenSource _loadingCancellationTokenSource = new();
        private DotNetObjectReference<Home> _objRef = null!;
        private int _showDetailId = -1;

        private bool IsShowDetail { get; set; }

        [Inject]
        public ISnackbar SnakeBar { get; set; } = null!;

        private List<ViewInfo> ViewGameInfos { get; } = [];

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        [Inject]
        private ITaskManager TaskManager { get; set; } = null!;

        [Inject]
        private ILogger<Home> Logger { get; set; } = null!;

        private bool IsSelectionMode { get; set; }

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = null!;

        [Parameter]
        public string InitFilter { get; set; } = "";

        private int CardListWidth { get; set; }

        private int CardGapPixel { get; set; }

        private int CardItemWidth { get; } = 230;

        private bool IsDeleting { get; set; }

        private bool IsLoading { get; set; } = true;

        private Virtualize<IEnumerable<ViewInfo>>? VirtualizeComponent { get; set; }

        private string CardListCss => CssBuilder
            .Default(IsDeleting ? "deleting justify-center align-center d-flex" : "" + " flex-grow-1")
            .Build();

        private string ListContainerClass => CssBuilder
            .Default("d-flex flex-column main-container")
            .AddClass("inactive", IsShowDetail)
            .Build();

        public void Dispose()
        {
            JsRuntime.InvokeVoidAsync("resizeHandlers.removeResizeListener");
            _deleteTaskCancellationTokenSource.Cancel();
            _loadingCancellationTokenSource.Cancel();
        }

        private async Task DetectNewerVersion()
        {
            if (_IsVersionChecked)
                return;
            IVersionService versionService = App.ServiceProvider.GetRequiredService<IVersionService>();
            string? newestVersion = await versionService.DetectNewestVersion();
            if (string.IsNullOrWhiteSpace(newestVersion))
                return;
            ShowUpdateNotify(newestVersion);

            _IsVersionChecked = true;

            return;

            void ShowUpdateNotify(string version)
            {
                SnakeBar.Add($"New version {version} available", Severity.Info);
            }
        }

        private async Task AddNewGame(string exePath)
        {
            if (IsDeleting)
                return;
            Logger.LogInformation("Add new game button clicked");
            string? saveFilePath = null;
            try
            {
                saveFilePath = await ScanSaveFilePath();
            }
            catch (UnauthorizedAccessException)
            {
                // ignore 
            }
            
            var inputModel = new DialogGameInfoEdit.FormModel
            {
                GameName = Path.GetFileName(Path.GetDirectoryName(exePath)) ?? "null",
                ExePath = Path.GetDirectoryName(exePath),
                SaveFilePath = saveFilePath
            };
            try
            {
                var fileInfo = new FileInfo(exePath);
                if (!fileInfo.Attributes.HasFlag(FileAttributes.Directory) && fileInfo.Exists)
                {
                    inputModel.ExeFile = Path.GetFileName(exePath);
                }
            }
            catch (Exception)
            {
                // ignore 
            }
            var parameters = new DialogParameters<DialogGameInfoEdit>
            {
                { x => x.Model, inputModel }
            };
            IDialogReference dialogReference = await DialogService.ShowAsync<DialogGameInfoEdit>("Add new game",
                parameters,
                new DialogOptions
                {
                    MaxWidth = MaxWidth.Large,
                    FullWidth = true,
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult is null or { Canceled: true })
                return;
            if (dialogResult.Data is not DialogGameInfoEdit.FormModel resultModel)
                return;
            var gameInfo = new GameInfoDTO();
            IsLoading = true;
            await InvokeAsync(StateHasChanged);
            Debug.Assert(ConfigService != null);
            if (resultModel.Cover != null)
            {
                try
                {
                    if (!resultModel.Cover.IsHttpLink())
                        resultModel.Cover = await ConfigService.AddCoverImage(resultModel.Cover);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error occurred when adding cover image : {Exception}", e.ToString());
                    await DialogService.ShowMessageBox(
                        "Error",
                        $"{e.Message}"
                        , cancelText: "Cancel");
                }
            }

            DataMapService.Map(resultModel, gameInfo);
            try
            {
                gameInfo = await ConfigService.AddGameInfoAsync(gameInfo);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred when adding game info : {Exception}", e.ToString());
                SnakeBar.Add(Resources.Message_FailedToAddGameInfo, Severity.Error);
                IsLoading = false;
                _ = InvokeAsync(StateHasChanged);
                return;
            }

            ViewGameInfos.Insert(0, new ViewInfo
            {
                Info = gameInfo,
                Display = true
            });
            IsLoading = false;
            _ = InvokeAsync(StateHasChanged);

            return;

            Task<string?> ScanSaveFilePath()
            {
                List<string> possibleSaveDirNames = ["save", "savedata"];
                int level = 0;
                Queue<string> dirQueue = new();
                string? exeDir = Path.GetDirectoryName(exePath);
                if (exeDir == null)
                    return Task.FromResult<string?>(null);
                dirQueue.Enqueue(exeDir);
                while (level < 3)
                {
                    while (dirQueue.Count > 0)
                    {
                        string currentDir = dirQueue.Dequeue();
                        foreach (string dir in Directory.EnumerateDirectories(currentDir))
                        {
                            if (possibleSaveDirNames.Contains(Path.GetFileName(dir)))
                            {
                                return Task.FromResult<string?>(dir);
                            }

                            if (level + 1 < 3)
                                dirQueue.Enqueue(dir);
                        }
                    }

                    level++;
                }

                return Task.FromResult<string?>(null);
            }
        }

        private async Task OnDeleteGameCard(int id)
        {
            Logger.LogInformation("Delete game card with id {Id}", id);
            try
            {
                await ShowConfirmDialogAsync(Resources.Messeage_DeleteCheck);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            
            ViewInfo? item = ViewGameInfos.Find(x => x.Info.Id == id);
            if (item == null)
                return;
            bool hasException = await ExceptionHelper.ExecuteWithExceptionHandlingAsync(async () =>
            {
                await ConfigService.DeleteGameInfoByIdAsync(id);
            }, async ex =>
            {
                Logger.LogError(ex, "Error occurred when deleting game info");
                await DialogService.ShowMessageBox("Error", $"{ex.Message}", cancelText: "Cancel");
            });
            if (hasException)
                return;

            ViewGameInfos.Remove(item);
            _ = VirtualizeComponent?.RefreshDataAsync();
        }

        private async Task OnSearchInfo(ActionBar.SearchParameter parameter)
        {
            if (IsDeleting)
                return;
            IsLoading = true;
            await InvokeAsync(StateHasChanged);
            string pattern = parameter.SearchText?.Trim().ToLower() ?? "";
            await Parallel.ForEachAsync(ViewGameInfos, async (viewInfo, _) =>
            {
                bool display = string.IsNullOrEmpty(pattern);
                if (!display)
                {
                    string developer = viewInfo.Info.Developer?.ToLower() ?? "unknown";
                    string gameName = viewInfo.Info.GameName?.ToLower() ?? "";
                    string exePath = viewInfo.Info.ExePath?.ToLower() ?? "";
                    if (parameter.SearchFilter.SearchName)
                        display = gameName.Contains(pattern);
                    if (!display && parameter.SearchFilter.SearchDeveloper)
                        display = developer.Contains(pattern);
                    if (!display && parameter.SearchFilter.SearchExePath)
                        display = exePath.Contains(pattern);
                    if (!display && parameter.SearchFilter.SearchTag)
                        display = await ConfigService.CheckGameInfoHasTag(viewInfo.Info.Id, pattern);
                }

                viewInfo.Display = display;
            });

            _ = VirtualizeComponent?.RefreshDataAsync();
            IsLoading = false;
            _ = InvokeAsync(StateHasChanged);
        }

        private async Task ShowConfirmDialogAsync(string message)
        {
            DialogParameters<DialogConfirm> parameters = new()
            {
                { x => x.Content, message }
            };
            IDialogReference dialogReference = await DialogService.ShowAsync<DialogConfirm>("Warning", parameters,
                new DialogOptions
                {
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult is null or { Canceled: true })
                throw new TaskCanceledException();
        }

        private async Task OnDelete()
        {
            Logger.LogInformation("Bulk Delete button clicked");
            try
            {
                await ShowConfirmDialogAsync(Resources.Messeage_DeleteCheck);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            
            IsDeleting = true;
            await InvokeAsync(StateHasChanged);
            await ExceptionHelper.ExecuteWithExceptionHandlingAsync(async () =>
            {
                _deleteTaskCancellationTokenSource = new CancellationTokenSource();
                await Task.Run(async () =>
                {
                    var deleteItems = ViewGameInfos.Where(info => info.IsSelected)
                        .Select(x => new KeyValuePair<int, ViewInfo>(x.Info.Id, x)).ToDictionary();
                    CancellationToken token = _deleteTaskCancellationTokenSource.Token;
                    var idList = deleteItems.Select(x => x.Key).ToList();
                    await ConfigService.DeleteGameInfoByIdListAsync(idList, token, _ => { });
                }, _deleteTaskCancellationTokenSource.Token);
            }, async ex =>
            {
                if (ex is TaskCanceledException taskCanceledException)
                {
                    Logger.LogInformation(taskCanceledException, "Delete game info Task canceled");
                    return;
                }

                Logger.LogError(ex, "Error occurred when deleting game info");
                await DialogService.ShowMessageBox("Error", ex.Message, cancelText: "Cancel");
            }, async () =>
            {
                IsDeleting = false;
                await OnRefreshClick();
            });
        }

        private Task OnSortByChange(SortOrder order)
        {
            if (IsDeleting)
                return Task.CompletedTask;
            _currentOrder = order;
            switch (order)
            {
                case SortOrder.NAME:
                    ViewGameInfos.Sort((v1, v2) =>
                        string.Compare(v1.Info.GameName, v2.Info.GameName, StringComparison.Ordinal));
                    break;
                case SortOrder.UPLOAD_TIME:
                    ViewGameInfos.Sort((v1, v2) =>
                        DateTime.Compare((DateTime)v2.Info.UploadTime!, (DateTime)v1.Info.UploadTime!));
                    break;
                case SortOrder.DEVELOPER:
                    ViewGameInfos.Sort((v1, v2) =>
                        string.Compare(v1.Info.Developer, v2.Info.Developer, StringComparison.Ordinal));
                    break;
                case SortOrder.FAVORITE:
                    ViewGameInfos.Sort((v1, v2) =>
                        v2.Info.IsFavorite.CompareTo(v1.Info.IsFavorite));
                    break;
                case SortOrder.LAST_PLAYED:
                    ViewGameInfos.Sort((v1, v2) => DateTime.Compare(
                        v2.Info.LastPlayed ?? new DateTime(2000, 1, 1)
                        , v1.Info.LastPlayed ?? new DateTime(2000, 1, 1)));
                    break;
            }

            _ = VirtualizeComponent?.RefreshDataAsync();
            return Task.CompletedTask;
        }

        private void OnSelectionModeChange(bool value)
        {
            IsSelectionMode = value;
            if (!IsDeleting) return;
            foreach (ViewInfo viewInfo in ViewGameInfos)
            {
                viewInfo.IsSelected = false;
            }

            StateHasChanged();
        }

        private void OnSelectAllClick()
        {
            if (!IsSelectionMode || IsDeleting)
                return;
            foreach (ViewInfo viewInfo in ViewGameInfos)
            {
                if (viewInfo.Display)
                    viewInfo.IsSelected = true;
            }

            StateHasChanged();
        }

        private void OnCardClick(ViewInfo viewInfo)
        {
            if (!IsSelectionMode)
                return;
            viewInfo.IsSelected = !viewInfo.IsSelected;
            StateHasChanged();
        }

        private Task OnRefreshClick()
        {
            if (IsDeleting)
                return Task.CompletedTask;
            ViewGameInfos.Clear();
            _ = InvokeAsync(StateHasChanged);
            _loadingCancellationTokenSource = new CancellationTokenSource();
            return Task.Run(async () =>
            {
                Task loadTask = ConfigService.GetGameInfoForEachAsync(info =>
                {
                    ViewGameInfos.Add(new ViewInfo
                    {
                        Info = info,
                        Display = true
                    });
                }, _loadingCancellationTokenSource.Token);

                await loadTask;
                _ = OnSortByChange(_currentOrder);
            }, _loadingCancellationTokenSource.Token);
        }

        [JSInvokable("OnResizeEvent")]
        public async Task OnResizeEvent(int width, int height)
        {
            ValueTask<int> getWidthTask = JsRuntime.InvokeAsync<int>("getCardListWidth");
            ValueTask<float> getRemToPixels = JsRuntime.InvokeAsync<float>("remToPixels", 0.5);
            CardListWidth = await getWidthTask;
            CardGapPixel = (int)Math.Ceiling(await getRemToPixels);
            _ = VirtualizeComponent?.RefreshDataAsync();
            _ = InvokeAsync(StateHasChanged);
        }

        private ValueTask<ItemsProviderResult<IEnumerable<ViewInfo>>> CardItemProvider(
            ItemsProviderRequest request)
        {
            List<IEnumerable<ViewInfo>> items = [];
            const int paddingLeft = 15;
            int countOfCard = (CardListWidth - paddingLeft) / (CardItemWidth + CardGapPixel);
            var displayItem = ViewGameInfos.Where(info => info.Display).ToList();

            for (int i = request.StartIndex; i < request.StartIndex + request.Count; i++)
            {
                List<ViewInfo> rowCardItems = [];
                for (int j = 0; j < countOfCard; j++)
                {
                    int index = (i * countOfCard) + j;
                    if (index >= displayItem.Count)
                        break;
                    rowCardItems.Add(displayItem[index]);
                }

                items.Add(rowCardItems);
            }

            return new ValueTask<ItemsProviderResult<IEnumerable<ViewInfo>>>(
                new ItemsProviderResult<IEnumerable<ViewInfo>>(items,
                    (int)Math.Ceiling((float)displayItem.Count / countOfCard)));
        }

        private void OnChipTagClick(string chipText)
        {
            _ = OnSearchInfo(new ActionBar.SearchParameter(chipText, new ActionBar.SearchFilter
            {
                SearchExePath = false,
                SearchName = false
            }));
        }

        private Task OnShowDetail(int id)
        {
            IsShowDetail = true;
            _showDetailId = id;
            _ = InvokeAsync(StateHasChanged);
            return Task.CompletedTask;
        }

        private async Task OnCloseDetail()
        {
            IsShowDetail = false;
            await JsRuntime.InvokeVoidAsync("enableHtmlOverflow");
            GameInfoDTO? updateItem = await ConfigService.GetGameInfoBaseDTOAsync(_showDetailId);
            ViewInfo? viewInfo = ViewGameInfos.Find(x => x.Info.Id == _showDetailId);
            if (updateItem != null && viewInfo != null)
            {
                viewInfo.Info = updateItem;
            }

            _ = InvokeAsync(StateHasChanged);
        }

        private async Task OnEnableSyncClick()
        {
            await SetEnableState(true);
        }

        private async Task OnDisableSyncClick()
        {
            await SetEnableState(false);
        }

        private async Task SetEnableState(bool enableState)
        {
            List<ViewInfo> selectedViewInfos = ViewGameInfos.FindAll(x => x.IsSelected);
            var selectedInfos = selectedViewInfos.Select(x => x.Info.Id).ToList();
            bool hasException = await ExceptionHelper.ExecuteWithExceptionHandlingAsync(async () =>
            {
                await ConfigService.UpdateGameInfoSyncStatusAsync(selectedInfos, enableState);
            }, ex =>
            {
                Logger.LogError(ex, "Error occurred when enabling sync status");
                SnakeBar.Add(Resources.Message_UpdateDatabaseFailed, Severity.Error);
                return Task.CompletedTask;
            });
            if (hasException)
                return;
            Parallel.ForEach(selectedViewInfos, viewInfo =>
            {
                viewInfo.Info.EnableSync = enableState;
            });
            _ = InvokeAsync(StateHasChanged);
        }

        private class ViewInfo
        {
            public GameInfoDTO Info { get; set; } = null!;

            public bool Display { get; set; }

            public bool IsSelected { get; set; }
        }

        #region Lifecycle

        protected override Task OnInitializedAsync()
        {
            try
            {
                return base.OnInitializedAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred when initializing Home page : {Exception}", ex.ToString());
                throw;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            try
            {
                await base.OnAfterRenderAsync(firstRender);
                if (!firstRender)
                    return;
                AppSettingDTO appSetting = ConfigService.GetAppSettingDTO();
                if (appSetting.EnableSync)
                {
                    _ = TaskManager.StartBackgroundIntervalTask(App.SyncTaskJobName, () => TaskExecutor.SyncTask(),
                        TaskExecutor.CancelSyncTask, appSetting.SyncInterval, true);
                }

                _objRef = DotNetObjectReference.Create(this);
                await JsRuntime.InvokeVoidAsync("resizeHandlers.addResizeListener", _objRef);
                await base.OnInitializedAsync();
                _loadingCancellationTokenSource = new CancellationTokenSource();
                _ = Task.Run(async () =>
                {
                    Task loadTask = ConfigService.GetGameInfoForEachAsync(info =>
                    {
                        ViewGameInfos.Add(new ViewInfo
                        {
                            Info = info,
                            Display = string.IsNullOrEmpty(InitFilter) ||
                                      ConfigService.CheckGameInfoHasTag(info.Id, HttpUtility.UrlDecode(InitFilter))
                                          .Result
                        });
                    }, _loadingCancellationTokenSource.Token);

                    await loadTask;
                    IsLoading = false;
                    await InvokeAsync(StateHasChanged);
                    ValueTask<int> getWidthTask = JsRuntime.InvokeAsync<int>("getCardListWidth");
                    CardListWidth = await getWidthTask;
                }, _loadingCancellationTokenSource.Token);
                _ = DetectNewerVersion();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred when rendering Home page : {Exception}", ex.ToString());
                throw;
            }
        }

        protected override void OnParametersSet()
        {
            try
            {
                base.OnParametersSet();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred when setting parameters : {Exception}", ex.ToString());
                throw;
            }
        }

        #endregion
    }
}