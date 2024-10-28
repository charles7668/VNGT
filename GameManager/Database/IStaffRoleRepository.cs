using GameManager.DB.Models;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public interface IStaffRoleRepository
    {
        public Task<IEnumerable<StaffRole>> GetAsync(Expression<Func<StaffRole, bool>> queryExpression);
    }
}