using GameManager.Services;
using System.Globalization;

namespace GameManager
{
    public partial class App : Application
    {
        public App(IConfigService configService)
        {
            string locale = configService.GetAppSetting().Localization ?? "en-US";
            Thread.CurrentThread.CurrentCulture =
                new CultureInfo(locale);
            Thread.CurrentThread.CurrentUICulture =
                new CultureInfo(locale);

            InitializeComponent();

            MainPage = new MainPage();
        }
    }
}