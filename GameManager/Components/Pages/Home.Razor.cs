using GameManager.Components.Pages.components;
using GameManager.DB.Models;
using GameManager.Enums;
using GameManager.Services;
using Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using MudBlazor;
using System.Diagnostics;

namespace GameManager.Components.Pages
{
    public partial class Home : IDisposable
    {
        private CancellationTokenSource _deleteTaskCancellationTokenSource = new();
        private CancellationTokenSource _loadingCancellationTokenSource = new();
        private DotNetObjectReference<Home> _objRef = null!;

        [Inject]
        public ISnackbar SnakeBar { get; set; } = null!;

        private List<ViewInfo> ViewGameInfos { get; } = [];

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        private bool IsSelectionMode { get; set; }

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = null!;

        private int CardListWidth { get; set; }

        private int CardItemWidth { get; } = 275;

        private bool IsDeleting { get; set; }

        private bool IsLoading { get; set; } = true;

        private Virtualize<IEnumerable<ViewInfo>> VirtualizeComponent { get; set; } = null!;

        public void Dispose()
        {
            JsRuntime.InvokeVoidAsync("resizeHandlers.removeResizeListener");
            _deleteTaskCancellationTokenSource.Cancel();
            _loadingCancellationTokenSource.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
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
                        Display = true
                    });
                }, _loadingCancellationTokenSource.Token);

                await loadTask;
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
                ValueTask<int> getWidthTask = JsRuntime.InvokeAsync<int>("getCardListWidth");
                CardListWidth = await getWidthTask;
            }, _loadingCancellationTokenSource.Token);
        }

        private async Task AddNewGame(string exePath)
        {
            if (IsDeleting)
                return;
            var inputModel = new DialogGameInfoEdit.FormModel
            {
                GameName = Path.GetFileName(Path.GetDirectoryName(exePath)) ?? "null",
                ExePath = Path.GetDirectoryName(exePath)
            };
            var parameters = new DialogParameters<DialogGameInfoEdit>
            {
                { x => x.Model, inputModel }
            };
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogGameInfoEdit>("Add new game",
                parameters,
                new DialogOptions
                {
                    MaxWidth = MaxWidth.Large,
                    FullWidth = true,
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult.Canceled)
                return;
            if (dialogResult.Data is not DialogGameInfoEdit.FormModel resultModel)
                return;
            var gameInfo = new GameInfo();
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
                    await DialogService.ShowMessageBox(
                        "Error",
                        $"{e.Message}"
                        , cancelText: "Cancel");
                }
            }

            DataMapService.Map(resultModel, gameInfo);
            try
            {
                await ConfigService.AddGameInfoAsync(gameInfo);
            }
            catch (Exception e)
            {
                SnakeBar.Add(e.Message, Severity.Error);
                IsLoading = false;
                _ = InvokeAsync(StateHasChanged);
                return;
            }

            ViewGameInfos.Add(new ViewInfo
            {
                Info = gameInfo,
                Display = true
            });
            IsLoading = false;
            _ = InvokeAsync(StateHasChanged);
        }

        private async Task OnDeleteGameCard(int id)
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

            bool hasChane = false;
            for (int i = 0; i < ViewGameInfos.Count; i++)
            {
                if (ViewGameInfos[i].Info.Id != id)
                    continue;
                hasChane = true;
                try
                {
                    await ConfigService.DeleteGameById(id);
                }
                catch (Exception e)
                {
                    await DialogService.ShowMessageBox("Error", $"{e.Message}", cancelText: "Cancel");
                    return;
                }

                ViewGameInfos.RemoveAt(i);
                break;
            }

            if (hasChane)
            {
                _ = VirtualizeComponent.RefreshDataAsync();
            }
        }

        private Task OnSearchInfo(ActionBar.SearchParameter parameter)
        {
            if (IsDeleting)
                return Task.CompletedTask;
            string pattern = parameter.SearchText?.Trim().ToLower() ?? "";
            Parallel.ForEach(ViewGameInfos, viewInfo =>
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
                }

                viewInfo.Display = display;
            });

            _ = VirtualizeComponent.RefreshDataAsync();
            return Task.CompletedTask;
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
            IsDeleting = true;
            StateHasChanged();
            try
            {
                _deleteTaskCancellationTokenSource = new CancellationTokenSource();
                await Task.Run(async () =>
                {
                    var deleteItems = ViewGameInfos.Where(info => info.IsSelected).ToList();
                    CancellationToken token = _deleteTaskCancellationTokenSource.Token;

                    await Parallel.ForEachAsync(deleteItems, new ParallelOptions
                    {
                        CancellationToken = token
                    }, async (item, cancellationToken) =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;
                        await ConfigService.DeleteGameById(item.Info.Id);
                        ViewGameInfos.Remove(item);
                    });
                }, _deleteTaskCancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                if (!_deleteTaskCancellationTokenSource.Token.IsCancellationRequested)
                    await DialogService.ShowMessageBox("Error", e.Message, cancelText: "Cancel");
            }
            finally
            {
                IsDeleting = false;
                StateHasChanged();
            }
        }

        private Task OnSortByChange(SortOrder order)
        {
            if (IsDeleting)
                return Task.CompletedTask;
            switch (order)
            {
                case SortOrder.NAME:
                    ViewGameInfos.Sort((v1, v2) =>
                        string.Compare(v1.Info.GameName, v2.Info.GameName, StringComparison.Ordinal));
                    break;
                case SortOrder.UPLOAD_TIME:
                    ViewGameInfos.Sort((v1, v2) =>
                        DateTime.Compare((DateTime)v1.Info.UploadTime!, (DateTime)v2.Info.UploadTime!));
                    break;
                case SortOrder.DEVELOPER:
                    ViewGameInfos.Sort((v1, v2) =>
                        string.Compare(v1.Info.Developer, v2.Info.Developer, StringComparison.Ordinal));
                    break;
            }

            _ = VirtualizeComponent.RefreshDataAsync();
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
                _ = VirtualizeComponent.RefreshDataAsync();
            }, _loadingCancellationTokenSource.Token);
        }

        [JSInvokable("OnResizeEvent")]
        public async Task OnResizeEvent(int width, int height)
        {
            ValueTask<int> getWidthTask = JsRuntime.InvokeAsync<int>("getCardListWidth");
            CardListWidth = await getWidthTask;
        }

        private ValueTask<ItemsProviderResult<IEnumerable<ViewInfo>>> CardItemProvider(
            ItemsProviderRequest request)
        {
            List<IEnumerable<ViewInfo>> items = [];
            const int paddingLeft = 15;
            int countOfCard = (CardListWidth - paddingLeft) / CardItemWidth;
            var displayItem = ViewGameInfos.Where(info => info.Display).ToList();
            int start = request.StartIndex * countOfCard;
            int end = Math.Min(start + (request.Count * countOfCard), displayItem.Count);

            for (int i = start; i < end;)
            {
                List<ViewInfo> rowCardItems = [];
                for (int j = 0; j < countOfCard && i < end; j++, i++)
                {
                    rowCardItems.Add(displayItem[i]);
                }

                items.Add(rowCardItems);
            }

            _ = InvokeAsync(StateHasChanged);
            return new ValueTask<ItemsProviderResult<IEnumerable<ViewInfo>>>(
                new ItemsProviderResult<IEnumerable<ViewInfo>>(items,
                    (int)Math.Ceiling((float)displayItem.Count / countOfCard)));
        }

        private class ViewInfo
        {
            public GameInfo Info { get; init; } = null!;

            public bool Display { get; set; }

            public bool IsSelected { get; set; }
        }
    }
}