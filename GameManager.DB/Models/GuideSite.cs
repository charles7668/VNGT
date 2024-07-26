using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class GuideSite
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(150)]
        public string? SiteUrl { get; set; }

        public int AppSettingId { get; set; }
        public AppSetting? AppSetting { get; set; }
    }
}