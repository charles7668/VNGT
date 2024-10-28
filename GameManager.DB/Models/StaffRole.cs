using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    [Index(nameof(RoleName), IsUnique = true)]
    public class StaffRole
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string RoleName { get; set; } = string.Empty;
    }
}