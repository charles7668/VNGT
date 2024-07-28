using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddRunWithVNGTTranslatorLaunchOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVNGTTranslatorNeedAdmin",
                table: "LaunchOption",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RunWithVNGTTranslator",
                table: "LaunchOption",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVNGTTranslatorNeedAdmin",
                table: "LaunchOption");

            migrationBuilder.DropColumn(
                name: "RunWithVNGTTranslator",
                table: "LaunchOption");
        }
    }
}
