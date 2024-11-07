using GameManager.DB.Models;
using JetBrains.Annotations;
using System.Text.Json.Serialization;

namespace GameManager.DTOs
{
    public class AppSettingDTO : IConvertable<AppSetting>
    {
        [JsonIgnore]
        [UsedImplicitly]
        public int Id { get; set; }

        public string? LocaleEmulatorPath { get; set; }

        public string? SandboxiePath { get; set; }

        public bool IsAutoFetchInfoEnabled { get; set; } = true;

        public string? Localization { get; set; }

        [JsonIgnore]
        public DateTime UpdatedTime { get; set; }

        public List<GuideSiteDTO> GuideSites { get; set; } = [];

        public List<TextMappingDTO> TextMappings { get; set; } = [];

        [JsonIgnore]
        public string WebDAVUrl { get; set; } = "";

        [JsonIgnore]
        public string WebDAVUser { get; set; } = "";

        [JsonIgnore]
        public string WebDAVPassword { get; set; } = "";

        [JsonIgnore]
        public bool EnableSync { get; set; }

        public int SyncInterval { get; set; } = 1;

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
                TextMappings = TextMappings.Select(x => x.Convert()).ToList(),
                UpdatedTime = UpdatedTime,
                WebDAVUrl = WebDAVUrl,
                WebDAVUser = WebDAVUser,
                WebDAVPassword = WebDAVPassword,
                EnableSync = EnableSync,
                SyncInterval = SyncInterval
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
                TextMappings = appSetting.TextMappings.Select(TextMappingDTO.Create).ToList(),
                UpdatedTime = appSetting.UpdatedTime,
                WebDAVUrl = appSetting.WebDAVUrl ?? "",
                WebDAVUser = appSetting.WebDAVUser ?? "",
                WebDAVPassword = appSetting.WebDAVPassword ?? "",
                EnableSync = appSetting.EnableSync,
                SyncInterval = appSetting.SyncInterval
            };
        }
    }
}