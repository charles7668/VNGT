using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public List<GameInfo> GameInfos { get; set; } = [];
    }
}