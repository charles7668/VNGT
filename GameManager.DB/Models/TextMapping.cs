using Microsoft.EntityFrameworkCore;

namespace GameManager.DB.Models
{
    [Index(nameof(Original), IsUnique = true)]
    public class TextMapping
    {
        public int Id { get; set; }

        public string? Original { get; set; }

        public string? Replace { get; set; }

        public int AppSettingId { get; set; }
        public AppSetting? AppSetting { get; set; }
    }
}