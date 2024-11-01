using GameManager.DB.Models;
using GameManager.Properties;
using GameManager.Services;
using Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using ArgumentException = System.ArgumentException;

namespace GameManager.Components.Pages.components
{
    public partial class GameDetail
    {
        private GameInfo _gameInfo = null!;

        private string _selectedTab = "info";

        [Inject]
        private IImageService ImageService { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        [Parameter]
        public int InitGameId { get; set; }

        private ViewModel GameInfoViewModel { get; set; } = null!;

        private bool IsLoading { get; set; } = true;

        [CascadingParameter]
        private MudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public EventCallback OnReturnClick { get; set; }

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private ILogger<GameDetail> Logger { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            _ = RefreshData();
            await base.OnInitializedAsync();
        }

        private Task RefreshData(GameInfo? inputGameInfo = null)
        {
            IsLoading = true;
            StateHasChanged();
            return Task.Run(async () =>
            {
                GameInfo gameInfo = inputGameInfo ?? await ConfigService.GetGameInfoAsync(x => x.Id == InitGameId) ??
                    throw new ArgumentException("Game info not found with id " + InitGameId);
                gameInfo.Staffs = (await ConfigService.GetGameInfoStaffs(x => x.Id == gameInfo.Id)).ToList();
                gameInfo.ReleaseInfos =
                    (await ConfigService.GetGameInfoReleaseInfos(x => x.Id == gameInfo.Id)).ToList();
                gameInfo.RelatedSites =
                    (await ConfigService.GetGameInfoRelatedSites(x => x.Id == gameInfo.Id)).ToList();
                gameInfo.Characters =
                    (await ConfigService.GetGameInfoCharacters(x => x.Id == gameInfo.Id)).ToList();
                gameInfo.Tags = (await ConfigService.GetGameTagsAsync(gameInfo.Id)).Select(x => new Tag
                {
                    Name = x
                }).ToList();
                _gameInfo = gameInfo;
                GameInfoViewModel = new ViewModel(ImageService)
                {
                    OriginalName = gameInfo.GameName,
                    ChineseName = gameInfo.GameChineseName,
                    EnglishName = gameInfo.GameEnglishName,
                    CoverImage = gameInfo.CoverPath,
                    BackgroundImage = gameInfo.BackgroundImageUrl,
                    ScreenShots = gameInfo.ScreenShots,
                    LastPlayed = gameInfo.LastPlayed?.ToString("yyyy-MM-dd") ?? "Never",
                    HasCharacters = gameInfo.Characters.Count != 0
                };
                IsLoading = false;
                _ = InvokeAsync(StateHasChanged);
            });
        }

        private async Task OnReturnClickHandler()
        {
            if (OnReturnClick.HasDelegate)
            {
                await OnReturnClick.InvokeAsync();
            }
        }

        /// <summary>
        /// this method is call by child component
        /// </summary>
        private async Task OnUpdateNeededHandler()
        {
            await RefreshData(_gameInfo);
        }

        private async Task OnEditGameInfo()
        {
            Logger.LogInformation("Open Dialog Edit GameInfo");
            var inputModel = new DialogGameInfoEdit.FormModel();
            DataMapService.Map(_gameInfo, inputModel);
            inputModel.Tags = _gameInfo.Tags.Select(x => x.Name).ToList();
            if (_gameInfo.CoverPath != null && !CoverIsLocalFile(_gameInfo.CoverPath))
                inputModel.Cover = _gameInfo.CoverPath;
            else
                inputModel.Cover = ConfigService.GetCoverFullPath(_gameInfo.CoverPath).Result;

            var parameters = new DialogParameters<DialogGameInfoEdit>
            {
                { x => x.Model, inputModel }
            };
            IDialogReference dialogReference = await DialogService.ShowAsync<DialogGameInfoEdit>(
                Resources.Dialog_Title_EditGameInfo,
                parameters,
                new DialogOptions
                {
                    MaxWidth = MaxWidth.Large,
                    FullWidth = true,
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult?.Canceled is true)
                return;
            if (dialogResult?.Data is not DialogGameInfoEdit.FormModel resultModel)
                return;
            try
            {
                if ((resultModel.Cover == null && _gameInfo.CoverPath != null)
                    || (CoverIsLocalFile(resultModel.Cover) && !CoverIsLocalFile(_gameInfo.CoverPath)))
                {
                    await ConfigService.DeleteCoverImage(_gameInfo.CoverPath);
                }
                else if (resultModel.Cover != null &&
                         (!CoverIsLocalFile(_gameInfo.CoverPath) || _gameInfo.CoverPath == null) &&
                         CoverIsLocalFile(resultModel.Cover))
                {
                    resultModel.Cover = await ConfigService.AddCoverImage(resultModel.Cover);
                }
                else if (resultModel.Cover != null &&
                         CoverIsLocalFile(resultModel.Cover))
                {
                    await ConfigService.ReplaceCoverImage(resultModel.Cover, _gameInfo.CoverPath);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Error : {Message}", e.ToString());
                await DialogService.ShowMessageBox("Error", $"{e.Message}", cancelText: "Cancel");
            }

            _gameInfo.Tags = [];
            DataMapService.Map(resultModel, _gameInfo);
            await ConfigService.EditGameInfo(_gameInfo);
            await ConfigService.UpdateGameInfoTags(_gameInfo.Id, resultModel.Tags);
            await RefreshData();
            return;

            bool CoverIsLocalFile(string? coverUri)
            {
                if (coverUri == null)
                    return false;
                return !coverUri.IsHttpLink() && !coverUri.StartsWith("cors://");
            }
        }

        private void OnTabChangeClick(string tabName)
        {
            if (_selectedTab == tabName)
                return;
            _selectedTab = tabName;
            InvokeAsync(StateHasChanged);
        }

        private class ViewModel(IImageService imageService)
        {
            public string? OriginalName { get; init; }
            public string? ChineseName { get; init; }
            public string? EnglishName { get; init; }
            public string? CoverImage { get; init; }
            public string DisplayBackgroundImage => imageService.UriResolve(BackgroundImage, "");
            public string DisplayCoverImage => imageService.UriResolve(CoverImage);
            public string? BackgroundImage { get; init; }
            public List<string> ScreenShots { get; init; } = [];
            public string LastPlayed { get; init; } = "Never";
            public bool HasCharacters { get; init; }
        }
    }
}