namespace GameManager.DB.Models
{
    public class LaunchOption
    {
        public int Id { get; set; }

        public bool RunAsAdmin { get; set; }

        public string? LaunchWithLocaleEmulator { get; set; }
    }
}