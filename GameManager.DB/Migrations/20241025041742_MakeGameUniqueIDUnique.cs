using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class MakeGameUniqueIDUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameInfos_GameUniqeId",
                table: "GameInfos");

            migrationBuilder.CreateIndex(
                name: "IX_GameInfos_GameUniqeId",
                table: "GameInfos",
                column: "GameUniqeId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameInfos_GameUniqeId",
                table: "GameInfos");

            migrationBuilder.CreateIndex(
                name: "IX_GameInfos_GameUniqeId",
                table: "GameInfos",
                column: "GameUniqeId");
        }
    }
}
