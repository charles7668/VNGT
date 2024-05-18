using GameManager.Components.Pages.components;
using GameManager.Database;
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
        private bool _isCardUpdating;
        private CancellationTokenSource _loadingCancellationTokenSource = new();
        private DotNetObjectReference<Home> _objRef = null!;

        [Inject]
        public ISnackbar SnakeBar { get; set; } = null!;

        [Inject]
        private IUnitOfWork? UnitOfWork { get; set; }

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

        private int CardListRowCount { get; set; } = 1;

        private Virtualize<IEnumerable<ViewInfo>>? VirtualizeComponent { get; set; }

        private bool IsDeleting { get; set; }

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
            Debug.Assert(UnitOfWork != null);
            _loadingCancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                Task loadTask = UnitOfWork.GameInfoRepository.GetGameInfoForEachAsync(info =>
                {
                    ViewGameInfos.Add(new ViewInfo
                    {
                        Info = info,
                        Display = true
                    });
                }, _loadingCancellationTokenSource.Token);

                await loadTask;
                await InvokeAsync(StateHasChanged);
            }, _loadingCancellationTokenSource.Token);

            _ = base.OnInitializedAsync();
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
                await ConfigService.AddGameInfo(gameInfo);
            }
            catch (Exception e)
            {
                SnakeBar.Add(e.Message, Severity.Error);
                return;
            }

            ViewGameInfos.Add(new ViewInfo
            {
                Info = gameInfo,
                Display = true
            });
            StateHasChanged();
        }

        private async Task DeleteGameCard(int id)
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
                StateHasChanged();
        }

        private void OnSearchInfo(ActionBar.SearchParameter parameter)
        {
            if (IsDeleting)
                return;
            string pattern = parameter.SearchText?.Trim().ToLower() ?? "";
            for (int i = 0; i < ViewGameInfos.Count; i++)
            {
                ViewInfo viewInfo = ViewGameInfos[i];
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
                ViewGameInfos[i] = viewInfo;
            }

            StateHasChanged();
        }

        private async Task OnDelete()
        {
            IsDeleting = true;
            StateHasChanged();
            try
            {
                _deleteTaskCancellationTokenSource = new CancellationTokenSource();
                await Task.Run(async () =>
                {
                    var deleteItems = ViewGameInfos.Where(info => info.IsSelected).ToList();
                    CancellationToken token = _deleteTaskCancellationTokenSource.Token;

                    foreach (ViewInfo item in deleteItems)
                    {
                        if (token.IsCancellationRequested)
                            break;
                        await ConfigService.DeleteGameById(item.Info.Id);
                        ViewGameInfos.Remove(item);
                    }
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

        private async Task OnSortByChange(SortOrder order)
        {
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

            await InvokeAsync(StateHasChanged);
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
            Debug.Assert(UnitOfWork != null);
            ViewGameInfos.Clear();
            StateHasChanged();
            _loadingCancellationTokenSource = new CancellationTokenSource();
            return Task.Run(async () =>
            {
                Task loadTask = UnitOfWork.GameInfoRepository.GetGameInfoForEachAsync(info =>
                {
                    ViewGameInfos.Add(new ViewInfo
                    {
                        Info = info,
                        Display = true
                    });
                }, _loadingCancellationTokenSource.Token);

                await loadTask;
                if (VirtualizeComponent != null)
                    _ = VirtualizeComponent.RefreshDataAsync();
            }, _loadingCancellationTokenSource.Token);
        }

        [JSInvokable("OnResizeEvent")]
        public async Task OnResizeEvent(int width, int height)
        {
            ValueTask<int> getWidthTask = JsRuntime.InvokeAsync<int>("getCardListWidth");
            CardListWidth = await getWidthTask;
            if (!_isCardUpdating && VirtualizeComponent != null)
                _ = VirtualizeComponent.RefreshDataAsync();
        }

        private ValueTask<ItemsProviderResult<IEnumerable<ViewInfo>>> CardItemProvider(
            ItemsProviderRequest request)
        {
            _isCardUpdating = true;
            List<IEnumerable<ViewInfo>> items = [];
            const int paddingLeft = 15;
            int countOfCard = (CardListWidth - paddingLeft) / CardItemWidth;
            CardListRowCount = Math.Max((int)Math.Ceiling((float)ViewGameInfos.Count / countOfCard), 1);
            int start = request.StartIndex * countOfCard;
            for (int i = 0; i < request.Count; i++)
            {
                List<ViewInfo> rowCardItems = new();
                for (int j = 0; j < countOfCard;)
                {
                    int index = start;
                    if (index >= ViewGameInfos.Count)
                        break;
                    if (!ViewGameInfos[index].Display)
                        continue;
                    start++;
                    j++;
                    rowCardItems.Add(ViewGameInfos[index]);
                }

                items.Add(rowCardItems);
            }

            _ = InvokeAsync(StateHasChanged);
            _isCardUpdating = false;
            return new ValueTask<ItemsProviderResult<IEnumerable<ViewInfo>>>(
                new ItemsProviderResult<IEnumerable<ViewInfo>>(items, request.Count));
        }

        private class ViewInfo
        {
            public GameInfo Info { get; init; } = null!;

            public bool Display { get; set; }

            public bool IsSelected { get; set; }
        }
    }
}