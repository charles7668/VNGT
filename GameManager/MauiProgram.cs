using GameManager.Builders;
using GameManager.Database;
using GameManager.DB;
using GameManager.Extensions;
using GameManager.Models;
using GameManager.Modules.GameInstallAnalyzer;
using GameManager.Modules.SaveDataManager;
using GameManager.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Filters;

namespace GameManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            string[] args = Environment.GetCommandLineArgs();
            var programArgBuilder = new ProgramArgBuilder();
            foreach (string arg in args)
            {
                if (arg == "--debug")
                {
                    programArgBuilder.WithDebugMode();
                }
            }

#if DEBUG
            programArgBuilder.WithDebugMode();
#endif

            ProgramArg programArg = programArgBuilder.Build();

            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();

            // GlobalConfiguration.Configuration
            //     .UseMemoryStorage();
            builder.Services.AddHangfire(config => config.UseMemoryStorage());
            builder.Services.AddHangfireServer();

            if (programArg.IsDebugMode)
            {
                builder.Services.AddBlazorWebViewDeveloperTools();
                builder.Logging.AddDebug();
            }

            builder.Services.AddSingleton<IConfigService, ConfigService>();
            builder.Services.AddSingleton<IStaffService, StaffService>();
            builder.Services.AddSingleton<ISaveDataManager, SaveDataManager>();
            builder.Services.AddSingleton<IPickFolderService, PickFolderService>();

#if DEBUG
            IAppPathService appPathService = new DebugAppPathService();
#else
            IAppPathService appPathService = new AppPathService();
#endif
            appPathService.CreateAppPath();
            builder.Services.AddSingleton(appPathService);
            builder.Services.AddDbContextFactory<AppDbContext>(optionBuilder =>
            {
                optionBuilder.UseSqlite($"Data Source={appPathService.DBFilePath}");
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddGameInfoProviders();

            builder.Services.AddScoped<IImageService, ImageService>();

            builder.Services.AddScoped<IVersionService, VersionService>();

            builder.Services.AddScoped<IGameInstallAnalyzer, ProcessTracerGameInstallAnalyzer>();

            builder.Services.AddExtractors();
            builder.Services.AddSynchronizers();

            if (programArg.IsDebugMode)
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Debug()
                    .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Components"))
                    .WriteTo.File(new CompactJsonFormatter()
                        , Path.Combine(appPathService.LogDirPath, ".log")
                        , rollingInterval: RollingInterval.Day
                        , retainedFileCountLimit: 30)
                    .CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Information()
                    .WriteTo.File(new CompactJsonFormatter()
                        , Path.Combine(appPathService.LogDirPath, ".log")
                        , rollingInterval: RollingInterval.Day
                        , retainedFileCountLimit: 30)
                    .CreateLogger();
            }

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(Log.Logger);
            });

            return builder.Build();
        }
    }
}