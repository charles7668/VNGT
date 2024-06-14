namespace VNGTTranslator.TranslateProviders.SettingPages
{
    /// <summary>
    /// BaseSettingPage.xaml 的互動邏輯
    /// </summary>
    public partial class BaseSettingPage
    {
        public BaseSettingPage(ref SettingParams settingParamRef)
        {
            InitializeComponent();
            _settingParam = settingParamRef;
            DataContext = this;
        }

        private readonly SettingParams _settingParam;

        public bool IsUseProxy
        {
            get => _settingParam.IsUseProxy;
            set => _settingParam.IsUseProxy = value;
        }

        public class SettingParams
        {
            public bool IsUseProxy { get; set; }
        }
    }
}