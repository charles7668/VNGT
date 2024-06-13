using System.Windows.Media;

namespace VNGTTranslator.Models
{
    public struct DisplayTextStyle
    {
        public DisplayTextStyle()
        {
        }

        public string FontFamily { get; set; } = "Arial";
        public uint FontSize { get; set; } = 15;
        public Color TextColor { get; set; } = Colors.White;
        public bool IsTextShadowEnabled { get; set; } = true;
    }
}