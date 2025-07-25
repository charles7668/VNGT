﻿using GameManager.DB.Models;
using GameManager.DTOs;
using GameManager.Models;
using GameManager.Models.EventArgs;
using GameManager.Modules.GamePlayMonitor;
using GameManager.Modules.LaunchProgramStrategies;
using GameManager.Modules.SaveDataManager;
using GameManager.Modules.TaskManager;
using GameManager.Properties;
using GameManager.Services;
using Helper;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Utilities;
using System.Diagnostics;
using System.Web;

namespace GameManager.Components.Pages.components
{
    public partial class Card
    {
        private MudMenu? _menuRef;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private new ILogger<Card> Logger { get; set; } = null!;

        [Inject]
        private IAppPathService AppPathService { get; set; } = null!;

        [Inject]
        private ITaskManager TaskManager { get; set; } = null!;

        private string ClassName => new CssBuilder(Class)
            .AddClass(IsSelected ? "selection" : "")
            .AddClass("ma-0 pa-0")
            .Build();

        [Parameter]
        public int CardItemWidth { get; set; } = 230;

        private int ItemWidthIncludeBorder => 230 - (IsSelected ? 6 : 0);

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        [Inject]
        private IImageService ImageService { get; set; } = null!;

        [Inject]
        private ISaveDataManager SaveDataManager { get; set; } = null!;

        [Inject]
        private IGamePlayMonitor GamePlayMonitor { get; set; } = null!;

        [Parameter]
        [EditorRequired]
        public GameInfoDTO GameInfoParam { get; set; } = null!;

        [Parameter]
        public bool IsSelected { get; set; }

        [Parameter]
        public EventCallback<int> OnDeleteEventCallback { get; set; }

        [Parameter]
        public EventCallback<int> OnShowDetail { get; set; }

        [Parameter]
        public EventCallback<int> OnClick { get; set; }

        [Parameter]
        public EventCallback<string> OnChipTagClickEvent { get; set; }

        private List<string> BackupSaveFiles { get; set; } = [];

        private AppSettingDTO? AppSetting { get; set; }

        private bool IsMonitoring { get; set; }

        private IList<GuideSiteDTO> GuideSites
        {
            get
            {
                if (AppSetting != null)
                    return AppSetting.GuideSites;
                AppSetting = ConfigService.GetAppSettingDTO();
                return AppSetting.GuideSites;
            }
        }

        private List<string> DeveloperList
        {
            get
            {
                var list = GameInfoParam.Developer?.Split(',').ToList();
                if (list == null || list.Count == 0)
                    return ["UnKnown"];
                return list;
            }
        }

        private string ImageSrc =>
            ImageService.UriResolve(GameInfoParam.CoverPath);

        private void OnCardClick()
        {
            if (OnClick.HasDelegate)
                OnClick.InvokeAsync(GameInfoParam.Id);
        }

        private Task OnCardFavoriteClick()
        {
            GameInfoParam.IsFavorite = !GameInfoParam.IsFavorite;
            try
            {
                ConfigService.UpdateGameInfoFavoriteAsync(GameInfoParam.Id, GameInfoParam.IsFavorite);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Update favorite error");
                Snackbar.Add(e.Message, Severity.Error);
            }

            return InvokeAsync(StateHasChanged);
        }

        private Task OnChipClick(string developer)
        {
            if (OnChipTagClickEvent.HasDelegate)
                return OnChipTagClickEvent.InvokeAsync(developer);
            return Task.CompletedTask;
        }

        private async Task OnDelete()
        {
            Logger.LogInformation("Delete {GameName} click", GameInfoParam.GameName);
            if (OnDeleteEventCallback.HasDelegate)
                await OnDeleteEventCallback.InvokeAsync(GameInfoParam.Id);
        }

