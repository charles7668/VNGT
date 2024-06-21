using GameManager.DB.Models;
using GameManager.GameInfoProvider;
using GameManager.Properties;
using GameManager.Services;
using Helper.Image;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GameManager.Components.Pages.components
{
    public partial class DialogGameInfoEdit
    {
        [Parameter]
        public FormModel Model { get; set; } = new();

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private IGameInfoProvider GameInfoProvider { get; set; } = null!;

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

            await base.OnInitializedAsync();
        }

        private void OnCancel()
        {
            MudDialog?.Cancel();
        }

        private void OnSave()
        {
            MudDialog?.Close(DialogResult.Ok(Model));
        }

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

        private async Task OnInfoFetchClick()
        {
            if (string.IsNullOrEmpty(Model.GameName))
                return;
            try
            {
                (List<GameInfo>? infoList, bool hasMore) =
                    await GameInfoProvider.FetchGameSearchListAsync(Model.GameName, 10, 1);
                if (infoList == null || infoList.Count == 0)
                {
                    await DialogService.ShowMessageBox("Error", Resources.Message_RelatedGameNotFound,
                        @Resources.Dialog_Button_Cancel);
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

                GameInfo? info = await GameInfoProvider.FetchGameDetailByIdAsync(gameId);
                if (info == null)
                    return;
                List<string> replaceList = [];
                string[]? split = info.Developer?.Split(',');
                if (split is { Length: > 0 })
                {
                    foreach (string s in split)
                    {
                        TextMapping? mapping = await ConfigService.SearchTextMappingByOriginalText(s);
                        replaceList.Add(mapping is { Replace: not null } ? mapping.Replace : s);
                    }
                }

                info.Developer = string.Join(",", replaceList);

                info.ExePath = Model.ExePath;
                info.ExeFile = Model.ExeFile;
                info.LaunchOption ??= new LaunchOption();
                info.LaunchOption.RunAsAdmin = Model.RunAsAdmin;
                info.LaunchOption.LaunchWithLocaleEmulator = Model.LeConfig;
                DataMapService.Map(info, Model);
                StateHasChanged();
            }
            catch (Exception e)
            {
                await DialogService.ShowMessageBox("Error", e.Message, @Resources.Dialog_Button_Cancel);
            }
        }

        public class FormModel
        {
            public string? GameName { get; set; }

            public string? Vendor { get; set; }

            public string? ExePath { get; set; }

            public string? ExeFile { get; set; }

            public DateTime? DateTime { get; set; }

            public string? Cover { get; set; }

            public string? Description { get; set; }

            public bool RunAsAdmin { get; set; }

            public string? LeConfig { get; set; }
        }
    }
}