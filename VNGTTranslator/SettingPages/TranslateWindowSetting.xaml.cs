using HandyControl.Controls;
using HandyControl.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VNGTTranslator.Configs;
using MessageBox = System.Windows.MessageBox;
using Window = System.Windows.Window;

namespace VNGTTranslator.SettingPages
{
    /// <summary>
    /// TranslateWindowSetting.xaml 的互動邏輯
    /// </summary>
    public partial class TranslateWindowSetting : Page
    {
        public TranslateWindowSetting()
        {
            InitializeComponent();
            _appConfigProvider = Program.ServiceProvider.GetRequiredService<IAppConfigProvider>();
        }

        private readonly IAppConfigProvider _appConfigProvider;

        private void BtnBackgroundColorSelect_OnClick(object sender, RoutedEventArgs e)
        {
            ColorPicker? picker = SingleOpenHelper.CreateControl<ColorPicker>();
            var window = new PopupWindow
            {
                PopupElement = picker,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None,
                MinWidth = 0,
                MinHeight = 0,
                Title = "Select Color",
                Owner = Window.GetWindow(this)
            };
            picker.Confirmed += (_, args) =>
            {
                AppConfig appConfig = _appConfigProvider.GetAppConfig();
                Color backupColor = appConfig.TranslateWindowColor;
                appConfig.TranslateWindowColor = args.Info;
                bool result = _appConfigProvider.TrySaveAppConfig(out string? err);
                if (!result)
                {
                    appConfig.TranslateWindowColor = backupColor;
                    MessageBox.Show(err, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                window.Close();
            };

            picker.Canceled += delegate
            {
                window.Close();
            };

            window.Show();
        }
    }
}