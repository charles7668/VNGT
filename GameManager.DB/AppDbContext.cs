﻿using GameManager.DB.Models;
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

        public DbSet<TextMapping> TextMappings { get; set; }

        public DbSet<GameInfoTag> GameInfoTags { get; set; }

        public DbSet<Tag> Tags { get; set; }


        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // enable sensitive data logging if needed
            // options.EnableSensitiveDataLogging().LogTo((log) =>
            // {
            //     Debugger.Log(1, "sql", log);
            // }, LogLevel.Information);
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
            modelBuilder.Entity<GameInfo>()
                .HasMany(x => x.Tags)
                .WithMany(x => x.GameInfos)
                .UsingEntity<GameInfoTag>();

            base.OnModelCreating(modelBuilder);
        }
    }
}