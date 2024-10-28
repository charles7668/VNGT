using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    public class RelatedSite
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string? Url { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        public int GameInfoId { get; set; }
        public GameInfo GameInfo { get; set; } = null!;
    }
}