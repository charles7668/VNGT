using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayModeSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayMode",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayMode",
                table: "AppSettings");
        }
    }
}
