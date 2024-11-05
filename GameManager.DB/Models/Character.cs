using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    [Index(nameof(Name), IsUnique = false)]
    [Index(nameof(OriginalName), IsUnique = false)]
    public class Character
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? OriginalName { get; set; }

        public List<string> Alias { get; set; } = [];

        [MaxLength(100000)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? ImageUrl { get; set; }

        [MaxLength(100)]
        public string? Age { get; set; }

        [MaxLength(100)]
        public string? Sex { get; set; }

        [MaxLength(100)]
        public string? Birthday { get; set; }

        [MaxLength(100)]
        public string? BloodType { get; set; }

        public int GameInfoId { get; set; }
        public GameInfo GameInfo { get; set; } = null!;

        [MaxLength(100)]
        public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    }
}