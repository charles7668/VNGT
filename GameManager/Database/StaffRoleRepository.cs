using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public class StaffRoleRepository(AppDbContext context) : IStaffRoleRepository
    {
        async Task<IEnumerable<StaffRole>> IStaffRoleRepository.GetAsync(
            Expression<Func<StaffRole, bool>> queryExpression)
        {
            List<StaffRole> list = await context.StaffRoles.Where(queryExpression).ToListAsync();
            return list;
        }
    }
}