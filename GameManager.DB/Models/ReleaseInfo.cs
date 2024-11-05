using GameManager.DB.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    [Index(nameof(GameInfoId), IsUnique = false)]
    public class ReleaseInfo
    {
        public int Id { get; set; }

        public int GameInfoId { get; set; }
        public GameInfo GameInfo { get; set; } = null!;

        [MaxLength(100)]
        public string ReleaseName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ReleaseLanguage { get; set; } = string.Empty;

        public DateTime ReleaseDate { get; set; }

        [MaxLength(100)]
        public List<PlatformEnum> Platforms { get; set; } = [];

        public int AgeRating { get; set; } = 0;

        public List<ExternalLink> ExternalLinks { get; set; } = [];
        
        [MaxLength(100)]
        public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    }
}