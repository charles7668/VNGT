using System.Windows.Media;
using VNGTTranslator.Models;
using VNGTTranslator.TranslateProviders;

namespace VNGTTranslator.Configs
{
    public class AppConfig
    {
        public uint MaxTranslateWordCount { get; set; } = 1000;

        public Color TranslateWindowColor { get; set; } = Color.FromArgb(128, 0, 0, 0);

        public DisplayTextStyle SourceTextStyle { get; set; } = new();

        public Dictionary<string, DisplayTextStyle> TranslateTextStyles { get; set; } = new();

        public HashSet<string> UsedTranslateProviderSet { get; set; } = new();

        public LanguageConstant.Language SourceLanguage { get; set; } = LanguageConstant.Language.JAPANESE;

        public LanguageConstant.Language TargetLanguage { get; set; } = LanguageConstant.Language.CHINESE;

        public uint TranslateInterval { get; set; } = 1000;

        public bool IsUseProxy { get; set; } = true;
        public bool IsUseSystemProxy { get; set; } = true;
        public string ProxyAddress { get; set; } = "";
        public string ProxyPort { get; set; } = "";

        public AppConfig Clone()
        {
            return new AppConfig
            {
                TranslateWindowColor = TranslateWindowColor,
                SourceTextStyle = SourceTextStyle,
                TranslateTextStyles = new Dictionary<string, DisplayTextStyle>(TranslateTextStyles),
                UsedTranslateProviderSet = [..UsedTranslateProviderSet],
                MaxTranslateWordCount = MaxTranslateWordCount,
                IsUseProxy = IsUseProxy,
                IsUseSystemProxy = IsUseSystemProxy,
                ProxyAddress = ProxyAddress,
                ProxyPort = ProxyPort
            };
        }

        public void Update(AppConfig appConfig)
        {
            if (appConfig == this) return;
            TranslateWindowColor = appConfig.TranslateWindowColor;
            SourceTextStyle = appConfig.SourceTextStyle;
            TranslateTextStyles = new Dictionary<string, DisplayTextStyle>(appConfig.TranslateTextStyles);
            UsedTranslateProviderSet = [..appConfig.UsedTranslateProviderSet];
            MaxTranslateWordCount = appConfig.MaxTranslateWordCount;
            IsUseProxy = appConfig.IsUseProxy;
            IsUseSystemProxy = appConfig.IsUseSystemProxy;
            ProxyAddress = appConfig.ProxyAddress;
            ProxyPort = appConfig.ProxyPort;
        }
    }
}