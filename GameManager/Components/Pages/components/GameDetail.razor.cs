using GameManager.DB.Models;
using GameManager.Models;
using GameManager.Models.LaunchProgramStrategies;
using GameManager.Properties;
using GameManager.Services;
using Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using System.Diagnostics;
using System.Web;
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
        private IAppPathService AppPathService { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        [Parameter]
        public int InitGameId { get; set; }

        private ViewModel GameInfoVo { get; set; } = null!;

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

        private Task LoadingTask { get; set; } = Task.CompletedTask;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private Lazy<AppSetting> AppSetting { get; set; } = null!;

        private IEnumerable<GuideSite> GuideSites => AppSetting.Value.GuideSites;

        protected override Task OnInitializedAsync()
        {
            AppSetting = new Lazy<AppSetting>(() => ConfigService.GetAppSetting());

            return base.OnInitializedAsync();
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (IsLoading && LoadingTask.IsCompleted)
            {
                LoadingTask = RefreshData();
            }

            return base.OnAfterRenderAsync(firstRender);
        }

        private Task RefreshData(GameInfo? inputGameInfo = null)
        {
            IsLoading = true;
            StateHasChanged();
            return Task.Run(async () =>
            {
                try
                {
                    Logger.LogInformation("start loading game info");
                    _ = JsRuntime.InvokeVoidAsync("disableHtmlOverflow");
                    GameInfo gameInfo = inputGameInfo ??
                                        await ConfigService.GetGameInfoAsync(x => x.Id == InitGameId) ??
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
                    GameInfoVo = new ViewModel(ImageService)
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
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Failed to load game info");
                }
                finally
                {
                    Logger.LogInformation("finish loading game info");
                }
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

        private async Task OnStartGameClick()
        {
            try
            {
                string? exePath = _gameInfo.ExePath;
                if (string.IsNullOrWhiteSpace(exePath) || !Directory.Exists(_gameInfo.ExePath))
                {
                    throw new FileNotFoundException(Resources.Message_NoExecutionFile);
                }

                string? exeFile = _gameInfo.ExeFile;

                if (exeFile is null or "Not Set")
                {
                    throw new FileNotFoundException(Resources.Message_PleaseSetExeFirst);
                }

                Logger.LogInformation("Start game click : {GameName}", _gameInfo.GameName);

                IStrategy launchStrategy = LaunchProgramStrategyFactory.Create(_gameInfo, TryStartVNGTTranslator);
                await launchStrategy.ExecuteAsync();
                _gameInfo.LastPlayed = DateTime.Now;
                GameInfoVo.LastPlayed = _gameInfo.LastPlayed?.ToString("yyyy-MM-dd") ?? "Never";
            }
            catch (FileNotFoundException e)
            {
                Snackbar.Add(e.Message, Severity.Warning);
            }
            catch (Exception e)
            {
                Logger.LogError("Error : {Message}", e.ToString());
                Snackbar.Add(e.Message, Severity.Error);
            }
            finally
            {
                Logger.LogInformation("Finish start game click : {GameName}", _gameInfo.GameName);
            }

            return;

            void TryStartVNGTTranslator(int pid)
            {
                if (_gameInfo.LaunchOption is not { RunWithVNGTTranslator: true })
                    return;
                if (!File.Exists(Path.Combine(AppPathService.ToolsDirPath,
                        "VNGTTranslator/VNGTTranslator.exe")))
                {
                    Snackbar.Add($"{Resources.Message_VNGTTranslatorNotInstalled}");
                    return;
                }

                _ = Task.Run(() =>
                {
                    var translatorInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(AppPathService.ToolsDirPath,
                            "VNGTTranslator/VNGTTranslator.exe"),
                        Arguments = $"{pid}"
                    };
                    if (_gameInfo.LaunchOption is { IsVNGTTranslatorNeedAdmin: true })
                    {
                        translatorInfo.UseShellExecute = true;
                        translatorInfo.Verb = "runas";
                    }

                    Process.Start(translatorInfo);
                });
            }
        }

        private void OnOpenInExplorerClick()
        {
            Logger.LogInformation("Open in explorer click");
            if (string.IsNullOrWhiteSpace(_gameInfo.ExePath))
            {
                Snackbar.Add(Resources.Message_DirectoryNotExist, Severity.Warning);
                return;
            }

            try
            {
                Process.Start("explorer.exe", _gameInfo.ExePath);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error : {Message}", ex.ToString());
                DialogService.ShowMessageBox("Error", ex.Message, cancelText: Resources.Dialog_Button_Cancel);
            }
        }

        private Task OnGuideSearchClick(GuideSite site)
        {
            string searchUrl = "https://www.google.com/search?q="
                               + HttpUtility.UrlEncode(_gameInfo.GameName + $" site:{site.SiteUrl}");
            var startInfo = new ProcessStartInfo
            {
                FileName = searchUrl,
                UseShellExecute = true
            };
            Process.Start(startInfo);
            return Task.CompletedTask;
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
            public string LastPlayed { get; set; } = "Never";
            public bool HasCharacters { get; init; }
        }
    }
}