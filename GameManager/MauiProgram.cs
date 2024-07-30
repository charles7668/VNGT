using GameManager.Database;
using GameManager.DB;
using GameManager.Extensions;
using GameManager.GameInfoProvider;
using GameManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Serilog;
using Serilog.Filters;
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

#if DEBUG
            IAppPathService appPathService = new DebugAppPathService();
#else
            IAppPathService appPathService = new AppPathService();
#endif
            appPathService.CreateAppPath();
            builder.Services.AddSingleton(appPathService);
            var dbContext = new AppDbContext($"Data Source={appPathService.DBFilePath}");
            dbContext.Database.ExecuteSql($"PRAGMA foreign_keys=OFF;");
            if (dbContext.Database.GetPendingMigrations().Any())
                dbContext.Database.Migrate();
            dbContext.Database.EnsureCreated();
            dbContext.Database.ExecuteSql($"PRAGMA foreign_keys=ON;");
            builder.Services.AddTransient(_ =>
                new AppDbContext($"Data Source={appPathService.DBFilePath}"));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IGameInfoProvider, VndbProvider>();
            builder.Services.AddScoped<GameInfoProviderFactory, GameInfoProviderFactory>();

            builder.Services.AddExtractors();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
#if DEBUG
                .MinimumLevel.Debug()
                .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Components"))
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.File(new CompactJsonFormatter()
                    , Path.Combine(appPathService.LogDirPath, ".log")
                    , rollingInterval: RollingInterval.Day
                    , retainedFileCountLimit: 30)
                .CreateLogger();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(Log.Logger);
            });

            return builder.Build();
        }
    }
}