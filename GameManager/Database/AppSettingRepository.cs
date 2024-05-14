using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace GameManager.Database
{
    internal class AppSettingRepository(AppDbContext dbContext) : IAppSettingRepository
    {
        public async Task<AppSetting> GetAppSettingAsync()
        {
            AppSetting? appSetting = await dbContext.AppSettings.FirstOrDefaultAsync();
            if (appSetting != null)
                return appSetting;
            appSetting = new AppSetting();
            dbContext.AppSettings.Add(appSetting);
            await dbContext.SaveChangesAsync();

            return appSetting;
        }

        public async Task UpdateAppSettingAsync(AppSetting appSetting)
        {
            dbContext.AppSettings.Update(appSetting);
            await dbContext.SaveChangesAsync();
        }
    }
}