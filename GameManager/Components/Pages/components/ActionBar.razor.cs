using GameManager.DB.Models;
using GameManager.DTOs;
using GameManager.Enums;
using GameManager.Extractor;
using GameManager.Models;
using GameManager.Modules.GameInstallAnalyzer;
using GameManager.Properties;
using GameManager.Services;
using Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;
using FileInfo = GameManager.Models.FileInfo;

namespace GameManager.Components.Pages.components
{
    public partial class ActionBar
    {
        [Parameter]
        public string? SearchText { get; set; }

        [Parameter]
        public EventCallback<string> AddNewGameEvent { get; set; }

        [Parameter]
        public EventCallback<SearchParameter> SearchEvent { get; set; }

        [Parameter]
        public Func<string, CancellationToken, Task<List<string>>>? SearchSuggestionFunc { get; set; }

        [Parameter]
        public EventCallback OnDeleteEvent { get; set; }

        [Parameter]
        public EventCallback OnRefreshEvent { get; set; }

        private Dictionary<SortOrder, string> SortOrderDict { get; set; } = null!;

        private SortOrder SortBy { get; set; } = SortOrder.UPLOAD_TIME;

        [Parameter]
        public EventCallback<SortOrder> OnSortByChangeEvent { get; set; }

        private SearchFilter SearchFilterModel { get; set; } = new();

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private IAppPathService AppPathService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        private new ILogger<ActionBar> Logger { get; set; } = null!;

        private MudAutocomplete<string> SearchAutoCompleteRef { get; set; } = null!;

        private List<string> suggestions = [];

        protected override void OnInitialized()
        {
            SortOrderDict = new Dictionary<SortOrder, string>
            {
                { SortOrder.NAME, Resources.Home_SortBy_GameName },
                { SortOrder.UPLOAD_TIME, Resources.Home_SortBy_UploadTime },
                { SortOrder.DEVELOPER, Resources.Home_SortBy_Developer },
                { SortOrder.FAVORITE, Resources.Home_SortBy_Favorite },
                { SortOrder.LAST_PLAYED, Resources.Home_SortBy_LastPlayed }
            };

            base.OnInitialized();
        }

