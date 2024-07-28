using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    public class LaunchOption
    {
        public int Id { get; set; }

        public bool RunAsAdmin { get; set; }

        public bool RunWithVNGTTranslator { get; set; }

        public bool IsVNGTTranslatorNeedAdmin { get; set; }

        [MaxLength(100)]
        public string? LaunchWithLocaleEmulator { get; set; }
    }
}