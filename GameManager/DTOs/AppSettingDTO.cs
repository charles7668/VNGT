using GameManager.DB.Models;

namespace GameManager.DTOs
{
    public class AppSettingDTO : IConvertable<AppSetting>
    {
        public int Id { get; set; }

        public string? LocaleEmulatorPath { get; set; }

        public string? SandboxiePath { get; set; }

        public bool IsAutoFetchInfoEnabled { get; set; } = true;

        public string? Localization { get; set; }

        public List<GuideSiteDTO> GuideSites { get; set; } = [];

        public List<TextMappingDTO> TextMappings { get; set; } = [];

        public AppSetting Convert()
        {
            return new AppSetting
            {
                Id = Id,
                LocaleEmulatorPath = LocaleEmulatorPath,
                SandboxiePath = SandboxiePath,
                IsAutoFetchInfoEnabled = IsAutoFetchInfoEnabled,
                Localization = Localization,
                GuideSites = GuideSites.Select(x => x.Convert()).ToList(),
                TextMappings = TextMappings.Select(x => x.Convert()).ToList()
            };
        }

        public static AppSettingDTO Create(AppSetting appSetting)
        {
            return new AppSettingDTO
            {
                Id = appSetting.Id,
                LocaleEmulatorPath = appSetting.LocaleEmulatorPath,
                SandboxiePath = appSetting.SandboxiePath,
                IsAutoFetchInfoEnabled = appSetting.IsAutoFetchInfoEnabled,
                Localization = appSetting.Localization,
                GuideSites = appSetting.GuideSites.Select(GuideSiteDTO.Create).ToList(),
                TextMappings = appSetting.TextMappings.Select(TextMappingDTO.Create).ToList()
            };
        }
    }
}