using GameManager.DB.Models;
using GameManager.DTOs;
using GameManager.GameInfoProvider;
using GameManager.Properties;
using GameManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Utilities;

namespace GameManager.Components.Pages.components
{
    public partial class GameDetailScreenShots : IAsyncDisposable
    {
        private string _fetchProvider = "VNDB";

        private string _fetchSearchText = string.Empty;
        private CancellationTokenSource _fetchTaskCts = new();

        [Parameter]
        [EditorRequired]
        public GameInfoDTO GameInfo { get; set; } = null!;

        private ViewModel GameInfoVo { get; } = new();

        [Inject]
        private IImageService ImageService { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private ILogger<GameDetailScreenShots> Logger { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Parameter]
        public EventCallback OnUpdateNeeded { get; set; }

        private bool IsLoading { get; set; } = true;
        private Task LoadingTask { get; set; } = Task.CompletedTask;
        private Task FetchTask { get; set; } = Task.CompletedTask;

        [Inject]
        private GameInfoProviderFactory GameInfoProviderFactory { get; set; } = null!;

        public ValueTask DisposeAsync()
        {
            _fetchTaskCts.Cancel();
            return ValueTask.CompletedTask;
        }

        private string GetImageContainerClass(ScreenShotViewModel model)
        {
            return CssBuilder.Default("image-container")
                .AddClass("selected", model.IsSelected)
                .Build();
        }

        private void OnImageClick(ScreenShotViewModel model)
        {
            model.IsSelected = !model.IsSelected;

            InvokeAsync(StateHasChanged);
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (IsLoading && LoadingTask.IsCompleted)
            {
                LoadingTask = Task.Run(() =>
                {
                    _fetchSearchText = GameInfo.GameName ?? "";
                    GameInfoVo.ScreenShots = GameInfo.ScreenShots.Select(x => new ScreenShotViewModel(ImageService)
                    {
                        Url = x
                    }).ToList();
                    IsLoading = false;
                    InvokeAsync(StateHasChanged);
                });
            }

            return base.OnAfterRenderAsync(firstRender);
        }

        private async Task UpdateBackgroundImage()
        {
            ScreenShotViewModel? screenshotVo = GameInfoVo.ScreenShots.FirstOrDefault(x => x.IsSelected);
            if (screenshotVo == null)
                return;
            try
            {
                await ConfigService.UpdateGameInfoBackgroundImageAsync(GameInfo.Id, screenshotVo.Url);
                GameInfo.BackgroundImageUrl = screenshotVo.Url;
                if (OnUpdateNeeded.HasDelegate)
                    await OnUpdateNeeded.InvokeAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to update background image");
                Snackbar.Add(Resources.Message_FailedToUpdateBackgroundImage, Severity.Error);
            }
        }

        private async Task TryAddScreenShotsToGameInfo(List<string> urls)
        {
            try
            {
                await ConfigService.AddScreenshotsAsync(GameInfo.Id, urls);
                List<string> screenshots = GameInfo.ScreenShots;
                screenshots.AddRange(urls);
                screenshots = screenshots.Distinct().ToList();
                GameInfo.ScreenShots = screenshots;
                GameInfoVo.ScreenShots = screenshots.Select(x => new ScreenShotViewModel(ImageService)
                {
                    Url = x
                }).ToList();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to add screenshots");
                Snackbar.Add(Resources.Message_FailedToAddScreenshots, Severity.Error);
            }
        }

        private async Task AddScreenshotsByUrl()
        {
            IDialogReference dialogReference = await DialogService.ShowAsync<DialogMultiLineInputBox>();
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult is { Canceled: true })
            {
                return;
            }

            string? inputText = dialogResult?.Data as string;
            string[] splitText = inputText?.Split('\n') ?? [];
            for (int i = 0; i < splitText.Length; i++)
            {
                splitText[i] = splitText[i].Trim().Trim('\r');
            }

            await TryAddScreenShotsToGameInfo(splitText.ToList());
        }

