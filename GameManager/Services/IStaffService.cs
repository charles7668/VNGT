using GameManager.DB.Models;
using GameManager.Enums;
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