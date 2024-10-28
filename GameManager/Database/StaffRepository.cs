using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public class StaffRepository(AppDbContext context) : IStaffRepository
    {
        public Task<Staff?> GetAsync(Expression<Func<Staff, bool>> expression)
        {
            return context.Staffs.FirstOrDefaultAsync(expression);
        }
    }
}