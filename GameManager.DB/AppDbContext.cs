using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace GameManager.DB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(string connectString)
        {
            _connectString = connectString;
        }

        private readonly string _connectString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";

        public DbSet<GameInfo> GameInfos { get; set; }

        public DbSet<Library> Libraries { get; set; }

        public DbSet<AppSetting> AppSettings { get; set; }

        public DbSet<TextMapping> TextMapping { get; set; }


        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.EnableSensitiveDataLogging();
            options.UseSqlite(_connectString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppSetting>()
                .Property(x => x.IsAutoFetchInfoEnabled)
                .HasDefaultValue(true);
            modelBuilder.Entity<AppSetting>()
                .Property(x => x.Localization)
                .HasDefaultValue("zh-tw");

            base.OnModelCreating(modelBuilder);
        }
    }
}