using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddExeFileField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExeFile",
                table: "GameInfos",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExeFile",
                table: "GameInfos");
        }
    }
}
