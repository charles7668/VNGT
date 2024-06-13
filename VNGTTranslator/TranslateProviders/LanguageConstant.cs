namespace VNGTTranslator.TranslateProviders
{
    public static class LanguageConstant
    {
        public enum Language
        {
            JAPANESE,
            ENGLISH,
            CHINESE,
            CHINESE_TRADITIONAL
        }

        public static string Japanese { get; } = "ja";
        public static string English { get; } = "en";
        public static string Chinese { get; } = "zh-CN";
        public static string ChineseTraditional { get; } = "zh-TW";

        public static string GetLanguageCode(Language language)
        {
            return language switch
            {
                Language.JAPANESE => Japanese,
                Language.ENGLISH => English,
                Language.CHINESE => Chinese,
                Language.CHINESE_TRADITIONAL => ChineseTraditional,
                _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
            };
        }
    }
}