        private async Task OnInstallGameClick()
        {
            string processTracingToolDir = AppPathService.ProcessTracerDirPath;
            string processTracingToolPath = Path.Combine(processTracingToolDir, "ProcessTracer.exe");
            if (!File.Exists(processTracingToolPath))
            {
                Snackbar.Add("The program is missing a required tool: ProcessTracer.\r\nPlease consider reinstalling it.", Severity.Warning);
                return;
            }
            
            AppSettingDTO appSetting = ConfigService.GetAppSettingDTO();
            string? lePath = appSetting.LocaleEmulatorPath;
            string leExePath = string.IsNullOrEmpty(lePath) ? "" : Path.Combine(lePath, "LEProc.exe");
            string guid = "";
            if (File.Exists(leExePath))
            {
                IDialogReference dialogReference = await DialogService.ShowAsync<DialogUseLECheck>("Information",
                    new DialogOptions
                    {
                        MaxWidth = MaxWidth.Small,
                        FullWidth = true,
                        BackdropClick = false
                    });
                DialogResult? result = await dialogReference.Result;
                if (result is { Data: not null } && (string)result.Data != "None")
                {
                    string leConfigPath = Path.Combine(appSetting.LocaleEmulatorPath!, "LEConfig.xml");
                    if (!File.Exists(leConfigPath))
                    {
                        Snackbar.Add(Resources.Message_LENotFound, Severity.Error);
                        return;
                    }
            
                    var xmlDoc = XDocument.Load(leConfigPath);
                    XElement? node =
                        xmlDoc.XPathSelectElement(
                            $"//Profiles/Profile[@Name='{result.Data}']");
                    XAttribute? guidAttr = node?.Attribute("Guid");
                    if (guidAttr != null)
                    {
                        guid = guidAttr.Value;
                    }
                }
                else
                {
                    return;
                }
            }
            
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, [".exe"] }
            });
            
            var options = new PickOptions
            {
                PickerTitle = "Please select install file",
                FileTypes = customFileType
            };
            
            FileResult? installFileResult = await FilePicker.PickAsync(options);
            if (installFileResult == null)
            {
                return;
            }
            
            string tempPath =
                Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            Directory.CreateDirectory(tempPath);
            string tempOutputPath = Path.Combine(tempPath, "output.txt");
            string tempErrorPath = Path.Combine(tempPath, "error.txt");
            
            IDialogReference dialogReferenceProgress = await DialogService.ShowAsync<ProgressDialog>("Installing",
                new DialogOptions
                {
                    FullWidth = true,
                    MaxWidth = MaxWidth.Small
                });
            try
            {
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
                ProcessStartInfo processTracerStartInfo;
                if (!string.IsNullOrEmpty(guid))
                {
                    string[] args =
                    [
                        $"-f\"{leExePath.ToUnixPath()}\"",
                        $"-a\"-runas {guid} \\\"{installFileResult.FullPath.ToUnixPath()}\\\"\"",
                        $"-o\"{tempOutputPath.ToUnixPath()}\"",
                        $"-e\"{tempErrorPath.ToUnixPath()}\"",
                        "--hide"
                    ];
                    processTracerStartInfo = new ProcessStartInfo()
                    {
                        FileName = processTracingToolPath,
                        WorkingDirectory = AppPathService.AppDirPath,
                        Arguments = string.Join(' ', args),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                }
                else
                {
                    string[] args =
                    [
                        $"-f\"{installFileResult.FullPath.ToUnixPath()}\"",
                        $"-o\"{tempOutputPath.ToUnixPath()}\"",
                        $"-e\"{tempErrorPath.ToUnixPath()}\"",
                        "--hide"
                    ];
                    processTracerStartInfo = new ProcessStartInfo()
                    {
                        FileName = processTracingToolPath,
                        WorkingDirectory = AppPathService.AppDirPath,
                        Arguments = string.Join(' ', args),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                }
            
                var procTracerProc = Process.Start(processTracerStartInfo);
                if (procTracerProc != null)
                {
                    await procTracerProc.WaitForExitAsync();
                    var stdOutput = await procTracerProc.StandardOutput.ReadToEndAsync();
                    var stdErr= await procTracerProc.StandardError.ReadToEndAsync();
                    Logger.LogInformation("ProcessTracer output: {StdOutput}", stdOutput);
                    if(!string.IsNullOrEmpty(stdErr))
                    {
                        Snackbar.Add(stdErr, Severity.Error);
                        Logger.LogError("ProcessTracer error: {StdErr}", stdErr);
                        return;
                    }
                }

                IGameInstallAnalyzer gameInstallAnalyzer =
                    App.ServiceProvider.GetRequiredService<IGameInstallAnalyzer>();
                Result<string?> analyzeResult =
                    await gameInstallAnalyzer.AnalyzeFromFileAsync(tempOutputPath,
                        installFileResult.FullPath);
                string target = "";
                if (!analyzeResult.Success)
                {
                    Snackbar.Add(analyzeResult.Message, Severity.Error);
                    return;
                }
                else if (analyzeResult.Value == null)
                {
                    Snackbar.Add("Can't find the executable file of game", Severity.Error);
                    return;
                }
                else
                {
                    target = analyzeResult.Value;
                }
            
                if (string.IsNullOrEmpty(target))
                    return;
            
                if (AddNewGameEvent.HasDelegate)
                    await AddNewGameEvent.InvokeAsync(target);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to install game");
                Snackbar.Add(Resources.Message_DetectGameInstallError, Severity.Error);
            }
            finally
            {
                try
                {
                    Directory.Delete(tempPath, true);
                }
                catch
                {
                    // ignore
                }
                dialogReferenceProgress.Close();
            }
        }

        private async Task OnAddNewGame()
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".exe", ".bat" } }
            });

            var options = new PickOptions
            {
                PickerTitle = "Please select game exe file",
                FileTypes = customFileType
            };

            FileResult? result = await FilePicker.PickAsync(options);
            if (result == null)
            {
                return;
            }

            if (AddNewGameEvent.HasDelegate)
            {
                await AddNewGameEvent.InvokeAsync(result.FullPath);
            }
        }

        private async Task OnDeleteClick()
        {
            if (OnDeleteEvent.HasDelegate)
                await OnDeleteEvent.InvokeAsync();
        }

        private async Task OnKeyDown(KeyboardEventArgs key)
        {
            if (key.Key != "Enter" || (suggestions.Count > 0 && SearchAutoCompleteRef.Open))
                return;
            await OnSearch();
        }

        private async Task<IEnumerable<string>> TriggerSearchSuggestions(string searchText, CancellationToken token)
        {
            if (SearchSuggestionFunc != null)
                suggestions = await SearchSuggestionFunc(searchText ?? "", token);
            return suggestions;
        }

        private async Task OnSearch()
        {
            if (SearchEvent.HasDelegate)
            {
                await SearchEvent.InvokeAsync(new SearchParameter(SearchText, SearchFilterModel));
            }
        }

        private Task OnRefresh()
        {
            if (OnRefreshEvent.HasDelegate)
                return OnRefreshEvent.InvokeAsync();
            return Task.CompletedTask;
        }

        private async Task OnSortByChange()
        {
            if (OnSortByChangeEvent.HasDelegate)
                await OnSortByChangeEvent.InvokeAsync(SortBy);
        }

        private async Task OnAddNewGameFromArchive()
        {
            IReadOnlyList<string> supportExtensions =
                App.ServiceProvider.GetRequiredService<ExtractorFactory>().SupportedExtensions;
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, supportExtensions }
            });

            var options = new PickOptions
            {
                PickerTitle = "Please select archive file",
                FileTypes = customFileType
            };

            FileResult? targetFile = await FilePicker.PickAsync(options);
            if (targetFile == null)
            {
                return;
            }

            var dialogAddFromArchiveParameters = new DialogParameters<DialogAddFromArchive>
            {
                { x => x.SelectedFile, targetFile.FullPath }
            };

            IDialogReference dialogReference = await DialogService.ShowAsync<DialogAddFromArchive>("Add Game from Zip",
                dialogAddFromArchiveParameters,
                new DialogOptions
                {
                    FullWidth = true,
                    MaxWidth = MaxWidth.Small
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult is null or { Canceled: true })
                return;
            var extractOption = (DialogAddFromArchive.Model?)dialogResult.Data;
            if (extractOption?.GameName == null || extractOption.TargetLibrary == null)
                return;
            Logger.LogInformation("Start add game info from archive");
            string targetPath = Path.Combine(extractOption.TargetLibrary, extractOption.GameName);
            ExtractorFactory extractorFactory = App.ServiceProvider.GetRequiredService<ExtractorFactory>();
            IExtractor? extractor = extractorFactory.GetExtractor(Path.GetExtension(targetFile.FileName));
            if (extractor is null)
            {
                Logger.LogError("Extractor for {ResultFileName} not found", targetFile.FileName);
                return;
            }

            string tempPath =
                Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            Directory.CreateDirectory(tempPath);
            var dialogParameters = new DialogParameters<ProgressDialog>
            {
                { x => x.ProgressText, "0 %" },
                { x => x.IsDeterminateProgress, true }
            };
            IDialogReference dialogReferenceProgress = await DialogService.ShowAsync<ProgressDialog>("Extracting",
                dialogParameters,
                new DialogOptions
                {
                    FullWidth = true,
                    BackdropClick = false,
                    MaxWidth = MaxWidth.Small
                });
            try
            {
                Result<string> extractResult = await extractor.ExtractAsync(targetFile.FullPath, new ExtractOption
                {
                    CreateNewFolder = false,
                    Password = extractOption.ArchivePassword ?? "",
                    TargetPath = tempPath,
                    ProgressChanged = (_, progress) =>
                    {
                        var dialog = (ProgressDialog?)dialogReferenceProgress.Dialog;
                        if (dialog == null) return;
                        dialog.ProgressValue = progress;
                    }
                });
                if (!extractResult.Success)
                {
                    throw new Exception(extractResult.Message);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to extract file to temp folder");
                Snackbar.Add($"Failed to extract file to temp folder {e.Message}", Severity.Error);
                try
                {
                    Directory.Delete(tempPath, true);
                }
                catch
                {
                    // ignore
                }

                dialogReferenceProgress.Close();
                return;
            }

            Directory.CreateDirectory(targetPath);
            string startPath = tempPath;
            do
            {
                var dirInfo = new DirectoryInfo(startPath);
                int fileCount = dirInfo.EnumerateFiles().Count();
                int innerCount = dirInfo.EnumerateDirectories().Count() + fileCount;
                if (innerCount == 1 && fileCount == 0)
                {
                    startPath = dirInfo.EnumerateDirectories().First().FullName;
                }
                else
                {
                    break;
                }
            } while (true);

            try
            {
                FileHelper.CopyDirectory(startPath, targetPath);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to copy directory to target path");
                Snackbar.Add($"Failed to copy directory to target path : {e.Message}", Severity.Error);
                try
                {
                    Directory.Delete(targetPath, true);
                }
                catch
                {
                    // ignore
                }
            }

            try
            {
                Directory.Delete(tempPath, true);
            }
            catch
            {
                // ignore
            }

            Logger.LogInformation("Finish add game info from archive");
            dialogReferenceProgress.Close();
            _ = AddNewGameEvent.InvokeAsync(Path.Combine(targetPath, "Fake.exe"));
        }

        public class SearchFilter
        {
            public bool SearchName { get; set; } = true;

            public bool SearchDeveloper { get; set; } = true;

            public bool SearchExePath { get; set; } = true;

            public bool SearchTag { get; set; } = true;
        }

        public class SearchParameter(string? searchText, SearchFilter filter)
        {
            public string? SearchText { get; set; } = searchText;

            public SearchFilter SearchFilter { get; set; } = filter;
        }

        public async Task FocusSearchInputAsync()
        {
            await SearchAutoCompleteRef.FocusAsync();
        }
    }
}