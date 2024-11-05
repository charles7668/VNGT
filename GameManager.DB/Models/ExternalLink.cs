using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    public class ExternalLink
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string? Url { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Label { get; set; }
        
        public int ReleaseInfoId { get; set; }
        public ReleaseInfo ReleaseInfo { get; set; } = null!;

        [MaxLength(100)]
        public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    }
}