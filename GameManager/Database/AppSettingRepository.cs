using GameManager.Builders;
using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
            appSetting = AppSettingBuilder.CreateDefault().Build();
            EntityEntry<AppSetting> entityEntry = dbContext.AppSettings.Add(appSetting);
            await dbContext.SaveChangesAsync();
            appSetting = entityEntry.Entity;
            entityEntry.State = EntityState.Detached;
            return appSetting;
        }

        public Task UpdateAppSettingAsync(AppSetting appSetting)
        {
            dbContext.AppSettings.Update(appSetting);
            return Task.CompletedTask;
        }

        public async Task<TextMapping?> SearchTextMappingByOriginalText(string original)
        {
            AppSetting appSetting = await GetAppSettingAsync();
            return await dbContext.TextMappings
                .Where(x => x.Original == original)
                .FirstOrDefaultAsync(x => x.AppSettingId == appSetting.Id);
        }
    }
}