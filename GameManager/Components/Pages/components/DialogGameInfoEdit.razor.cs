﻿using GameManager.DTOs;
using GameManager.GameInfoProvider;
using GameManager.Properties;
using GameManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GameManager.Components.Pages.components
{
    public partial class DialogGameInfoEdit : ComponentBase, IAsyncDisposable
    {
        private CancellationTokenSource _scanningExecutionFileCts = new();

        private string _fetchProvider = "VNDB";
        private bool _isFetching;
        private bool _isSandboxieInstalled;

        private bool _isVNGTTranslatorInstalled;

        private MudAutocomplete<string> _sandboxieBoxAutoComplete = null!;
        private Task _scanningExecutionFileTask = Task.CompletedTask;
        private HashSet<string> _tagHashSet = [];
        private HashSet<string> _screenShotHashSet = [];

        [Parameter]
        public FormModel Model { get; set; } = new();

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private ILogger<DialogGameInfoEdit> Logger { get; set; } = null!;

        [Inject]
        private IImageService ImageService { get; set; } = null!;

        [Inject]
        private GameInfoProviderFactory GameInfoProviderFactory { get; set; } = null!;

        private string CoverPath => ImageService.UriResolve(Model.Cover);

        [CascadingParameter]
        public IMudDialogInstance? MudDialog { get; set; }

        private List<string> LeConfigs { get; set; } = [];

        private AppSettingDTO AppSetting { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        [Inject]
        private IAppPathService AppPathService { get; set; } = null!;

        [Inject]
        private IPickFolderService PickFolderService { get; set; } = null!;

        private List<string> ExeFiles { get; set; } = [];

        private void DatePickerTextChanged(string? value)
        {
            if (value == null || value.Length < 6)
            {
                Model.DateTime = null;
            }
            else
            {
                string[] formats = ["yyyy/MM/dd"];
                if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out DateTime validDate))
                {
                    Model.DateTime = validDate;
                }
                else
                {
                    Model.DateTime = null;
                }
            }
        }

        private async Task OnAddTagClick(MouseEventArgs obj)
        {
            var parameters = new DialogParameters<DialogMultiLineInputBox>
            {
                { x => x.HelperText, Resources.GameEditDialog_AddTagDialog_HelperText }
            };
            IDialogReference dialogReference = await DialogService.ShowAsync<DialogMultiLineInputBox>(
                Resources.GameEditDialog_AddTagDialog_Title,
                parameters,
                new DialogOptions
                {
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult is null or { Canceled: true })
                return;
            if (dialogResult.Data is not string tags)
                return;
            string[] split = tags.Split('\n');
            foreach (string s in split)
            {
                string tag = s.Trim();
                if (string.IsNullOrEmpty(tag))
                    continue;
                TryAddTag(s);
            }
        }

        private void OnCancel()
        {
            MudDialog?.Cancel();
        }

        private async Task OnInfoFetchClick()
        {
            if (_isFetching)
                return;
            if (string.IsNullOrEmpty(Model.GameName))
                return;
            _isFetching = true;
            _ = InvokeAsync(StateHasChanged);
            await Task.Run(async () =>
            {
                try
                {
                    string provider = _fetchProvider;
                    IGameInfoProvider? gameInfoProvider = GameInfoProviderFactory.GetProvider(provider);
                    if (gameInfoProvider == null)
                    {
                        Logger.LogError("Provider {Provider} not found", provider);
                        return;
                    }

                    (List<GameInfoDTO>? infoList, bool hasMore) =
                        await gameInfoProvider.FetchGameSearchListAsync(Model.GameName, 10, 1);

                    if (infoList == null || infoList.Count == 0)
                        throw new FileNotFoundException();

                    var parameters = new DialogParameters<DialogFetchSelection>
                    {
                        { x => x.DisplayInfos, infoList },
                        { x => x.HasMore, hasMore },
                        { x => x.SearchName, Model.GameName },
                        { x => x.ProviderName, gameInfoProvider.ProviderName }
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

                    GameInfoDTO? info = await gameInfoProvider.FetchGameDetailByIdAsync(gameId);
                    if (info == null)
                        return;
                    List<string> replaceList = [];
                    string[]? split = info.Developer?.Split(',');
                    if (split is { Length: > 0 })
                    {
                        foreach (string s in split)
                        {
                            TryAddTag(s);
                            TextMappingDTO? mapping = await ConfigService.SearchTextMappingByOriginalText(s);
                            string? mappingResult = mapping is { Replace: not null } ? mapping.Replace : s;
                            replaceList.Add(mappingResult);
                            TryAddTag(mappingResult);
                        }
                    }

                    info.Developer = string.Join(",", replaceList);
                    info.ExePath = Model.ExePath;
                    info.ExeFile = Model.ExeFile;
                    info.SaveFilePath = Model.SaveFilePath;
                    info.EnableSync = Model.EnableSync;
                    info.LaunchOption ??= new LaunchOptionDTO();
                    info.LaunchOption.RunAsAdmin = Model.RunAsAdmin;
                    info.LaunchOption.LaunchWithLocaleEmulator = Model.LeConfig;
                    info.LaunchOption.RunWithSandboxie = Model.RunWithSandboxie;
                    info.LaunchOption.SandboxieBoxName = Model.SandboxieBoxName;
                    info.LaunchOption.RunWithVNGTTranslator = Model.RunWithVNGTTranslator;
                    info.LaunchOption.IsVNGTTranslatorNeedAdmin = Model.IsVNGTTranslatorNeedAdmin;
                    foreach (TagDTO tag in info.Tags)
                        TryAddTag(tag.Name);
                    foreach (string screenShot in info.ScreenShots)
                    {
                        TryAddScreenShot(screenShot);
                    }

                    info.Tags = Model.Tags.Select(x => new TagDTO
                    {
                        Name = x
                    }).ToList();
                    info.ScreenShots = Model.ScreenShots;
                    DataMapService.Map(info, Model);
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
                    _isFetching = false;
                    _ = InvokeAsync(StateHasChanged);
                }
            });
        }

        private async Task OnSandboxieBoxNameAdornmentClick()
        {
            if (_sandboxieBoxAutoComplete.Open)
                await _sandboxieBoxAutoComplete.CloseMenuAsync();
        }

        private void OnSave()
        {
            MudDialog?.Close(DialogResult.Ok(Model));
        }

        private async Task OnSaveBrowseClick(MouseEventArgs obj)
        {
            await PickFolderService.PickFolderAsync(path => Model.SaveFilePath = path);
        }

        private void OnTagRemoveClick(MudChip<string> chip)
        {
            if (chip.Value == null)
                return;
            Model.Tags.Remove(chip.Value);
            _tagHashSet.Remove(chip.Value);
        }

        private async Task<IEnumerable<string>> SandboxieBoxSearchFunc(string searchText,
            CancellationToken cancellationToken)
        {
            Model.SandboxieBoxName = searchText;
            const string sandboxieIni = "C:\\Windows\\Sandboxie.ini";
            if (!File.Exists(sandboxieIni))
                return [];
            using StreamReader streamReader = new(sandboxieIni);
            const string searchField = "BoxGrouping=:";
            string fieldValue = "DefaultBox";
            while (await streamReader.ReadLineAsync(CancellationToken.None) is { } line)
            {
                if (line.StartsWith(searchField))
                {
                    fieldValue = line[searchField.Length..];
                }
            }

            var result = fieldValue.Split(',').ToList();
            return result;
        }

        private void TryAddTag(string tag)
        {
            if (_tagHashSet.Contains(tag)) return;
            Model.Tags.Add(tag);
            _tagHashSet.Add(tag);
        }

        private void TryAddScreenShot(string screenshot)
        {
            if (_screenShotHashSet.Contains(screenshot)) return;
            Model.ScreenShots.Add(screenshot);
            _screenShotHashSet.Add(screenshot);
        }

        private async Task UploadByUrl()
        {
            Debug.Assert(DialogService != null);
            IDialogReference dialogReference = await DialogService.ShowAsync<DialogImageChange>("Change Cover",
                new DialogOptions
                {
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult is null or { Canceled: true })
                return;
            string? cover = dialogResult.Data as string;
            Model.Cover = cover;
        }

        private async Task OnExecutionPathSelectClick()
        {
            await PickFolderService.PickFolderAsync(path =>
            {
                if (path == Model.ExePath)
                    return;
                Model.ExePath = path;
                Model.ExeFile = null;
                _scanningExecutionFileCts.Cancel();
                _scanningExecutionFileTask.ConfigureAwait(false).GetAwaiter().GetResult();
                _scanningExecutionFileCts = new CancellationTokenSource();
                _ = ReloadExeFilesAsync(_scanningExecutionFileCts.Token);
            });
        }

        private async Task ReloadExeFilesAsync(CancellationToken cancellationToken)
        {
            ExeFiles = ["Not Set"];
            _scanningExecutionFileTask = Task.Run(() =>
            {
                Queue<string> dirs = new();
                if (!Directory.Exists(Model.ExePath))
                    return;
                dirs.Enqueue(Model.ExePath);
                while (dirs.Count > 0 && !cancellationToken.IsCancellationRequested)
                {
                    string dir = dirs.Dequeue();
                    string[] subDirs = Directory.GetDirectories(dir);
                    foreach (string subDir in subDirs)
                        dirs.Enqueue(subDir);
                    foreach (string file in Directory.EnumerateFiles(dir, "*.exe"))
                        ExeFiles.Add(Path.GetRelativePath(Model.ExePath, Path.GetFullPath(file)));
                    foreach (string file in Directory.EnumerateFiles(dir, "*.bat"))
                        ExeFiles.Add(Path.GetRelativePath(Model.ExePath, Path.GetFullPath(file)));
                }
            }, cancellationToken).ContinueWith(_ =>
            {
                Application.Current?.Dispatcher.Dispatch(StateHasChanged);
            }, cancellationToken);
            await InvokeAsync(StateHasChanged);
        }

        public class FormModel
        {
            public string? GameName { get; set; }

            public string? GameChineseName { get; set; }

            public string? GameEnglishName { get; set; }

            public string? Vendor { get; set; }

            public string? ExePath { get; set; }

            public string? SaveFilePath { get; set; }

            public string? ExeFile { get; set; }

            public DateTime? DateTime { get; set; }

            public string? Cover { get; set; }

            public string? Description { get; set; }

            public bool RunAsAdmin { get; set; }

            public bool RunWithVNGTTranslator { get; set; }

            public bool RunWithSandboxie { get; set; }

            [Label("Box Name")]
            public string SandboxieBoxName { get; set; } = "DefaultBox";

            public bool IsVNGTTranslatorNeedAdmin { get; set; }

            public string? LeConfig { get; set; }

            public bool EnableSync { get; set; }

            public List<string> Tags { get; set; } = [];

            public List<StaffDTO> Staffs { get; set; } = [];

            public List<CharacterDTO> Characters { get; set; } = [];

            public List<ReleaseInfoDTO> ReleaseInfos { get; set; } = [];

            public List<RelatedSiteDTO> RelatedSites { get; set; } = [];

            public List<string> ScreenShots { get; set; } = [];
        }

        #region Lifecycle

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _scanningExecutionFileCts = new CancellationTokenSource();
                _ = ReloadExeFilesAsync(_scanningExecutionFileCts.Token);
                await base.OnInitializedAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to initialize : {Message}", e.Message);
                throw;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            try
            {
                await base.OnAfterRenderAsync(firstRender);
                if (!firstRender)
                {
                    return;
                }

                LeConfigs = ["None"];
                AppSetting = ConfigService.GetAppSettingDTO();
                if (!string.IsNullOrEmpty(AppSetting.LocaleEmulatorPath)
                    && File.Exists(Path.Combine(AppSetting.LocaleEmulatorPath, "LEConfig.xml")))
                {
                    string configPath = Path.Combine(AppSetting.LocaleEmulatorPath, "LEConfig.xml");
                    var xmlDoc = XDocument.Load(configPath);
                    IEnumerable<XElement> nodes = xmlDoc.XPathSelectElements("//Profiles/Profile");
                    foreach (XElement node in nodes)
                    {
                        XAttribute? attr = node.Attribute("Name");
                        if (attr == null || string.IsNullOrEmpty(attr.Value))
                            continue;
                        LeConfigs.Add(attr.Value);
                    }
                }

                Model.LeConfig ??= "None";

                _tagHashSet = Model.Tags.ToHashSet();
                _screenShotHashSet = Model.ScreenShots.ToHashSet();

                string toolsPath = AppPathService.ToolsDirPath;
                _isVNGTTranslatorInstalled =
                    File.Exists(Path.Combine(toolsPath, "VNGTTranslator", "VNGTTranslator.exe"));
                string? sandboxiePath = Path.GetDirectoryName(AppSetting.SandboxiePath);
                _isSandboxieInstalled = !string.IsNullOrEmpty(sandboxiePath) &&
                                        File.Exists(Path.Combine(sandboxiePath, "Start.exe"));
                _ = InvokeAsync(StateHasChanged);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to initialize : {Message}", e.Message);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _scanningExecutionFileCts.CancelAsync();
        }

        #endregion
    }
}