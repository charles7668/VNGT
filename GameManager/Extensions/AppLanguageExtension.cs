using GameManager.Enums;

namespace GameManager.Extensions
{
    internal static class AppLanguageExtension
    {
        internal static string ToLanguageTag(this AppLanguage appLanguage)
        {
            return appLanguage switch
            {
                AppLanguage.ENGLISH => "en-US",
                AppLanguage.CHINESE_SIMPLIFIED => "zh-cn",
                AppLanguage.CHINESE_TRADITIONAL => "zh-tw",
                _ => throw new NotImplementedException()
            };
        }
    }
}