using GameManager.DTOs;
using GameManager.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GameManager.Components.Pages.components
{
    public partial class DialogUseLECheck
    {
        private string? LeConfig { get; set; }

        private List<string> LeConfigs { get; set; } = null!;

        [CascadingParameter]
        public MudDialogInstance? MudDialog { get; set; }

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        protected override void OnInitialized()
        {
            LeConfigs = ["None"];
            AppSettingDTO appSetting = ConfigService.GetAppSettingDTO();
            if (!string.IsNullOrEmpty(appSetting.LocaleEmulatorPath)
                && File.Exists(Path.Combine(appSetting.LocaleEmulatorPath, "LEConfig.xml")))
            {
                string configPath = Path.Combine(appSetting.LocaleEmulatorPath, "LEConfig.xml");
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

            LeConfig ??= LeConfigs[0];
            base.OnInitialized();
        }

        private void OnYes()
        {
            MudDialog?.Close(DialogResult.Ok(LeConfig));
        }

        private void OnNo()
        {
            MudDialog?.Cancel();
        }
    }
}