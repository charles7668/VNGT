using GameManager.DB.Models;

namespace GameManager.Database
{
    public interface IAppSettingRepository
    {
        Task<AppSetting> GetAppSettingAsync();

        Task UpdateAppSettingAsync(AppSetting appSetting);

        Task<TextMapping?> SearchTextMappingByOriginalText(string original);
    }
}