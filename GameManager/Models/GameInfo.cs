namespace GameManager.Models
{
    public class GameInfo
    {
        public string GameName { get; set; } = string.Empty;

        public string ExePath { get; set; } = string.Empty;

        public bool Display { get; set; } = true;

        public string CoverPath { get; set; } = string.Empty;
    }
}