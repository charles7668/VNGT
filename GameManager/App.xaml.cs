using GameManager.DB;
using GameManager.DB.Models;
using GameManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
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
            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
            if (pendingMigrations.Any())
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
                    IMigrator migrator = dbContext.Database.GetService<IMigrator>();
                    foreach (string migration in pendingMigrations)
                    {
                        migrator.Migrate(migration);
                        if (migration == "20241025035509_AddGameUniqueID")
                        {
                            foreach (GameInfo item in dbContext.GameInfos.Where(e => true))
                            {
                                item.GameUniqeId = Guid.NewGuid();
                            }

                            dbContext.SaveChanges();
                        }

                        logger.LogInformation("Applying migration {Migration}", migration);
                    }
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