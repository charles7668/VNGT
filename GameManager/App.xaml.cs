using GameManager.DB;
using GameManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace GameManager
{
    public partial class App
    {
        public App(IServiceProvider serviceProvider)
        {
            IDbContextFactory<AppDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            IAppPathService appPathService = serviceProvider.GetRequiredService<IAppPathService>();
            ILogger<App> logger = serviceProvider.GetRequiredService<ILogger<App>>();
            AppDbContext dbContext = dbContextFactory.CreateDbContext();
            dbContext.Database.ExecuteSql($"PRAGMA foreign_keys=OFF;");
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                try
                {
                    File.Copy(appPathService.DBFilePath, appPathService.DBFilePath + ".bak", true);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error while backing up database");
                    throw;
                }

                try
                {
                    dbContext.Database.Migrate();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error while migrating database");
                    if (File.Exists(appPathService.DBFilePath + ".bak"))
                        File.Copy(appPathService.DBFilePath + ".bak", appPathService.DBFilePath, true);
                    throw;
                }
            }

            dbContext.Database.EnsureCreated();
            dbContext.Database.ExecuteSql($"PRAGMA foreign_keys=ON;");

            ServiceProvider = serviceProvider;
            IConfigService configService = serviceProvider.GetRequiredService<IConfigService>();
            string locale = configService.GetAppSetting().Localization ?? "zh-tw";
            Thread.CurrentThread.CurrentCulture =
                new CultureInfo(locale);
            Thread.CurrentThread.CurrentUICulture =
                new CultureInfo(locale);

            InitializeComponent();

            MainPage = new MainPage();
        }

        public static IServiceProvider ServiceProvider { get; private set; } = null!;
    }
}