namespace GameManager.DB.Models
{
    public class AppSetting
    {
        public int Id { get; set; }

        public string? LocaleEmulatorPath { get; set; }

        public bool IsAutoFetchInfoEnabled { get; set; } = true;

        public string? Localization { get; set; }

        public IList<GuideSite> GuideSites { get; set; }
    }
}