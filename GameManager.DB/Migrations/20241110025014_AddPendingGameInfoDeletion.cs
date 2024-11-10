using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingGameInfoDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingGameInfoDeletions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameInfoUniqueId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DeletionDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingGameInfoDeletions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingGameInfoDeletions_GameInfoUniqueId",
                table: "PendingGameInfoDeletions",
                column: "GameInfoUniqueId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingGameInfoDeletions");
        }
    }
}
