using GameManager.DB.Enums;
using GameManager.DB.Models;
using GameManager.DTOs;
using System.Linq.Expressions;

namespace GameManager.Services
{
    public interface IStaffService
    {
        public Task<IEnumerable<StaffRoleDTO>> GetStaffRolesAsync();

        public Task<StaffDTO?> GetStaffAsync(Expression<Func<Staff, bool>> expression);

        public Task<StaffRoleDTO> GetStaffRoleAsync(StaffRoleEnum role);

        public Task<StaffRoleEnum> GetStaffRoleEnumByName(string roleName);
    }
}