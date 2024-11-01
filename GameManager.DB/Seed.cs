using GameManager.DB.Enums;
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
                    Id = StaffRoleEnum.STAFF,
                    RoleName = "staff"
                },
                new StaffRole
                {
                    Id = StaffRoleEnum.SCENARIO,
                    RoleName = "scenario"
                },
                new StaffRole
                {
                    Id = StaffRoleEnum.DIRECTOR,
                    RoleName = "director"
                },
                new StaffRole
                {
                    Id = StaffRoleEnum.CHARACTER_DESIGN,
                    RoleName = "character design"
                },
                new StaffRole
                {
                    Id = StaffRoleEnum.ARTIST,
                    RoleName = "artist"
                },
                new StaffRole
                {
                    Id = StaffRoleEnum.MUSIC,
                    RoleName = "music"
                },
                new StaffRole
                {
                    Id = StaffRoleEnum.SONG,
                    RoleName = "song"
                }
            ];
        }
    }
}