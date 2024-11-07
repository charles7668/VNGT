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

        public List<GuideSite> GuideSites { get; set; } = [];

        public List<TextMapping> TextMappings { get; set; } = [];

        public bool EnableSync { get; set; }

        [MaxLength(200)]
        public string? WebDAVUrl { get; set; }

        [MaxLength(100)]
        public string? WebDAVUser { get; set; }

        [MaxLength(100)]
        public string? WebDAVPassword { get; set; }

        /// <summary>
        /// sync interval in seconds , default 60 seconds , min 5 seconds
        /// </summary>
        [Range(5, int.MaxValue)]
        public int SyncInterval { get; set; } = 60;

        public DateTime UpdatedTime { get; set; } = DateTime.MinValue;
    }
}