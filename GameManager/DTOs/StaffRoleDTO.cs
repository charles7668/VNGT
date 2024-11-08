using GameManager.DB.Enums;
using GameManager.DB.Models;
using JetBrains.Annotations;

namespace GameManager.DTOs
{
    public class StaffRoleDTO : IConvertable<StaffRole>
    {
        [UsedImplicitly]
        public StaffRoleEnum Id { get; set; }

        [UsedImplicitly]
        public string RoleName { get; set; } = string.Empty;

        public StaffRole Convert()
        {
            return new StaffRole
            {
                Id = Id,
                RoleName = RoleName
            };
        }

        public static StaffRoleDTO Create(StaffRole staffStaffRole)
        {
            return new StaffRoleDTO
            {
                Id = staffStaffRole.Id,
                RoleName = staffStaffRole.RoleName
            };
        }
    }
}