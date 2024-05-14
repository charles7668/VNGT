namespace GameManager.DB.Models
{
    public class GameInfo
    {
        public int Id { get; set; }

        public string? GameInfoId { get; set; }

        public string? GameName { get; set; }

        public string? Developer { get; set; }

        public string? ExePath { get; set; }

        public string? ExeFile { get; set; }

        public string? CoverPath { get; set; }

        public string? Description { get; set; }

        public DateTime? DateTime { get; set; }

        public int? LaunchOptionId { get; set; }

        public LaunchOption? LaunchOption { get; set; }
    }
}