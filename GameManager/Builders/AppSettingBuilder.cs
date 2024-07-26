using GameManager.DB.Models;
using GameManager.Enums;
using GameManager.Extensions;

namespace GameManager.Builders
{
    internal class AppSettingBuilder
    {
        private AppSettingBuilder()
        {
            _appSetting = new AppSetting();
        }

        private readonly AppSetting _appSetting;

        private AppSettingBuilder SetLanguage(AppLanguage lang)
        {
            _appSetting.Localization = lang.ToLanguageTag();
            return this;
        }

        public AppSetting Build()
        {
            return _appSetting;
        }

        public static AppSettingBuilder CreateDefault()
        {
            return Create()
                .SetLanguage(AppLanguage.ENGLISH);
        }

        private static AppSettingBuilder Create()
        {
            return new AppSettingBuilder();
        }
    }
}