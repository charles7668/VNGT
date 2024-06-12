using System.Windows.Media;

namespace VNGTTranslator.Configs
{
    public class AppConfig
    {
        public Color TranslateWindowColor { get; set; } = Color.FromArgb(128, 0, 0, 0);

        public string SourceFontFamily { get; set; } = "Arial";

        public uint SourceFontSize { get; set; } = 15;

        public Color SourceTextColor { get; set; } = Colors.White;

        public AppConfig Clone()
        {
            return new AppConfig
            {
                TranslateWindowColor = TranslateWindowColor,
                SourceFontFamily = SourceFontFamily,
                SourceFontSize = SourceFontSize,
                SourceTextColor = SourceTextColor
            };
        }

        public void Update(AppConfig appConfig)
        {
            if (appConfig == this) return;
            TranslateWindowColor = appConfig.TranslateWindowColor;
            SourceFontFamily = appConfig.SourceFontFamily;
            SourceFontSize = appConfig.SourceFontSize;
            SourceTextColor = appConfig.SourceTextColor;
        }
    }
}