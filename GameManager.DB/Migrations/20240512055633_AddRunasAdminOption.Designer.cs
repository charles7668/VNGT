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
    [Migration("20240512055633_AddRunasAdminOption")]
    partial class AddRunasAdminOption
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.4");

            modelBuilder.Entity("GameManager.DB.Models.GameInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CoverPath")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Developer")
                        .HasColumnType("TEXT");

                    b.Property<string>("ExePath")
                        .HasColumnType("TEXT");

                    b.Property<string>("GameInfoId")
                        .HasColumnType("TEXT");

                    b.Property<string>("GameName")
                        .HasColumnType("TEXT");

                    b.Property<int?>("LaunchOptionId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("LaunchOptionId");

                    b.ToTable("GameInfos");
                });

            modelBuilder.Entity("GameManager.DB.Models.LaunchOption", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("RunAsAdmin")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("LaunchOption");
                });

            modelBuilder.Entity("GameManager.DB.Models.GameInfo", b =>
                {
                    b.HasOne("GameManager.DB.Models.LaunchOption", "LaunchOption")
                        .WithMany()
                        .HasForeignKey("LaunchOptionId");

                    b.Navigation("LaunchOption");
                });
#pragma warning restore 612, 618
        }
    }
}
