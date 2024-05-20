using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizationSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Localization",
                table: "AppSettings",
                type: "TEXT",
                nullable: true,
                defaultValue: "en-US");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Localization",
                table: "AppSettings");
        }
    }
}
