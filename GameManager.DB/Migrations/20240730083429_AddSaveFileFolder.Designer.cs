﻿// <auto-generated />
using System;
using GameManager.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GameManager.DB.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240730083429_AddSaveFileFolder")]
    partial class AddSaveFileFolder
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.4");

            modelBuilder.Entity("GameManager.DB.Models.AppSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsAutoFetchInfoEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(true);

                    b.Property<string>("LocaleEmulatorPath")
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.Property<string>("Localization")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT")
                        .HasDefaultValue("zh-tw");

                    b.HasKey("Id");

                    b.ToTable("AppSettings");
                });

            modelBuilder.Entity("GameManager.DB.Models.GameInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CoverPath")
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasMaxLength(10000)
                        .HasColumnType("TEXT");

                    b.Property<string>("Developer")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("ExeFile")
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.Property<string>("ExePath")
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.Property<string>("GameInfoId")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("GameName")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsFavorite")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastPlayed")
                        .HasColumnType("TEXT");

                    b.Property<int?>("LaunchOptionId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SaveFilePath")
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UploadTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ExePath")
                        .IsUnique();

                    b.HasIndex("LaunchOptionId");

                    b.HasIndex("UploadTime", "GameName");

                    b.ToTable("GameInfos");
                });

            modelBuilder.Entity("GameManager.DB.Models.GameInfoTag", b =>
                {
                    b.Property<int>("GameInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TagId")
                        .HasColumnType("INTEGER");

                    b.HasKey("GameInfoId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("GameInfoTags");
                });

            modelBuilder.Entity("GameManager.DB.Models.GuideSite", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AppSettingId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("SiteUrl")
                        .HasMaxLength(150)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AppSettingId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("GuideSite");
                });

            modelBuilder.Entity("GameManager.DB.Models.LaunchOption", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsVNGTTranslatorNeedAdmin")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LaunchWithLocaleEmulator")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<bool>("RunAsAdmin")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("RunWithVNGTTranslator")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("LaunchOption");
                });

            modelBuilder.Entity("GameManager.DB.Models.Library", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FolderPath")
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Libraries");
                });

            modelBuilder.Entity("GameManager.DB.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("GameManager.DB.Models.TextMapping", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AppSettingId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Original")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Replace")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AppSettingId");

                    b.HasIndex("Original")
                        .IsUnique();

                    b.ToTable("TextMappings");
                });

            modelBuilder.Entity("GameManager.DB.Models.GameInfo", b =>
                {
                    b.HasOne("GameManager.DB.Models.LaunchOption", "LaunchOption")
                        .WithMany()
                        .HasForeignKey("LaunchOptionId");

                    b.Navigation("LaunchOption");
                });

            modelBuilder.Entity("GameManager.DB.Models.GameInfoTag", b =>
                {
                    b.HasOne("GameManager.DB.Models.GameInfo", null)
                        .WithMany()
                        .HasForeignKey("GameInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GameManager.DB.Models.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GameManager.DB.Models.GuideSite", b =>
                {
                    b.HasOne("GameManager.DB.Models.AppSetting", "AppSetting")
                        .WithMany("GuideSites")
                        .HasForeignKey("AppSettingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppSetting");
                });

            modelBuilder.Entity("GameManager.DB.Models.TextMapping", b =>
                {
                    b.HasOne("GameManager.DB.Models.AppSetting", "AppSetting")
                        .WithMany("TextMappings")
                        .HasForeignKey("AppSettingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppSetting");
                });

            modelBuilder.Entity("GameManager.DB.Models.AppSetting", b =>
                {
                    b.Navigation("GuideSites");

                    b.Navigation("TextMappings");
                });
#pragma warning restore 612, 618
        }
    }
}