        private async Task OnEdit()
        {
            Logger.LogInformation("on edit click");
            var inputModel = new DialogGameInfoEdit.FormModel();
            DataMapService.Map(GameInfoParam, inputModel);
            inputModel.Tags = (await ConfigService.GetGameTagsAsync(GameInfoParam.Id)).ToList();
            if (GameInfoParam.CoverPath != null && !CoverIsLocalFile(GameInfoParam.CoverPath))
                inputModel.Cover = GameInfoParam.CoverPath;
            else
                inputModel.Cover = ConfigService.GetCoverFullPath(GameInfoParam.CoverPath).Result;

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
            if (dialogResult is null or { Canceled: true })
                return;
            if (dialogResult.Data is not DialogGameInfoEdit.FormModel resultModel)
                return;
            try
            {
                if ((string.IsNullOrEmpty(resultModel.Cover) && !string.IsNullOrEmpty(GameInfoParam.CoverPath))
                    || (CoverIsLocalFile(resultModel.Cover) && !CoverIsLocalFile(GameInfoParam.CoverPath)))
                {
                    await ConfigService.DeleteCoverImage(GameInfoParam.CoverPath);
                }
                else if (!string.IsNullOrEmpty(resultModel.Cover) &&
                         (!CoverIsLocalFile(GameInfoParam.CoverPath) ||
                          string.IsNullOrEmpty(GameInfoParam.CoverPath)) &&
                         CoverIsLocalFile(resultModel.Cover))
                {
                    resultModel.Cover = await ConfigService.AddCoverImage(resultModel.Cover);
                }
                else if (!string.IsNullOrEmpty(resultModel.Cover) &&
                         CoverIsLocalFile(resultModel.Cover))
                {
                    await ConfigService.ReplaceCoverImage(resultModel.Cover, GameInfoParam.CoverPath);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Error : {Message}", e.ToString());
                await DialogService.ShowMessageBox("Error", $"{e.Message}", cancelText: "Cancel");
            }

            DataMapService.Map(resultModel, GameInfoParam);
            try
            {
                GameInfoParam.UpdatedTime = DateTime.UtcNow;
                GameInfoParam = await ConfigService.UpdateGameInfoAsync(GameInfoParam);
            }
            catch (Exception e)
            {
                GameInfoParam = (await ConfigService.GetGameInfoDTOAsync(x => x.Id == GameInfoParam.Id,
                    q => q.Include(x => x.LaunchOption)))!;
                Logger.LogError(e, "Error to edit game info");
                await DialogService.ShowMessageBox("Error", e.Message, cancelText: "Cancel");
            }

            StateHasChanged();
            return;

            bool CoverIsLocalFile(string? coverUri)
            {
                if (coverUri == null)
                    return false;
                return !coverUri.IsHttpLink() && !coverUri.StartsWith("cors://");
            }
        }

        private Task OnGuideSearchClick(GuideSiteDTO site)
        {
            string searchUrl = "https://www.google.com/search?q="
                               + HttpUtility.UrlEncode(GameInfoParam.GameName + $" site:{site.SiteUrl}");
            var startInfo = new ProcessStartInfo
            {
                FileName = searchUrl,
                UseShellExecute = true
            };
            Process.Start(startInfo);
            _menuRef?.CloseMenuAsync();
            return Task.CompletedTask;
        }

        #region LifeCycles

        protected override void OnInitialized()
        {
            try
            {
                base.OnInitialized();
                IsMonitoring = GamePlayMonitor.IsMonitoring(GameInfoParam.Id);
            }
            catch (Exception e)
            {
                Logger.LogError("Error : {Message}", e.ToString());
                throw;
            }
        }

        #endregion

        private async Task TryUpdateLastPlayTime()
        {
            int tryCount = 0;
            while (true)
            {
                try
                {
                    tryCount++;
                    await ConfigService.UpdateLastPlayedByIdAsync(GameInfoParam.Id,
                        GameInfoParam.LastPlayed!.Value);
                    break;
                }
                catch (DbUpdateException)
                {
                    if (tryCount >= 20)
                        throw;
                    await Task.Delay(50);
                }
            }
        }

        [UsedImplicitly]
        private async Task OnLaunch()
        {
            string? exePath = GameInfoParam.ExePath;
            string? exeFile = GameInfoParam.ExeFile;
            if (string.IsNullOrEmpty(exePath) || !Directory.Exists(exePath))
            {
                Snackbar.Add(Resources.Message_NoExecutionFile, Severity.Warning);
                return;
            }

            Logger.LogInformation("Launch click");

            if (exeFile is null or "Not Set")
            {
                Snackbar.Add(Resources.Message_PleaseSetExeFirst, Severity.Warning);
                return;
            }

            IStrategy launchStrategy = LaunchProgramStrategyFactory.Create(GameInfoParam, TryStartVNGTTranslator);
            try
            {
                int pid = await launchStrategy.ExecuteAsync();
                GameInfoParam.LastPlayed = DateTime.UtcNow;
                await TryUpdateLastPlayTime();
                Result addResult = await GamePlayMonitor.AddMonitorItem(GameInfoParam.Id, GameInfoParam.GameName ?? "",
                    pid,
                    e =>
                    {
                        TaskManager.StartBackgroundTask($"update-play-time-{e.GameId}",
                            () => TaskExecutor.UpdateGamePlayTimeTask(e.GameId, e.GameName, e.Duration), () => { });
                    });
                if (addResult.Success)
                {
                    GamePlayMonitor.RegisterCallback(GameInfoParam.Id, OnMonitorStop);
                    IsMonitoring = true;
                    _ = InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Error : {Message}", e.ToString());
                Snackbar.Add(e.Message, Severity.Error);
            }

            return;

            void TryStartVNGTTranslator(int pid)
            {
                if (GameInfoParam.LaunchOption is not { RunWithVNGTTranslator: true })
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
                    if (GameInfoParam.LaunchOption is { IsVNGTTranslatorNeedAdmin: true })
                    {
                        translatorInfo.UseShellExecute = true;
                        translatorInfo.Verb = "runas";
                    }

                    Process.Start(translatorInfo);
                });
            }
        }

