using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UploadTime",
                table: "GameInfos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameInfos_UploadTime_GameName",
                table: "GameInfos",
                columns: new[] { "UploadTime", "GameName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameInfos_UploadTime_GameName",
                table: "GameInfos");

            migrationBuilder.DropColumn(
                name: "UploadTime",
                table: "GameInfos");
        }
    }
}
