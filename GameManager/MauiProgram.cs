using GameManager.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace GameManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();


#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            IConfigService configService = new ConfigService();
            configService.CreateConfigFolderIfNotExistAsync();
            builder.Services.AddSingleton(configService);

            return builder.Build();
        }
    }
}