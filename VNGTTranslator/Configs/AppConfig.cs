using System.Windows.Media;

namespace VNGTTranslator.Configs
{
    public class AppConfig
    {
        public Color TranslateWindowColor { get; set; } = Color.FromArgb(128, 0, 0, 0);

        public string SourceFontFamily { get; set; } = "Arial";

        public uint SourceFontSize { get; set; } = 15;

        public Color SourceTextColor { get; set; } = Colors.White;

        public bool SourceTextShadowEnabled { get; set; } = true;

        public bool TranslateTextShadowEnabled { get; set; } = true;

        public string TranslateFontFamily { get; set; } = "Arial";

        public uint TranslateFontSize { get; set; } = 15;

        public Color TranslateTextColor { get; set; } = Colors.White;

        public AppConfig Clone()
        {
            return new AppConfig
            {
                TranslateWindowColor = TranslateWindowColor,
                SourceFontFamily = SourceFontFamily,
                SourceFontSize = SourceFontSize,
                SourceTextColor = SourceTextColor,
                TranslateFontFamily = TranslateFontFamily,
                TranslateFontSize = TranslateFontSize,
                TranslateTextColor = TranslateTextColor
            };
        }

        public void Update(AppConfig appConfig)
        {
            if (appConfig == this) return;
            TranslateWindowColor = appConfig.TranslateWindowColor;
            SourceFontFamily = appConfig.SourceFontFamily;
            SourceFontSize = appConfig.SourceFontSize;
            SourceTextColor = appConfig.SourceTextColor;
            TranslateFontFamily = appConfig.TranslateFontFamily;
            TranslateFontSize = appConfig.TranslateFontSize;
            TranslateTextColor = appConfig.TranslateTextColor;
        }
    }
}