﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameManager.DB.Models
{
    [Index(nameof(ExePath), IsUnique = true)]
    [Index(nameof(UploadTime), nameof(GameName), IsUnique = false)]
    public class GameInfo
    {
        public int Id { get; set; }

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