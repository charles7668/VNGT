using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddRunasAdminOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LaunchOptionId",
                table: "GameInfos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LaunchOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RunAsAdmin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaunchOption", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameInfos_LaunchOptionId",
                table: "GameInfos",
                column: "LaunchOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameInfos_LaunchOption_LaunchOptionId",
                table: "GameInfos",
                column: "LaunchOptionId",
                principalTable: "LaunchOption",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameInfos_LaunchOption_LaunchOptionId",
                table: "GameInfos");

            migrationBuilder.DropTable(
                name: "LaunchOption");

            migrationBuilder.DropIndex(
                name: "IX_GameInfos_LaunchOptionId",
                table: "GameInfos");

            migrationBuilder.DropColumn(
                name: "LaunchOptionId",
                table: "GameInfos");
        }
    }
}
