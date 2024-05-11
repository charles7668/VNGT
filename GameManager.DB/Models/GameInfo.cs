﻿namespace GameManager.DB.Models
{
    public class GameInfo
    {
        public int Id { get; set; }

        public string? GameName { get; set; }

        public string? Vendor { get; set; }

        public string? ExePath { get; set; }

        public string? CoverPath { get; set; }

        public string? Description { get; set; }

        public DateTime? DateTime { get; set; }
    }
}