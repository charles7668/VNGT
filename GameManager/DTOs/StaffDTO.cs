using GameManager.DB.Models;
using JetBrains.Annotations;

namespace GameManager.DTOs
{
    public class StaffDTO : IConvertable<Staff>
    {
        [UsedImplicitly]
        public int Id { get; set; }

        [UsedImplicitly]
        public StaffRoleDTO StaffRole { get; set; } = null!;

        [UsedImplicitly]
        public string Name { get; set; } = "";

        public static StaffDTO Create(Staff staff)
        {
            return new StaffDTO
            {
                Id = staff.Id,
                StaffRole = StaffRoleDTO.Create(staff.StaffRole),
                Name = staff.Name
            };
        }

        public Staff Convert()
        {
            return new Staff
            {
                Id = Id,
                StaffRole = StaffRole.Convert(),
                Name = Name
            };
        }
    }
}