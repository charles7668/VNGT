using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddSandboxieBoxName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SandboxieBoxName",
                table: "LaunchOption",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "DefaultBox");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SandboxieBoxName",
                table: "LaunchOption");
        }
    }
}
