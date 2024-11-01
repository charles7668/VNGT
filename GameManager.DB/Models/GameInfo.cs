using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    [Index(nameof(GameUniqueId), IsUnique = true)]
    [Index(nameof(ExePath), IsUnique = false)]
    [Index(nameof(UploadTime), nameof(GameName), IsUnique = false)]
    public class GameInfo
    {
        public int Id { get; set; }

        [Description("Unique ID for each game , used to identify the game")]
        public Guid GameUniqueId { get; set; } = Guid.NewGuid();

        [Description("Fetch ID , used to fetch info from the internet")]
        [MaxLength(100)]
        public string? GameInfoFetchId { get; set; }

        [MaxLength(100)]
        public string? GameName { get; set; }

        [MaxLength(100)]
        public string? GameChineseName { get; set; }

        [MaxLength(100)]
        public string? GameEnglishName { get; set; }

        [MaxLength(100)]
        public string? Developer { get; set; }

        [MaxLength(260)]
        public string? ExePath { get; set; }

        [MaxLength(260)]
        public string? SaveFilePath { get; set; }

        [MaxLength(260)]
        public string? ExeFile { get; set; }

        [MaxLength(260)]
        public string? CoverPath { get; set; }

        [MaxLength(10000)]
        public string? Description { get; set; }

        public DateTime? DateTime { get; set; }

        public int? LaunchOptionId { get; set; }
        public LaunchOption? LaunchOption { get; set; }

        public DateTime? UploadTime { get; set; }

        public bool IsFavorite { get; set; }

        public DateTime? LastPlayed { get; set; }

        public List<Staff> Staffs { get; set; } = [];

        public List<Character> Characters { get; set; } = [];

        public List<ReleaseInfo> ReleaseInfos { get; set; } = [];

        public List<RelatedSite> RelatedSites { get; set; } = [];

        public List<Tag> Tags { get; set; } = [];

        public List<string> ScreenShots { get; set; } = [];

        [MaxLength(200)]
        [Description("Background image url")]
        public string? BackgroundImageUrl { get; set; } = string.Empty;
    }
}