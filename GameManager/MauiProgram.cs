using GameManager.DB;
using GameManager.Services;
using Microsoft.EntityFrameworkCore;
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

            // Add Database
            var dbContext = new AppDbContext($"Data Source={configService.GetDbPath()}");
            dbContext.Database.EnsureCreated();
            if (dbContext.Database.GetPendingMigrations().Any())
                dbContext.Database.Migrate();
            dbContext.SaveChanges();
            builder.Services.AddSingleton(dbContext);

            return builder.Build();
        }
    }
}