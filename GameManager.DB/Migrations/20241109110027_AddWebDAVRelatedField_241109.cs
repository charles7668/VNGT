using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddWebDAVRelatedField_241109 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "GameInfos",
                newName: "ReleaseDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedTime",
                table: "Libraries",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "EnableSync",
                table: "GameInfos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedTime",
                table: "GameInfos",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "EnableSync",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SyncInterval",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedTime",
                table: "AppSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "WebDAVPassword",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebDAVUrl",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebDAVUser",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedTime",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "EnableSync",
                table: "GameInfos");

            migrationBuilder.DropColumn(
                name: "UpdatedTime",
                table: "GameInfos");

            migrationBuilder.DropColumn(
                name: "EnableSync",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "SyncInterval",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedTime",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "WebDAVPassword",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "WebDAVUrl",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "WebDAVUser",
                table: "AppSettings");

            migrationBuilder.RenameColumn(
                name: "ReleaseDate",
                table: "GameInfos",
                newName: "DateTime");
        }
    }
}
