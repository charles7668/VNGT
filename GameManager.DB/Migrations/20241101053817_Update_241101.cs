using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class Update_241101 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GameUniqeId",
                table: "GameInfos",
                newName: "GameUniqueId");

            migrationBuilder.RenameIndex(
                name: "IX_GameInfos_GameUniqeId",
                table: "GameInfos",
                newName: "IX_GameInfos_GameUniqueId");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "StaffRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImageUrl",
                table: "GameInfos",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundImageUrl",
                table: "GameInfos");

            migrationBuilder.RenameColumn(
                name: "GameUniqueId",
                table: "GameInfos",
                newName: "GameUniqeId");

            migrationBuilder.RenameIndex(
                name: "IX_GameInfos_GameUniqueId",
                table: "GameInfos",
                newName: "IX_GameInfos_GameUniqeId");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "StaffRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);
        }
    }
}
