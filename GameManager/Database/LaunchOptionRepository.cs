using GameManager.DB;
using GameManager.DB.Models;

namespace GameManager.Database
{
    public class LaunchOptionRepository(AppDbContext context) : ILaunchOptionRepository
    {
        public Task<LaunchOption?> Delete(int id)
        {
            LaunchOption? option = context.LaunchOptions.FirstOrDefault(x => x.Id == id);
            if (option == null)
                return Task.FromResult<LaunchOption?>(null);
            context.LaunchOptions.Remove(option);
            return Task.FromResult<LaunchOption?>(option);
        }
    }
}