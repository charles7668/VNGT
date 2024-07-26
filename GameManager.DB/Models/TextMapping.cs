using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    [Index(nameof(Original), IsUnique = true)]
    public class TextMapping
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Original { get; set; }

        [MaxLength(100)]
        public string? Replace { get; set; }

        public int AppSettingId { get; set; }
        public AppSetting? AppSetting { get; set; }
    }
}