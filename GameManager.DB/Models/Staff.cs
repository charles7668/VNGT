using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    [Index(nameof(StaffRoleId), IsUnique = false)]
    [Index(nameof(Name), IsUnique = false)]
    [Index(nameof(Name), nameof(StaffRoleId), IsUnique = true)]
    [Index(nameof(StaffRoleId), nameof(Name), IsUnique = true)]
    public class Staff
    {
        public int Id { get; set; }

        public int StaffRoleId { get; set; }
        public StaffRole StaffRole { get; set; } = null!;

        [MaxLength(100)]
        public string Name { get; set; } = "";

        public ICollection<GameInfo> GameInfos { get; set; } = [];
    }
}