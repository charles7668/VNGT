﻿using GameManager.DB.Models;
using GameManager.GameInfoProvider;
using GameManager.Properties;
using GameManager.Services;
using Helper.Image;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace GameManager.Components.Pages.components
{
    public partial class DialogGameInfoEdit : ComponentBase
    {
        private HashSet<string> _tagHashSet = [];

        [Parameter]
        public FormModel Model { get; set; } = new();

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private ILogger<DialogGameInfoEdit> Logger { get; set; } = null!;

        [Inject]
        private GameInfoProviderFactory GameInfoProviderFactory { get; set; } = null!;

        private string CoverPath => string.IsNullOrEmpty(Model.Cover)
            ? "/images/no-image.webp"
            : ImageHelper.GetDisplayUrl(Model.Cover);

        [CascadingParameter]
        public MudDialogInstance? MudDialog { get; set; }

        private List<string> LeConfigs { get; set; } = [];

        private AppSetting AppSetting { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

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
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogMultiLineInputBox>("Add Tags",
                new DialogOptions
                {
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult.Canceled)
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
            if (string.IsNullOrEmpty(Model.GameName))
                return;
            try
            {
                IGameInfoProvider? gameInfoProvider = GameInfoProviderFactory.GetProvider("VNDB");
                if (gameInfoProvider == null)
                {
                    throw new ArgumentException("game info provider not found : vndb");
                }

                (List<GameInfo>? infoList, bool hasMore) =
                    await gameInfoProvider.FetchGameSearchListAsync(Model.GameName, 10, 1);
                if (infoList == null || infoList.Count == 0)
                {
                    await DialogService.ShowMessageBox("Error", Resources.Message_RelatedGameNotFound,
                        Resources.Dialog_Button_Cancel);
                    return;
                }

                string gameId = infoList[0].GameInfoId ?? "";
                if (infoList.Count > 1)
                {
                    var parameters = new DialogParameters<DialogFetchSelection>
                    {
                        { x => x.DisplayInfos, infoList },
                        { x => x.HasMore, hasMore },
                        { x => x.SearchName, Model.GameName }
                    };
                    IDialogReference? dialogReference = await DialogService.ShowAsync<DialogFetchSelection>("",
                        parameters,
                        new DialogOptions
                        {
                            BackdropClick = false
                        });
                    DialogResult? dialogResult = await dialogReference.Result;
                    if (dialogResult.Canceled)
                        return;
                    gameId = dialogResult.Data as string ?? "";
                }

                GameInfo? info = await gameInfoProvider.FetchGameDetailByIdAsync(gameId);
                if (info == null)
                    return;
                List<string> replaceList = [];
                string[]? split = info.Developer?.Split(',');
                if (split is { Length: > 0 })
                {
                    foreach (string s in split)
                    {
                        TryAddTag(s);
                        TextMapping? mapping = await ConfigService.SearchTextMappingByOriginalText(s);
                        string? mappingResult = mapping is { Replace: not null } ? mapping.Replace : s;
                        replaceList.Add(mappingResult);
                        TryAddTag(mappingResult);
                    }
                }

                info.Developer = string.Join(",", replaceList);
                info.ExePath = Model.ExePath;
                info.ExeFile = Model.ExeFile;
                info.LaunchOption ??= new LaunchOption();
                info.LaunchOption.RunAsAdmin = Model.RunAsAdmin;
                info.LaunchOption.LaunchWithLocaleEmulator = Model.LeConfig;
                foreach (Tag tag in info.Tags)
                    TryAddTag(tag.Name);
                DataMapService.Map(info, Model);
                StateHasChanged();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to fetch game info {Exception}", e.ToString());
                await DialogService.ShowMessageBox("Error", e.Message, Resources.Dialog_Button_Cancel);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            LeConfigs = ["None"];
            AppSetting =
                AppSetting = ConfigService.GetAppSetting();
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

            ExeFiles = ["Not Set"];
            if (Directory.Exists(Model.ExePath))
            {
                string[] files = Directory.GetFiles(Model.ExePath, "*.exe", SearchOption.AllDirectories);
                foreach (string file in files)
                    ExeFiles.Add(Path.GetRelativePath(Model.ExePath, Path.GetFullPath(file)));
            }

            _tagHashSet = Model.Tags.ToHashSet();

            await base.OnInitializedAsync();
        }

        private void OnSave()
        {
            MudDialog?.Close(DialogResult.Ok(Model));
        }

        private async Task OnSaveBrowseClick(MouseEventArgs obj)
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            IElementHandler? elementHandler = Application.Current?.Windows[0].Handler;
            IntPtr? handle = ((MauiWinUIWindow?)elementHandler?.PlatformView)?.WindowHandle;
            if (handle != null)
            {
                InitializeWithWindow.Initialize(folderPicker, (IntPtr)handle);
                StorageFolder? result = await folderPicker.PickSingleFolderAsync();
                if (result == null)
                    return;
                Model.SaveFilePath = result.Path;
            }
        }

        private void OnTagRemoveClick(MudChip<string> chip)
        {
            if (chip.Value == null)
                return;
            Model.Tags.Remove(chip.Value);
            _tagHashSet.Remove(chip.Value);
        }

        private void TryAddTag(string tag)
        {
            if (_tagHashSet.Contains(tag)) return;
            Model.Tags.Add(tag);
            _tagHashSet.Add(tag);
        }

        private async Task UploadByUrl()
        {
            Debug.Assert(DialogService != null);
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogImageChange>("Change Cover",
                new DialogOptions
                {
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult.Canceled)
                return;
            string? cover = dialogResult.Data as string;
            Model.Cover = cover;
        }

        public class FormModel
        {
            public string? GameName { get; set; }

            public string? Vendor { get; set; }

            public string? ExePath { get; set; }

            public string? SaveFilePath { get; set; }

            public string? ExeFile { get; set; }

            public DateTime? DateTime { get; set; }

            public string? Cover { get; set; }

            public string? Description { get; set; }

            public bool RunAsAdmin { get; set; }

            public bool RunWithVNGTTranslator { get; set; }

            public bool IsVNGTTranslatorNeedAdmin { get; set; }

            public string? LeConfig { get; set; }

            public List<string> Tags { get; set; } = [];
        }
    }
}