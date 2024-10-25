using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddGameUniqueID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameInfos_ExePath",
                table: "GameInfos");

            migrationBuilder.AddColumn<Guid>(
                name: "GameUniqeId",
                table: "GameInfos",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_GameInfos_ExePath",
                table: "GameInfos",
                column: "ExePath");

            migrationBuilder.CreateIndex(
                name: "IX_GameInfos_GameUniqeId",
                table: "GameInfos",
                column: "GameUniqeId");

            migrationBuilder.Sql("UPDATE GameInfos SET GameUniqeId = lower(hex(randomblob(16)))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameInfos_ExePath",
                table: "GameInfos");

            migrationBuilder.DropIndex(
                name: "IX_GameInfos_GameUniqeId",
                table: "GameInfos");

            migrationBuilder.DropColumn(
                name: "GameUniqeId",
                table: "GameInfos");

            migrationBuilder.CreateIndex(
                name: "IX_GameInfos_ExePath",
                table: "GameInfos",
                column: "ExePath",
                unique: true);
        }
    }
}
