using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace GameManager.DB
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<GameInfo> GameInfos { get; set; }

        public DbSet<Library> Libraries { get; set; }

        public DbSet<AppSetting> AppSettings { get; set; }

        public DbSet<TextMapping> TextMappings { get; set; }

        public DbSet<GameInfoTag> GameInfoTags { get; set; }

        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppSetting>()
                .Property(x => x.IsAutoFetchInfoEnabled)
                .HasDefaultValue(true);
            modelBuilder.Entity<AppSetting>()
                .Property(x => x.Localization)
                .HasDefaultValue("zh-tw");
            modelBuilder.Entity<GameInfo>()
                .HasMany(x => x.Tags)
                .WithMany(x => x.GameInfos)
                .UsingEntity<GameInfoTag>();
            modelBuilder.Entity<LaunchOption>()
                .Property(x => x.SandboxieBoxName)
                .HasDefaultValue("DefaultBox");

            base.OnModelCreating(modelBuilder);
        }
    }
}