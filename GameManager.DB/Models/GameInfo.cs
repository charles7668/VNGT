using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    [Index(nameof(GameUniqeId), IsUnique = true)]
    [Index(nameof(ExePath), IsUnique = false)]
    [Index(nameof(UploadTime), nameof(GameName), IsUnique = false)]
    public class GameInfo
    {
        public int Id { get; set; }

        [Description("Unique ID for each game , used to identify the game")]
        public Guid GameUniqeId { get; set; }

        [MaxLength(100)]
        public string? GameInfoId { get; set; }

        [MaxLength(100)]
        public string? GameName { get; set; }

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

        public List<Tag> Tags { get; set; } = [];
    }
}