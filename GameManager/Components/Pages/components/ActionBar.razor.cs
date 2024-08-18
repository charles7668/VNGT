﻿using GameManager.DB.Models;
using GameManager.Enums;
using GameManager.Properties;
using GameManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GameManager.Components.Pages.components
{
    public partial class ActionBar
    {
        private string? SearchText { get; set; }

        [Parameter]
        public EventCallback<string> AddNewGameEvent { get; set; }

        [Parameter]
        public EventCallback<SearchParameter> SearchEvent { get; set; }

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
            string toolPath = AppPathService.ToolsDirPath;
            string processTracingToolPath = Path.Combine(toolPath, "ProcessTracer", "ProcessTracer.exe");
            if (!File.Exists(processTracingToolPath))
            {
                Snackbar.Add("Please install ProcessTracer tool first", Severity.Warning);
                return;
            }

            AppSetting appSetting = ConfigService.GetAppSetting();
            string? lePath = appSetting.LocaleEmulatorPath;
            string leExePath = string.IsNullOrEmpty(lePath) ? "" : Path.Combine(lePath, "LEProc.exe");
            string guid = "";
            if (File.Exists(leExePath))
            {
                IDialogReference? dialogReference = await DialogService.ShowAsync<DialogUseLECheck>("Information",
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

            if (!string.IsNullOrEmpty(guid))
            {
                var leProcessStartInfo = new ProcessStartInfo
                {
                    FileName = leExePath,
                    Arguments = $"-runas \"{guid}\" \"{installFileResult.FullPath}\"",
                    UseShellExecute = false
                };
                var leProc = Process.Start(leProcessStartInfo);
                if (leProc != null)
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(500);
                    try
                    {
                        await leProc.WaitForExitAsync(cancellationTokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        // ignore
                    }

                    await Task.Delay(200, CancellationToken.None);
                }
            }
            else
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = installFileResult.FullPath
                };
                Process.Start(processStartInfo);
                await Task.Delay(100);
            }

            string tempPath =
                Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            Directory.CreateDirectory(tempPath);
            string tempConsoleOutputPath = Path.Combine(tempPath, "output.txt");
            string tempConsoleErrorPath = Path.Combine(tempPath, "error.txt");

            var processTracerStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = AppPathService.AppDirPath,
                Arguments =
                    $"/c {processTracingToolPath} --file \"{installFileResult.FullPath}\" > \"{tempConsoleOutputPath}\" 2>\"{tempConsoleErrorPath}\"",
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = true,
                CreateNoWindow = true,
                Verb = "runas"
            };
            var processTracerProc = Process.Start(processTracerStartInfo);
            if (processTracerProc == null)
            {
                Snackbar.Add("Failed to start ProcessTracer tool", Severity.Error);
                return;
            }

            await processTracerProc.WaitForExitAsync();
            string target = "";
            string errors = "";
            if (File.Exists(tempConsoleErrorPath))
            {
                errors = await File.ReadAllTextAsync(tempConsoleErrorPath);
            }

            if (!string.IsNullOrEmpty(errors))
            {
                Snackbar.Add(errors, Severity.Error);
            }
            else if (File.Exists(tempConsoleOutputPath))
            {
                HashSet<string> candidateDirs = [];
                List<string> excludePath =
                [
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                ];
                using (var sr = new StreamReader(tempConsoleOutputPath, Encoding.UTF8))
                {
                    do
                    {
                        string? line = await sr.ReadLineAsync();
                        // only find FileIOWrite event data
                        if (line == null || !line.StartsWith("[FileIOWrite]"))
                        {
                            continue;
                        }

                        int startIndex = line.LastIndexOf(", File: ", StringComparison.Ordinal);
                        string filePath = line.Substring(startIndex + 8);
                        // if the file path is in the exclude path, skip it
                        if (excludePath.Any(p => filePath.StartsWith(p)))
                        {
                            continue;
                        }

                        string? dirPath = Path.GetDirectoryName(filePath);
                        string ext = Path.GetExtension(filePath);
                        // only find for exe path
                        if (dirPath != null && ext == ".exe")
                        {
                            candidateDirs.Add(filePath);
                        }
                    } while (!sr.EndOfStream);
                }

                if (candidateDirs.Count < 1)
                {
                    Snackbar.Add("Can't find the executable file of game", Severity.Error);
                    return;
                }

                var candidates = candidateDirs.ToList();
                int minSplitCount = candidates[0].Replace('/', '\\').Split('\\').Length;
                target = candidates[0];
                for (int i = 1; i < candidates.Count; ++i)
                {
                    int splitCount = candidates[i].Replace('/', '\\').Split('\\').Length;
                    if (splitCount < minSplitCount)
                    {
                        minSplitCount = splitCount;
                        target = candidates[i];
                    }
                }
            }

            try
            {
                File.Delete(tempConsoleOutputPath);
                File.Delete(tempConsoleErrorPath);
            }
            catch (Exception)
            {
                // ignore
            }

            // if the target is empty, return
            if (string.IsNullOrEmpty(target))
                return;

            if (AddNewGameEvent.HasDelegate)
                await AddNewGameEvent.InvokeAsync(target);
        }

        private async Task OnAddNewGame()
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".exe" } }
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
            if (key.Key == "Enter")
                await OnSearch();
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
    }
}