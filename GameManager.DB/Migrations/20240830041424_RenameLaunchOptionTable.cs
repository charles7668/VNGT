using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class RenameLaunchOptionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameInfos_LaunchOption_LaunchOptionId",
                table: "GameInfos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LaunchOption",
                table: "LaunchOption");

            migrationBuilder.RenameTable(
                name: "LaunchOption",
                newName: "LaunchOptions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LaunchOptions",
                table: "LaunchOptions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameInfos_LaunchOptions_LaunchOptionId",
                table: "GameInfos",
                column: "LaunchOptionId",
                principalTable: "LaunchOptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameInfos_LaunchOptions_LaunchOptionId",
                table: "GameInfos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LaunchOptions",
                table: "LaunchOptions");

            migrationBuilder.RenameTable(
                name: "LaunchOptions",
                newName: "LaunchOption");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LaunchOption",
                table: "LaunchOption",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameInfos_LaunchOption_LaunchOptionId",
                table: "GameInfos",
                column: "LaunchOptionId",
                principalTable: "LaunchOption",
                principalColumn: "Id");
        }
    }
}
