using GameManager.DB.Models;
using GameManager.DTOs;
using GameManager.Models;
using GameManager.Models.EventArgs;
using GameManager.Modules.GamePlayMonitor;
using GameManager.Modules.LaunchProgramStrategies;
using GameManager.Modules.TaskManager;
using GameManager.Properties;
using GameManager.Services;
using Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Web;
using ArgumentException = System.ArgumentException;

namespace GameManager.Components.Pages.components
{
    public partial class GameDetail : IAsyncDisposable
    {
        private GameInfoDTO _gameInfo = null!;

        private string _selectedTab = "info";

        [Inject]
        private IImageService ImageService { get; set; } = null!;

        [Inject]
        private IAppPathService AppPathService { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;
        
        [Inject]
        private ITaskManager TaskManager { get; set; } = null!;

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
        private IGamePlayMonitor GamePlayMonitor { get; set; } = null!;

        [Inject]
        private ILogger<GameDetail> Logger { get; set; } = null!;

        private Task LoadingTask { get; set; } = Task.CompletedTask;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private Lazy<AppSettingDTO> AppSetting { get; set; } = null!;

        private IEnumerable<GuideSiteDTO> GuideSites => AppSetting.Value.GuideSites;

        protected override Task OnInitializedAsync()
        {
            AppSetting = new Lazy<AppSettingDTO>(() => ConfigService.GetAppSettingDTO());

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

        private Task RefreshData(GameInfoDTO? inputGameInfo = null)
        {
            IsLoading = true;
            StateHasChanged();
            return Task.Run(async () =>
            {
                try
                {
                    Logger.LogInformation("start loading game info");
                    GameInfoDTO gameInfo = inputGameInfo ??
                                           await ConfigService.GetGameInfoDTOAsync(x => x.Id == InitGameId,
                                               q => q.Include(x => x.LaunchOption)) ??
                                           throw new ArgumentException("Game info not found with id " + InitGameId);
                    gameInfo.Staffs = await ConfigService.GetGameInfoStaffDTOs(x => x.Id == gameInfo.Id);
                    gameInfo.ReleaseInfos =
                        await ConfigService.GetGameInfoReleaseInfos(x => x.Id == gameInfo.Id);
                    gameInfo.RelatedSites =
                        await ConfigService.GetGameInfoRelatedSites(x => x.Id == gameInfo.Id);
                    gameInfo.Characters =
                        await ConfigService.GetGameInfoCharacters(x => x.Id == gameInfo.Id);
                    gameInfo.Tags = (await ConfigService.GetGameTagsAsync(gameInfo.Id)).Select(x => new TagDTO
                    {
                        Name = x
                    }).ToList();
                    _gameInfo = gameInfo;
                    bool monitorStatus = GamePlayMonitor.IsMonitoring(gameInfo.Id);
                    if (monitorStatus)
                    {
                        GamePlayMonitor.RegisterCallback(gameInfo.Id, OnMonitoringStopCallback);
                    }
                    GameInfoVo = new ViewModel(ImageService)
                    {
                        OriginalName = gameInfo.GameName,
                        ChineseName = gameInfo.GameChineseName,
                        EnglishName = gameInfo.GameEnglishName,
                        CoverImage = gameInfo.CoverPath,
                        BackgroundImage = gameInfo.BackgroundImageUrl,
                        ScreenShots = gameInfo.ScreenShots,
                        LastPlayed = gameInfo.LastPlayed?.ToString("yyyy-MM-dd") ?? "Never",
                        HasCharacters = gameInfo.Characters.Count != 0,
                        EnableSync = gameInfo.EnableSync,
                        PlayTime = gameInfo.PlayTime,
                        IsMonitoring = monitorStatus
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
                if ((string.IsNullOrEmpty(resultModel.Cover) && !string.IsNullOrEmpty(_gameInfo.CoverPath))
                    || (CoverIsLocalFile(resultModel.Cover) && !CoverIsLocalFile(_gameInfo.CoverPath)))
                {
                    await ConfigService.DeleteCoverImage(_gameInfo.CoverPath);
                }
                else if (!string.IsNullOrEmpty(resultModel.Cover) &&
                         (!CoverIsLocalFile(_gameInfo.CoverPath) ||
                          string.IsNullOrEmpty(_gameInfo.CoverPath)) &&
                         CoverIsLocalFile(resultModel.Cover))
                {
                    resultModel.Cover = await ConfigService.AddCoverImage(resultModel.Cover);
                }
                else if (!string.IsNullOrEmpty(resultModel.Cover) &&
                         CoverIsLocalFile(resultModel.Cover))
                {
                    await ConfigService.ReplaceCoverImage(resultModel.Cover, _gameInfo.CoverPath);
                }

                DataMapService.Map(resultModel, _gameInfo);
                _gameInfo.UpdatedTime = DateTime.UtcNow;
                await ConfigService.UpdateGameInfoAsync(_gameInfo);
            }
            catch (Exception e)
            {
                Logger.LogError("Error : {Message}", e.ToString());
                await DialogService.ShowMessageBox("Error", $"{e.Message}", cancelText: "Cancel");
            }

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
                var monitorPid = await launchStrategy.ExecuteAsync();
                _gameInfo.LastPlayed = DateTime.UtcNow;
                GameInfoVo.LastPlayed = _gameInfo.LastPlayed?.ToString("yyyy-MM-dd") ?? "Never";
                await ConfigService.UpdateLastPlayedByIdAsync(_gameInfo.Id, _gameInfo.LastPlayed!.Value);
                Result addResult =
                    await GamePlayMonitor.AddMonitorItem(_gameInfo.Id, _gameInfo.GameName ?? "", monitorPid, e =>
                    {
                        TaskManager.StartBackgroundTask($"update-play-time-{e.GameId}",
                            () => TaskExecutor.UpdateGamePlayTimeTask(e.GameId, e.GameName, e.Duration), () => { });
                    });
                if (addResult.Success)
                {
                    GamePlayMonitor.RegisterCallback(_gameInfo.Id, OnMonitoringStopCallback);
                    GameInfoVo.IsMonitoring = true;
                    _ = InvokeAsync(StateHasChanged);
                }
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

        private void OpenPath(string? path, [CallerMemberName] string? callerName = "")
        {
            Logger.LogInformation("Open path {Path} called by {Caller}", path, callerName);
            if (string.IsNullOrWhiteSpace(path))
            {
                Snackbar.Add(Resources.Message_DirectoryNotExist, Severity.Warning);
                return;
            }

            Result result = PathService.OpenPathInExplorer(path);
            if (result.Success) return;
            Logger.LogError(result.Exception, "Open Path Error : {Message}", result.Message);
            Snackbar.Add(result.Message, Severity.Error);
        }

        private void OnOpenSaveFilePathClick()
        {
            OpenPath(_gameInfo.SaveFilePath);
        }

        private void OnOpenInExplorerClick()
        {
            OpenPath(_gameInfo.ExePath);
        }

        private Task OnGuideSearchClick(GuideSiteDTO site)
        {
            Logger.LogInformation("Search guide for {GameName} on {SiteName}", _gameInfo.GameName, site.Name);
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

        private void OnMonitoringStopCallback(GameStartEventArgs e)
        {
            GameInfoVo.IsMonitoring = false;
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
            public string LastPlayed { get; set; } = "Never";
            public bool HasCharacters { get; init; }
            public bool EnableSync { get; set; }
            public bool IsMonitoring { get; set; }
            public double PlayTime { get; set; }

            public string DisplayPlayTime => (int)PlayTime == 0
                ? Resources.DetailPage_SmanllThanOneMinutes
                : $"{(int)PlayTime} {Resources.Common_Minutes}";
        }

        public async ValueTask DisposeAsync()
        {
            await CastAndDispose(MudDialog);
            await CastAndDispose(LoadingTask);
            await CastAndDispose(Snackbar);
            GamePlayMonitor.UnregisterCallback(_gameInfo.Id, OnMonitoringStopCallback);

            static async ValueTask CastAndDispose(object? resource)
            {
                switch (resource)
                {
                    case null:
                        return;
                    case IAsyncDisposable resourceAsyncDisposable:
                        await resourceAsyncDisposable.DisposeAsync();
                        break;
                    case IDisposable resourceDisposable:
                        resourceDisposable.Dispose();
                        break;
                }
            }
        }
    }
}