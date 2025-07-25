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
    [Migration("20250626114802_AddSearchHistory")]
    partial class AddSearchHistory
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("GameManager.DB.Models.AppSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("DisplayMode")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableSync")
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

                    b.Property<string>("SandboxiePath")
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.Property<int>("SyncInterval")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(5);

                    b.Property<DateTime>("UpdatedTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("WebDAVPassword")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("WebDAVUrl")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<string>("WebDAVUser")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("AppSettings");
                });

            modelBuilder.Entity("GameManager.DB.Models.Character", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Age")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Birthday")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("BloodType")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasMaxLength(100000)
                        .HasColumnType("TEXT");

                    b.Property<int>("GameInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ImageUrl")
                        .HasMaxLength(1000)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalName")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Sex")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GameInfoId");

                    b.HasIndex("Name");

                    b.HasIndex("OriginalName");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("GameManager.DB.Models.ExternalLink", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConcurrencyStamp")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Label")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("ReleaseInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ReleaseInfoId");

                    b.ToTable("ExternalLinks");
                });

            modelBuilder.Entity("GameManager.DB.Models.GameInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BackgroundImageUrl")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<string>("CoverPath")
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasMaxLength(10000)
                        .HasColumnType("TEXT");

                    b.Property<string>("Developer")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<bool>("EnableSync")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ExeFile")
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.Property<string>("ExePath")
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.Property<string>("GameChineseName")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("GameEnglishName")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("GameInfoFetchId")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("GameName")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("GameUniqueId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsFavorite")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastPlayed")
                        .HasColumnType("TEXT");

                    b.Property<int?>("LaunchOptionId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("PlayTime")
                        .HasColumnType("REAL");

                    b.Property<DateTime?>("ReleaseDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("SaveFilePath")
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.Property<string>("ScreenShots")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UploadTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ExePath");

                    b.HasIndex("GameUniqueId")
                        .IsUnique();

                    b.HasIndex("LaunchOptionId");

                    b.HasIndex("UploadTime", "GameName");

                    b.ToTable("GameInfos");
                });

            modelBuilder.Entity("GameManager.DB.Models.GameInfoStaff", b =>
                {
                    b.Property<int>("GameInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StaffId")
                        .HasColumnType("INTEGER");

                    b.HasKey("GameInfoId", "StaffId");

                    b.HasIndex("StaffId");

                    b.ToTable("GameInfoStaffs");
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

                    b.Property<string>("ConcurrencyStamp")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

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

                    b.Property<bool>("RunWithSandboxie")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("RunWithVNGTTranslator")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SandboxieBoxName")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT")
                        .HasDefaultValue("DefaultBox");

                    b.HasKey("Id");

                    b.ToTable("LaunchOptions");
                });

            modelBuilder.Entity("GameManager.DB.Models.Library", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FolderPath")
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Libraries");
                });

            modelBuilder.Entity("GameManager.DB.Models.PendingGameInfoDeletion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DeletionDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("GameInfoUniqueId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GameInfoUniqueId")
                        .IsUnique();

                    b.ToTable("PendingGameInfoDeletions");
                });

            modelBuilder.Entity("GameManager.DB.Models.RelatedSite", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConcurrencyStamp")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("GameInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GameInfoId");

                    b.ToTable("RelatedSites");
                });

            modelBuilder.Entity("GameManager.DB.Models.ReleaseInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AgeRating")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConcurrencyStamp")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("GameInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Platforms")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReleaseDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("ReleaseLanguage")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("ReleaseName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GameInfoId");

                    b.ToTable("ReleaseInfos");
                });

            modelBuilder.Entity("GameManager.DB.Models.SearchHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("SearchText")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("SearchTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("SearchHistories");
                });

            modelBuilder.Entity("GameManager.DB.Models.Staff", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("StaffRoleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("StaffRoleId");

                    b.HasIndex("Name", "StaffRoleId")
                        .IsUnique();

                    b.HasIndex("StaffRoleId", "Name")
                        .IsUnique();

                    b.ToTable("Staffs");
                });

            modelBuilder.Entity("GameManager.DB.Models.StaffRole", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoleName")
                        .IsUnique();

                    b.ToTable("StaffRoles");

                    b.HasData(
                        new
                        {
                            Id = 99,
                            RoleName = "staff"
                        },
                        new
                        {
                            Id = 1,
                            RoleName = "scenario"
                        },
                        new
                        {
                            Id = 2,
                            RoleName = "director"
                        },
                        new
                        {
                            Id = 3,
                            RoleName = "character design"
                        },
                        new
                        {
                            Id = 4,
                            RoleName = "artist"
                        },
                        new
                        {
                            Id = 5,
                            RoleName = "music"
                        },
                        new
                        {
                            Id = 6,
                            RoleName = "song"
                        });
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

            modelBuilder.Entity("GameManager.DB.Models.Character", b =>
                {
                    b.HasOne("GameManager.DB.Models.GameInfo", "GameInfo")
                        .WithMany("Characters")
                        .HasForeignKey("GameInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GameInfo");
                });

            modelBuilder.Entity("GameManager.DB.Models.ExternalLink", b =>
                {
                    b.HasOne("GameManager.DB.Models.ReleaseInfo", "ReleaseInfo")
                        .WithMany("ExternalLinks")
                        .HasForeignKey("ReleaseInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ReleaseInfo");
                });

            modelBuilder.Entity("GameManager.DB.Models.GameInfo", b =>
                {
                    b.HasOne("GameManager.DB.Models.LaunchOption", "LaunchOption")
                        .WithMany()
                        .HasForeignKey("LaunchOptionId");

                    b.Navigation("LaunchOption");
                });

            modelBuilder.Entity("GameManager.DB.Models.GameInfoStaff", b =>
                {
                    b.HasOne("GameManager.DB.Models.GameInfo", null)
                        .WithMany()
                        .HasForeignKey("GameInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GameManager.DB.Models.Staff", null)
                        .WithMany()
                        .HasForeignKey("StaffId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
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

            modelBuilder.Entity("GameManager.DB.Models.RelatedSite", b =>
                {
                    b.HasOne("GameManager.DB.Models.GameInfo", "GameInfo")
                        .WithMany("RelatedSites")
                        .HasForeignKey("GameInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GameInfo");
                });

            modelBuilder.Entity("GameManager.DB.Models.ReleaseInfo", b =>
                {
                    b.HasOne("GameManager.DB.Models.GameInfo", "GameInfo")
                        .WithMany("ReleaseInfos")
                        .HasForeignKey("GameInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GameInfo");
                });

            modelBuilder.Entity("GameManager.DB.Models.Staff", b =>
                {
                    b.HasOne("GameManager.DB.Models.StaffRole", "StaffRole")
                        .WithMany()
                        .HasForeignKey("StaffRoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("StaffRole");
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

            modelBuilder.Entity("GameManager.DB.Models.GameInfo", b =>
                {
                    b.Navigation("Characters");

                    b.Navigation("RelatedSites");

                    b.Navigation("ReleaseInfos");
                });

            modelBuilder.Entity("GameManager.DB.Models.ReleaseInfo", b =>
                {
                    b.Navigation("ExternalLinks");
                });
#pragma warning restore 612, 618
        }
    }
}
