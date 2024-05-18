using GameManager.Database;
using GameManager.DB;
using GameManager.GameInfoProvider;
using GameManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Compact;

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
            builder.Services.AddTransient(_ =>
                new AppDbContext($"Data Source={config.GetDbPath()}"));


            builder.Services.AddScoped<IGameInfoRepository, GameInfoRepository>();
            builder.Services.AddScoped<ILibraryRepository, LibraryRepository>();
            builder.Services.AddScoped<IAppSettingRepository, AppSettingRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IHttpService, HttpService>();
            builder.Services.AddScoped<IGameInfoProvider, VndbProvider>();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.File(new CompactJsonFormatter()
                    , Path.Combine(config.GetLogPath(), ".log")
                    , rollingInterval: RollingInterval.Day
                    , retainedFileCountLimit: 30)
                .CreateLogger();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddProvider(new SerilogLoggerProvider(Log.Logger));
            });

            return builder.Build();
        }
    }
}