        private void OnStopClick()
        {
            GamePlayMonitor.SendStopRequest(GameInfoParam.Id);
        }

        private void OnOpenInExplorer()
        {
            if (string.IsNullOrEmpty(GameInfoParam.ExePath))
                return;
            Logger.LogInformation("Open in explorer click");
            try
            {
                // using "explorer.exe" and send path
                Process.Start("explorer.exe", GameInfoParam.ExePath);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error : {Message}", ex.ToString());
                DialogService.ShowMessageBox("Error", ex.Message, cancelText: Resources.Dialog_Button_Cancel);
            }

            _menuRef?.CloseMenuAsync();
        }

        private void OnOpenSaveFilePath(MouseEventArgs obj)
        {
            string? dirPath = GameInfoParam.SaveFilePath;
            if (string.IsNullOrEmpty(dirPath))
            {
                Snackbar.Add(Resources.Message_ParameterNotSet, Severity.Warning);
                return;
            }

            if (!Directory.Exists(dirPath))
            {
                Snackbar.Add($"{dirPath} : " + Resources.Message_DirectoryNotExist, Severity.Warning);
                return;
            }

            Logger.LogInformation("Open save file path click");
            try
            {
                // using "explorer.exe" and send path
                Process.Start("explorer.exe", dirPath);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error : {Message}", ex.ToString());
                DialogService.ShowMessageBox("Error", ex.Message, cancelText: Resources.Dialog_Button_Cancel);
            }
        }

        private void ShowSnackBarFromResult<T>(Result<T> result)
        {
            Severity severity = Severity.Warning;
            if (result.Exception != null)
                severity = Severity.Error;
            Snackbar.Add(result.Message, severity);
        }

        private async Task OnSaveFileBackupClick(MouseEventArgs obj)
        {
            Result<string> result = await SaveDataManager.BackupSaveFileAsync(GameInfoParam);
            if (!result.Success)
            {
                ShowSnackBarFromResult(result);
            }

            _menuRef?.CloseMenuAsync();
        }

        private async Task OnSaveFileRestoreClick(MouseEventArgs obj)
        {
            BackupSaveFiles = await SaveDataManager.GetBackupListAsync(GameInfoParam);
        }

        private async Task OnSaveFileStartRestore(string backupFile)
        {
            Result result = await SaveDataManager.RestoreSaveFileAsync(GameInfoParam, backupFile);
            if (!result.Success)
            {
                ShowSnackBarFromResult(result);
            }

            _menuRef?.CloseMenuAsync();
        }

        private Task ShowDetail()
        {
            return OnShowDetail.HasDelegate ? OnShowDetail.InvokeAsync(GameInfoParam.Id) : Task.CompletedTask;
        }

        private void OnMonitorStop(GameStartEventArgs e)
        {
            IsMonitoring = false;
            InvokeAsync(StateHasChanged);
        }
    }
}