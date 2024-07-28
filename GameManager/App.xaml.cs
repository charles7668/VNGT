using GameManager.Services;
using System.Globalization;

namespace GameManager
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public App(IConfigService configService, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            string locale = configService.GetAppSetting().Localization ?? "zh-tw";
            Thread.CurrentThread.CurrentCulture =
                new CultureInfo(locale);
            Thread.CurrentThread.CurrentUICulture =
                new CultureInfo(locale);

            InitializeComponent();

            MainPage = new MainPage();
        }
    }
}