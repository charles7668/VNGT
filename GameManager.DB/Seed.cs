using GameManager.DB.Models;

namespace GameManager.DB
{
    public static class Seed
    {
        public static StaffRole[] SeedStaffRoles()
        {
            return
            [
                new StaffRole
                {
                    Id = 99,
                    RoleName = "staff"
                },
                new StaffRole
                {
                    Id = 1,
                    RoleName = "scenario"
                },
                new StaffRole
                {
                    Id = 2,
                    RoleName = "director"
                },
                new StaffRole
                {
                    Id = 3,
                    RoleName = "character design"
                },
                new StaffRole
                {
                    Id = 4,
                    RoleName = "artist"
                },
                new StaffRole
                {
                    Id = 5,
                    RoleName = "music"
                },
                new StaffRole
                {
                    Id = 6,
                    RoleName = "song"
                }
            ];
        }
    }
}