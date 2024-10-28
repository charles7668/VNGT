using GameManager.DB.Models;
using System.Linq.Expressions;

namespace GameManager.Database
{
    public interface IStaffRepository
    {
        Task<Staff?> GetAsync(Expression<Func<Staff, bool>> expression);
    }
}