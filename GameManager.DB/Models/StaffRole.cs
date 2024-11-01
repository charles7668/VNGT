using GameManager.DB.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    [Index(nameof(RoleName), IsUnique = true)]
    public class StaffRole
    {
        [Key]
        public StaffRoleEnum Id { get; set; }

        [MaxLength(100)]
        public string RoleName { get; set; } = string.Empty;
    }
}