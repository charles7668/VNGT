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
                .AsNoTracking()
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

        public async Task UpdateAppSettingAsync(AppSetting appSetting)
        {
            AppSetting? entity = dbContext.AppSettings
                .Include(x => x.GuideSites)
                .Include(x => x.TextMappings)
                .FirstOrDefault();
            if (entity == null)
            {
                await dbContext.AppSettings.AddAsync(appSetting);
                return;
            }

            dbContext.Entry(entity).CurrentValues.SetValues(appSetting);
            entity.GuideSites.Clear();
            foreach (GuideSite guideSite in appSetting.GuideSites)
            {
                guideSite.AppSettingId = entity.Id;
                guideSite.Id = 0;
                entity.GuideSites.Add(guideSite);
            }

            entity.TextMappings.Clear();
            foreach (TextMapping textMapping in appSetting.TextMappings)
            {
                textMapping.AppSettingId = entity.Id;
                textMapping.Id = 0;
                entity.TextMappings.Add(textMapping);
            }
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