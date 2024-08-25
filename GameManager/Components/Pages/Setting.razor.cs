using GameManager.Components.Pages.components;
using GameManager.DB.Models;
using GameManager.Properties;
using GameManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Win32;
using MudBlazor;

namespace GameManager.Components.Pages
{
    public partial class Setting
    {
        private int _selectedRowNumber = -1;

        private int _selectedTextMappingRowNumber = -1;

        private AppSetting AppSetting { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        private MudTable<GuideSite> GuideSiteTable { get; set; } = null!;

        private MudTable<TextMapping> TextMappingTable { get; set; } = null!;

        private List<GuideSite> GuideSites { get; set; } = null!;

        private List<TextMapping> TextMappings { get; set; } = null!;

        protected override void OnInitialized()
        {
            AppSetting = ConfigService.GetAppSetting();
            GuideSites = AppSetting.GuideSites?.ToList() ?? new List<GuideSite>();
            TextMappings = AppSetting.TextMappings?.ToList() ?? new List<TextMapping>();
            base.OnInitialized();
        }

        private async Task UpdateSetting()
        {
            try
            {
                AppSetting.GuideSites = GuideSites;
                AppSetting.TextMappings = TextMappings;
                await ConfigService.UpdateAppSettingAsync(AppSetting);
            }
            catch (Exception e)
            {
                await DialogService.ShowMessageBox("Error", e.Message);
                return;
            }

            await DialogService.ShowMessageBox("Complete", Resources.Message_UpdateSuccess);
        }

        private Task ScanLocaleEmulator()
        {
            RegistryKey key = Registry.ClassesRoot;
            const string subKeyPath = @"CLSID\{C52B9871-E5E9-41FD-B84D-C5ACADBEC7AE}\InprocServer32";
            RegistryKey? subKey = key.OpenSubKey(subKeyPath);

            if (subKey?.GetValue("CodeBase") is not string codeBase)
            {
                return Task.CompletedTask;
            }

            var uri = new Uri(codeBase);
            string? lePath = Path.GetDirectoryName(uri.LocalPath);
            if (lePath == null)
            {
                return Task.CompletedTask;
            }

            AppSetting.LocaleEmulatorPath = lePath;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task ScanSandboxie()
        {
            RegistryKey key = Registry.LocalMachine;
            const string subKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Sandboxie-Plus_is1";
            RegistryKey? subKey = key.OpenSubKey(subKeyPath);
            if (subKey?.GetValue("InstallLocation") is not string installLocation)
            {
                return Task.CompletedTask;
            }

            string startFilePath = Path.Combine(installLocation, "Start.exe");
            if (!File.Exists(startFilePath))
            {
                return Task.CompletedTask;
            }

            AppSetting.SandboxiePath = startFilePath;
            return Task.CompletedTask;
        }

        private async Task OnAddGuideSiteClick()
        {
            List<DialogInputBox.InputModel> inputModel =
            [
                new DialogInputBox.InputModel
                {
                    Label = "Name",
                    Validate = s =>
                    {
                        if (string.IsNullOrEmpty(s))
                        {
                            return "Name is required";
                        }

                        if (AppSetting.GuideSites != null && AppSetting.GuideSites.Any(x => x.Name == s))
                            return "Name is exist";
                        return null;
                    }
                },
                new DialogInputBox.InputModel
                {
                    Label = "Site URL",
                    Validate = s => string.IsNullOrEmpty(s) ? "URL is required" : null
                }
            ];

            DialogParameters<DialogInputBox> parameters = new()
            {
                { x => x.Inputs, inputModel }
            };
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogInputBox>("Add Guide Site",
                parameters, new DialogOptions
                {
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference?.Result;
            if (dialogResult == null || dialogResult.Canceled)
            {
                return;
            }

            GuideSites ??= [];
            var guideSite = new GuideSite
            {
                Name = inputModel[0].Value,
                SiteUrl = inputModel[1].Value
            };
            GuideSites.Add(guideSite);
        }

        private Task OnDeleteGuideSiteClick()
        {
            if (_selectedRowNumber == -1)
                return Task.CompletedTask;
            GuideSites.RemoveAt(_selectedRowNumber);
            _selectedRowNumber = -1;
            return Task.CompletedTask;
        }

        private string OnSelectedRowClassFunc(GuideSite guideSite, int rowNumber)
        {
            if (_selectedRowNumber == rowNumber)
            {
                _selectedRowNumber = -1;
                return string.Empty;
            }

            if (GuideSiteTable.SelectedItem != null
                && GuideSiteTable.SelectedItem.Equals(guideSite))
            {
                _selectedRowNumber = rowNumber;
                return "selected";
            }

            return string.Empty;
        }

        private async Task OnAddTextMappingClick()
        {
            List<DialogInputBox.InputModel> inputModel =
            [
                new DialogInputBox.InputModel
                {
                    Label = "Original",
                    Validate = s =>
                    {
                        if (string.IsNullOrEmpty(s))
                        {
                            return "Original is required";
                        }

                        if (AppSetting.GuideSites != null && AppSetting.GuideSites.Any(x => x.Name == s))
                            return "Original is exist";
                        return null;
                    }
                },
                new DialogInputBox.InputModel
                {
                    Label = "Replace",
                    Validate = s => string.IsNullOrEmpty(s) ? "Replace is required" : null
                }
            ];

            DialogParameters<DialogInputBox> parameters = new()
            {
                { x => x.Inputs, inputModel }
            };
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogInputBox>("Add Text Mapping",
                parameters, new DialogOptions
                {
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference?.Result;
            if (dialogResult == null || dialogResult.Canceled)
            {
                return;
            }

            TextMappings ??= [];
            var textMapping = new TextMapping
            {
                Original = inputModel[0].Value,
                Replace = inputModel[1].Value
            };
            TextMappings.Add(textMapping);
        }

        private Task OnDeleteTextMappingClick()
        {
            if (_selectedTextMappingRowNumber == -1)
                return Task.CompletedTask;
            TextMappings.RemoveAt(_selectedTextMappingRowNumber);
            _selectedTextMappingRowNumber = -1;
            return Task.CompletedTask;
        }

        private string OnTextMappingsSelectedRowClassFunc(TextMapping textMapping, int rowNumber)
        {
            if (_selectedTextMappingRowNumber == rowNumber)
            {
                _selectedTextMappingRowNumber = -1;
                return string.Empty;
            }

            if (TextMappingTable.SelectedItem != null
                && TextMappingTable.SelectedItem.Equals(textMapping))
            {
                _selectedTextMappingRowNumber = rowNumber;
                return "selected";
            }

            return string.Empty;
        }
    }
}