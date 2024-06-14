using System.Windows.Media;
using VNGTTranslator.Models;

namespace VNGTTranslator.Configs
{
    public class AppConfig
    {
        public uint MaxTranslateWordCount { get; set; } = 1000;

        public Color TranslateWindowColor { get; set; } = Color.FromArgb(128, 0, 0, 0);

        public DisplayTextStyle SourceTextStyle { get; set; } = new();

        public Dictionary<string, DisplayTextStyle> TranslateTextStyles { get; set; } = new();

        public HashSet<string> UsedTranslateProviderSet { get; set; } = new();

        public AppConfig Clone()
        {
            return new AppConfig
            {
                TranslateWindowColor = TranslateWindowColor,
                SourceTextStyle = SourceTextStyle,
                TranslateTextStyles = new Dictionary<string, DisplayTextStyle>(TranslateTextStyles),
                UsedTranslateProviderSet = [..UsedTranslateProviderSet],
                MaxTranslateWordCount = MaxTranslateWordCount
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
        }
    }
}