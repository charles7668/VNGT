using GameManager.Database;
using GameManager.DB;
using GameManager.GameInfoProvider;
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

            builder.Services.AddSingleton<IConfigService, ConfigService>();

            // just for get db path
            IConfigService config = new ConfigService();
            var dbContext = new AppDbContext($"Data Source={config.GetDbPath()}");
            dbContext.Database.ExecuteSql($"PRAGMA foreign_keys=OFF;");
            if (dbContext.Database.GetPendingMigrations().Any())
                dbContext.Database.Migrate();
            dbContext.Database.EnsureCreated();
            dbContext.Database.ExecuteSql($"PRAGMA foreign_keys=ON;");
            builder.Services.AddSingleton(dbContext);


            builder.Services.AddScoped<IGameInfoRepository, GameInfoRepository>();
            builder.Services.AddScoped<ILibraryRepository, LibraryRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IHttpService, HttpService>();
            builder.Services.AddScoped<IProvider, VndbProvider>();

            return builder.Build();
        }
    }
}