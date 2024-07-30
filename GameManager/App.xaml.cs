using GameManager.DB;
using GameManager.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GameManager
{
    public partial class App
    {
        public App(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            string locale = serviceProvider.GetRequiredService<IConfigService>()
                .GetAppSetting().Localization ?? "zh-tw";
            Thread.CurrentThread.CurrentCulture =
                new CultureInfo(locale);
            Thread.CurrentThread.CurrentUICulture =
                new CultureInfo(locale);

            IDbContextFactory<AppDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            AppDbContext dbContext = dbContextFactory.CreateDbContext();
            dbContext.Database.ExecuteSql($"PRAGMA foreign_keys=OFF;");
            if (dbContext.Database.GetPendingMigrations().Any())
                dbContext.Database.Migrate();
            dbContext.Database.EnsureCreated();
            dbContext.Database.ExecuteSql($"PRAGMA foreign_keys=ON;");


            InitializeComponent();

            MainPage = new MainPage();
        }

        public static IServiceProvider ServiceProvider { get; private set; } = null!;
    }
}