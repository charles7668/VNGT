using GameManager.DB.Models;

namespace GameManager.Database
{
    public interface IAppSettingRepository
    {
        Task<AppSetting> GetAsync();

        Task UpdateAsync(AppSetting appSetting);

        Task<TextMapping?> SearchTextMappingByOriginalText(string original);
    }
}