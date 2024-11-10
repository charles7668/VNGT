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

        public DbSet<GameInfoStaff> GameInfoStaffs { get; set; }

        public DbSet<GameInfoTag> GameInfoTags { get; set; }

        public DbSet<LaunchOption> LaunchOptions { get; set; }

        public DbSet<Staff> Staffs { get; set; }

        public DbSet<Character> Characters { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<StaffRole> StaffRoles { get; set; }
    
        public DbSet<RelatedSite> RelatedSites { get; set; }
        
        public DbSet<ReleaseInfo> ReleaseInfos { get; set; }
        
        public DbSet<ExternalLink> ExternalLinks { get; set; }
        
        public DbSet<PendingGameInfoDeletion> PendingGameInfoDeletions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppSetting>()
                .Property(x => x.IsAutoFetchInfoEnabled)
                .HasDefaultValue(true);
            modelBuilder.Entity<AppSetting>()
                .Property(x => x.SyncInterval)
                .HasDefaultValue(5);
            modelBuilder.Entity<AppSetting>()
                .Property(x => x.Localization)
                .HasDefaultValue("zh-tw");
            modelBuilder.Entity<GameInfo>()
                .HasMany(x => x.Tags)
                .WithMany(x => x.GameInfos)
                .UsingEntity<GameInfoTag>();
            modelBuilder.Entity<GameInfo>()
                .HasMany(x => x.Staffs)
                .WithMany(x => x.GameInfos)
                .UsingEntity<GameInfoStaff>();
            modelBuilder.Entity<GameInfo>()
                .HasMany(x => x.RelatedSites)
                .WithOne(x => x.GameInfo)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ReleaseInfo>()
                .HasMany(x => x.ExternalLinks)
                .WithOne(x => x.ReleaseInfo)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<LaunchOption>()
                .Property(x => x.SandboxieBoxName)
                .HasDefaultValue("DefaultBox");

            modelBuilder.Entity<StaffRole>()
                .HasData(Seed.SeedStaffRoles());

            base.OnModelCreating(modelBuilder);
        }
    }
}