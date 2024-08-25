using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    public class AppSetting
    {
        public int Id { get; set; }

        [MaxLength(260)]
        public string? LocaleEmulatorPath { get; set; }

        [MaxLength(260)]
        public string? SandboxiePath { get; set; }

        public bool IsAutoFetchInfoEnabled { get; set; } = true;

        [MaxLength(10)]
        public string? Localization { get; set; }

        public IList<GuideSite> GuideSites { get; set; } = [];

        public IList<TextMapping> TextMappings { get; set; } = [];
    }
}