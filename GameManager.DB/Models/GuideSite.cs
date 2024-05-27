using Microsoft.EntityFrameworkCore;

namespace GameManager.DB.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class GuideSite
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? SiteUrl { get; set; }

        public int AppSettingId { get; set; }
        public AppSetting? AppSetting { get; set; }
    }
}