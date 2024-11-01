using GameManager.DB.Enums;
using GameManager.DB.Models;
using System.Linq.Expressions;

namespace GameManager.Services
{
    public interface IStaffService
    {
        public Task<IEnumerable<StaffRole>> GetStaffRolesAsync();

        public Task<Staff?> GetStaffAsync(Expression<Func<Staff, bool>> expression);

        public Task<StaffRole> GetStaffRoleAsync(StaffRoleEnum role);

        public Task<StaffRoleEnum> GetStaffRoleEnumByName(string roleName);
    }
}