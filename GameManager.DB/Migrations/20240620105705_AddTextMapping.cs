using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddTextMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Localization",
                table: "AppSettings",
                type: "TEXT",
                nullable: true,
                defaultValue: "zh-tw",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldDefaultValue: "en-US");

            migrationBuilder.CreateTable(
                name: "TextMapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    Replace = table.Column<string>(type: "TEXT", nullable: true),
                    AppSettingId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextMapping_AppSettings_AppSettingId",
                        column: x => x.AppSettingId,
                        principalTable: "AppSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TextMapping_AppSettingId",
                table: "TextMapping",
                column: "AppSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_TextMapping_Original",
                table: "TextMapping",
                column: "Original",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TextMapping");

            migrationBuilder.AlterColumn<string>(
                name: "Localization",
                table: "AppSettings",
                type: "TEXT",
                nullable: true,
                defaultValue: "en-US",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldDefaultValue: "zh-tw");
        }
    }
}