        private async Task OnRemoveScreenshotClick()
        {
            var screenshotVos = GameInfoVo.ScreenShots.Where(x => x.IsSelected).ToList();
            if (screenshotVos.Count == 0)
                return;
            try
            {
                await ConfigService.RemoveScreenshotsAsync(GameInfo.Id, screenshotVos.Select(x => x.Url).ToList());
                GameInfoVo.ScreenShots.RemoveAll(x => screenshotVos.Contains(x));
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to remove screenshot");
                Snackbar.Add(Resources.Message_FailedToRemoveScreenshots, Severity.Error);
            }
        }

        private Task OnFetchButtonClick()
        {
            if (!FetchTask.IsCompleted)
                return Task.CompletedTask;
            string fetchProviderName = _fetchProvider;
            string searchText = _fetchSearchText;
            Logger.LogInformation("Start fetch game info from {Provider} with search text {SearchText}",
                fetchProviderName,
                searchText);
            _fetchTaskCts = new CancellationTokenSource();
            FetchTask = Task.Run<Task>(async () =>
            {
                CancellationToken cancellationToken = _fetchTaskCts.Token;
                try
                {
                    IGameInfoProvider? provider = GameInfoProviderFactory.GetProvider(fetchProviderName);
                    if (provider is null)
                    {
                        Logger.LogError("Provider {Provider} not found", fetchProviderName);
                        return;
                    }

                    (List<GameInfoDTO>? infoList, bool hasMore) =
                        await provider.FetchGameSearchListAsync(searchText, 10, 1);

                    if (cancellationToken.IsCancellationRequested)
                        throw new TaskCanceledException();

                    if (infoList == null || infoList.Count == 0)
                        throw new FileNotFoundException();

                    var parameters = new DialogParameters<DialogFetchSelection>
                    {
                        { x => x.DisplayInfos, infoList },
                        { x => x.HasMore, hasMore },
                        { x => x.SearchName, searchText },
                        { x => x.ProviderName, provider.ProviderName }
                    };
                    IDialogReference dialogReference = await DialogService.ShowAsync<DialogFetchSelection>("",
                        parameters,
                        new DialogOptions
                        {
                            BackdropClick = false
                        });
                    DialogResult? dialogResult = await dialogReference.Result;
                    if ((dialogResult?.Canceled ?? true) || dialogResult.Data is not string gameId)
                        throw new TaskCanceledException();
                    if (gameId == null)
                        throw new FileNotFoundException("Game ID not found");

                    GameInfoDTO? info = await provider.FetchGameDetailByIdAsync(gameId);
                    if (info == null)
                        return;
                    if (cancellationToken.IsCancellationRequested)
                        throw new TaskCanceledException();
                    await TryAddScreenShotsToGameInfo(info.ScreenShots);
                }
                catch (FileNotFoundException e)
                {
                    Logger.LogError(e, "Failed to fetch game info");
                    await DialogService.ShowMessageBox("Error", Resources.Message_RelatedGameNotFound,
                        Resources.Dialog_Button_Cancel);
                }
                catch (TaskCanceledException)
                {
                    Logger.LogInformation("Fetch game info canceled");
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Failed to fetch game info");
                    await DialogService.ShowMessageBox("Error", e.Message, Resources.Dialog_Button_Cancel);
                }
                finally
                {
                    Logger.LogInformation("Fetch game info finished");
                    _ = InvokeAsync(StateHasChanged);
                }
            }, _fetchTaskCts.Token);
            return Task.CompletedTask;
        }

        private class ScreenShotViewModel(IImageService imageService)
        {
            public string Url { get; set; } = string.Empty;
            public string Display => imageService.UriResolve(Url);
            public bool IsSelected { get; set; }
        }

        private class ViewModel
        {
            public List<ScreenShotViewModel> ScreenShots { get; set; } = [];
        }
    }
}