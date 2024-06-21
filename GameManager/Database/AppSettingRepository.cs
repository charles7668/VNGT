using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace GameManager.Database
{
    internal class AppSettingRepository(AppDbContext dbContext) : IAppSettingRepository
    {
        public async Task<AppSetting> GetAppSettingAsync()
        {
            AppSetting? appSetting = await dbContext.AppSettings
                .Include(x => x.GuideSites)
                .Include(x => x.TextMappings)
                .FirstOrDefaultAsync();
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

        public async Task<TextMapping?> SearchTextMappingByOriginalText(string original)
        {
            AppSetting appSetting = await GetAppSettingAsync();
            return await dbContext.TextMapping.Where(x => x.Original == original)
                .FirstOrDefaultAsync(x => x.AppSettingId == appSetting.Id);
        }
    }
}