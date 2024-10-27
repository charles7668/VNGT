using GameManager.DB.Models;
using GameManager.Extractor;
using GameManager.Models;
using GameManager.Models.LaunchProgramStrategies;
using GameManager.Properties;
using GameManager.Services;
using Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Utilities;
using System.Diagnostics;
using System.IO.Compression;
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

        private string ClassName => new CssBuilder(Class)
            .AddClass(IsSelected ? "selection" : "")
            .AddClass("ma-0 pa-0")
            .Build();

        [Parameter]
        public int CardItemWidth { get; set; } = 230;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;
        
        [Inject]
        private IImageService ImageService { get; set; } = null!;

        [Parameter]
        public GameInfo? GameInfo { get; set; }

        [Parameter]
        public bool IsSelected { get; set; }

        [Parameter]
        public EventCallback<int> OnDeleteEventCallback { get; set; }

        [Parameter]
        public EventCallback<int> OnClick { get; set; }

        [Parameter]
        public EventCallback<string> OnChipTagClickEvent { get; set; }

        private List<string> BackupSaveFiles { get; set; } = [];

        private AppSetting? AppSetting { get; set; }

        private IList<GuideSite> GuideSites
        {
            get
            {
                if (AppSetting != null)
                    return AppSetting.GuideSites;
                AppSetting = ConfigService.GetAppSetting();
                return AppSetting.GuideSites;
            }
        }

        private List<string> DeveloperList
        {
            get
            {
                var list = GameInfo?.Developer?.Split(',').ToList();
                if (list == null || list.Count == 0)
                    return ["UnKnown"];
                return list;
            }
        }

        private string ImageSrc =>
            ImageService.UriResolve(GameInfo?.CoverPath);

        private void OnCardClick()
        {
            if (GameInfo == null)
                return;
            if (OnClick.HasDelegate)
                OnClick.InvokeAsync(GameInfo.Id);
        }

        private Task OnCardFavoriteClick()
        {
            if (GameInfo == null)
                return Task.CompletedTask;
            GameInfo.IsFavorite = !GameInfo.IsFavorite;
            ConfigService.EditGameInfo(GameInfo);
            InvokeAsync(StateHasChanged);
            return Task.CompletedTask;
        }

        private Task OnChipClick(string developer)
        {
            if (OnChipTagClickEvent.HasDelegate)
                return OnChipTagClickEvent.InvokeAsync(developer);
            return Task.CompletedTask;
        }

        private async Task OnDelete()
        {
            if (GameInfo == null)
                return;
            Logger.LogInformation("Delete click");
            if (OnDeleteEventCallback.HasDelegate)
                await OnDeleteEventCallback.InvokeAsync(GameInfo.Id);
        }

        private async Task OnEdit()
        {
            if (GameInfo == null)
                return;
            Logger.LogInformation("on edit click");
            var inputModel = new DialogGameInfoEdit.FormModel();
            DataMapService.Map(GameInfo, inputModel);
            inputModel.Tags = (await ConfigService.GetGameTagsAsync(GameInfo.Id)).ToList();
            if (GameInfo.CoverPath != null && !CoverIsLocalFile(GameInfo.CoverPath))
                inputModel.Cover = GameInfo.CoverPath;
            else
                inputModel.Cover = ConfigService.GetCoverFullPath(GameInfo.CoverPath).Result;

            var parameters = new DialogParameters<DialogGameInfoEdit>
            {
                { x => x.Model, inputModel }
            };
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogGameInfoEdit>(
                Resources.Dialog_Title_EditGameInfo,
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
            try
            {
                if ((resultModel.Cover == null && GameInfo.CoverPath != null)
                    || (CoverIsLocalFile(resultModel.Cover) && !CoverIsLocalFile(GameInfo.CoverPath)))
                {
                    await ConfigService.DeleteCoverImage(GameInfo.CoverPath);
                }
                else if (resultModel.Cover != null &&
                         (!CoverIsLocalFile(GameInfo.CoverPath) || GameInfo.CoverPath == null) &&
                         CoverIsLocalFile(resultModel.Cover))
                {
                    resultModel.Cover = await ConfigService.AddCoverImage(resultModel.Cover);
                }
                else if (resultModel.Cover != null &&
                         CoverIsLocalFile(resultModel.Cover))
                {
                    await ConfigService.ReplaceCoverImage(resultModel.Cover, GameInfo.CoverPath);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Error : {Message}", e.ToString());
                await DialogService.ShowMessageBox("Error", $"{e.Message}", cancelText: "Cancel");
            }

            DataMapService.Map(resultModel, GameInfo);
            await ConfigService.EditGameInfo(GameInfo);
            await ConfigService.UpdateGameInfoTags(GameInfo.Id, resultModel.Tags);
            StateHasChanged();
            return;

            bool CoverIsLocalFile(string? coverUri)
            {
                if (coverUri == null)
                    return false;
                return !coverUri.IsHttpLink() && !coverUri.StartsWith("cors://");
            }
        }

        private Task OnGuideSearchClick(GuideSite site)
        {
            if (GameInfo == null)
            {
                return Task.CompletedTask;
            }

            string searchUrl = "https://www.google.com/search?q="
                               + HttpUtility.UrlEncode(GameInfo.GameName + $" site:{site.SiteUrl}");
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
            }
            catch (Exception e)
            {
                Logger.LogError("Error : {Message}", e.ToString());
                throw;
            }
        }

        #endregion

        private async Task OnLaunch()
        {
            if (GameInfo == null || string.IsNullOrEmpty(GameInfo.ExePath) || !Directory.Exists(GameInfo.ExePath))
            {
                Snackbar.Add(Resources.Message_NoExecutionFile, Severity.Warning);
                return;
            }

            Logger.LogInformation("Launch click");

            if (GameInfo.ExeFile is null or "Not Set")
            {
                Snackbar.Add(Resources.Message_PleaseSetExeFirst, Severity.Warning);
                return;
            }

            IStrategy launchStrategy = LaunchProgramStrategyFactory.Create(GameInfo, TryStartVNGTTranslator);
            try
            {
                await launchStrategy.ExecuteAsync();
                GameInfo.LastPlayed = DateTime.Now;
            }
            catch (Exception e)
            {
                Logger.LogError("Error : {Message}", e.ToString());
                Snackbar.Add(e.Message, Severity.Error);
            }

            return;

            void TryStartVNGTTranslator(int pid)
            {
                if (GameInfo?.LaunchOption is not { RunWithVNGTTranslator: true })
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
                    if (GameInfo.LaunchOption is { IsVNGTTranslatorNeedAdmin: true })
                    {
                        translatorInfo.UseShellExecute = true;
                        translatorInfo.Verb = "runas";
                    }

                    Process.Start(translatorInfo);
                });
            }
        }

        private void OnOpenInExplorer()
        {
            Debug.Assert(GameInfo != null);
            if (GameInfo.ExePath == null)
                return;
            Logger.LogInformation("Open in explorer click");
            try
            {
                // using "explorer.exe" and send path
                Process.Start("explorer.exe", GameInfo.ExePath);
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
            if (GameInfo == null)
                return;
            string? dirPath = GameInfo.SaveFilePath;
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

        private void OnSaveFileBackupClick(MouseEventArgs obj)
        {
            if (GameInfo?.ExePath == null)
                return;
            if (string.IsNullOrEmpty(GameInfo.SaveFilePath))
            {
                Snackbar.Add(Resources.Message_ParameterNotSet, Severity.Warning);
                return;
            }

            if (!Directory.Exists(GameInfo.SaveFilePath))
            {
                Snackbar.Add(GameInfo.SaveFilePath + Resources.Message_DirectoryNotExist, Severity.Warning);
                return;
            }

            string backupDirHash = HashHelper.GetMD5(GameInfo.ExePath);
            string backupDir = Path.Combine(AppPathService.SaveFileBackupDirPath, backupDirHash);
            Directory.CreateDirectory(backupDir);
            IEnumerable<string> oldFiles = Directory.EnumerateFiles(backupDir, "*.zip")
                .OrderByDescending(x => x).Skip(9);
            foreach (string file in oldFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Logger.LogError("Error to delete old save backup {File} : {Message}", file, e.ToString());
                }
            }

            using ZipArchive zipArchive =
                ZipFile.Open(Path.Combine(backupDir, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip"),
                    ZipArchiveMode.Create);
            string[] files = Directory.GetFiles(GameInfo.SaveFilePath, "*", SearchOption.AllDirectories);

            foreach (string filePath in files)
            {
                string relativePath = Path.GetRelativePath(GameInfo.SaveFilePath, filePath);
                zipArchive.CreateEntryFromFile(filePath, relativePath);
            }

            _menuRef?.CloseMenuAsync();
        }

        private void OnSaveFileRestoreClick(MouseEventArgs obj)
        {
            if (GameInfo?.ExePath == null)
                return;
            string backupDirHash = HashHelper.GetMD5(GameInfo.ExePath);
            string backupDir = Path.Combine(AppPathService.SaveFileBackupDirPath, backupDirHash);
            if (!Directory.Exists(backupDir))
                return;
            IEnumerable<string> files = Directory.EnumerateFiles(backupDir, "*.zip")
                .OrderByDescending(x => x).Take(10);
            BackupSaveFiles = files.Select(Path.GetFileNameWithoutExtension).ToList()!;
        }

        private async Task OnSaveFileStartRestore(string backupFile)
        {
            if (GameInfo?.ExePath == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(GameInfo?.SaveFilePath))
            {
                Snackbar.Add(Resources.Message_ParameterNotSet, Severity.Warning);
                return;
            }

            if (!Directory.Exists(GameInfo.SaveFilePath))
            {
                Snackbar.Add(GameInfo.SaveFilePath + Resources.Message_DirectoryNotExist, Severity.Warning);
                return;
            }

            string backupDirHash = HashHelper.GetMD5(GameInfo.ExePath);
            string filePath = Path.Combine(AppPathService.SaveFileBackupDirPath, backupDirHash, backupFile + ".zip");
            if (!File.Exists(filePath))
            {
                Snackbar.Add(backupFile + " " + Resources.Message_NotExist, Severity.Warning);
                return;
            }

            ExtractorFactory extractorFactory = App.ServiceProvider.GetRequiredService<ExtractorFactory>();
            IExtractor zipExtractor = extractorFactory.GetExtractor(".zip") ??
                                      throw new ArgumentException(".zip extractor not found");
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Result<string> extractResult = await zipExtractor.ExtractAsync(filePath,
                new ExtractOption
                {
                    TargetPath = tempPath
                });
            if (!extractResult.Success)
            {
                Logger.LogError("extract file to temp path failed : {ErrorMessage}", extractResult.Message);
                Snackbar.Add(extractResult.Message, Severity.Error);
                try
                {
                    Directory.Delete(tempPath);
                }
                catch (Exception ex)
                {
                    Logger.LogError("error to delete file : {Exception}", ex.ToString());
                }

                return;
            }

            try
            {
                FileHelper.CopyDirectory(tempPath, GameInfo.SaveFilePath);
            }
            catch (Exception e)
            {
                Logger.LogError("Error to copy file : {Message}", e.ToString());
                Snackbar.Add(e.Message, Severity.Error);
            }

            _menuRef?.CloseMenuAsync();
        }
    }
}