using GameManager.DB.Models;

namespace GameManager.Database
{
    public interface ILaunchOptionRepository
    {
        public Task<LaunchOption?> Delete(int id);
    }
}