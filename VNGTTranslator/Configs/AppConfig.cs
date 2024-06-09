using System.Windows.Media;

namespace VNGTTranslator.Configs
{
    public class AppConfig
    {
        public Color TranslateWindowColor = Color.FromArgb(128, 0, 0, 0);

        public string SourceFontFamily { get; set; } = "Arial";

        public uint SourceFontSize { get; set; } = 15;

        public AppConfig Clone()
        {
            return new AppConfig
            {
                TranslateWindowColor = TranslateWindowColor,
                SourceFontFamily = SourceFontFamily,
                SourceFontSize = SourceFontSize
            };
        }

        public void Update(AppConfig appConfig)
        {
            if (appConfig == this) return;
            TranslateWindowColor = appConfig.TranslateWindowColor;
            SourceFontFamily = appConfig.SourceFontFamily;
            SourceFontSize = appConfig.SourceFontSize;
        }
    }
}