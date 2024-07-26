using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameInfoTag_GameInfos_GameInfoId",
                table: "GameInfoTag");

            migrationBuilder.DropForeignKey(
                name: "FK_GameInfoTag_Tags_TagId",
                table: "GameInfoTag");

            migrationBuilder.DropForeignKey(
                name: "FK_TextMapping_AppSettings_AppSettingId",
                table: "TextMapping");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TextMapping",
                table: "TextMapping");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameInfoTag",
                table: "GameInfoTag");

            migrationBuilder.RenameTable(
                name: "TextMapping",
                newName: "TextMappings");

            migrationBuilder.RenameTable(
                name: "GameInfoTag",
                newName: "GameInfoTags");

            migrationBuilder.RenameIndex(
                name: "IX_TextMapping_Original",
                table: "TextMappings",
                newName: "IX_TextMappings_Original");

            migrationBuilder.RenameIndex(
                name: "IX_TextMapping_AppSettingId",
                table: "TextMappings",
                newName: "IX_TextMappings_AppSettingId");

            migrationBuilder.RenameIndex(
                name: "IX_GameInfoTag_TagId",
                table: "GameInfoTags",
                newName: "IX_GameInfoTags_TagId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TextMappings",
                table: "TextMappings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameInfoTags",
                table: "GameInfoTags",
                columns: new[] { "GameInfoId", "TagId" });

            migrationBuilder.AddForeignKey(
                name: "FK_GameInfoTags_GameInfos_GameInfoId",
                table: "GameInfoTags",
                column: "GameInfoId",
                principalTable: "GameInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameInfoTags_Tags_TagId",
                table: "GameInfoTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TextMappings_AppSettings_AppSettingId",
                table: "TextMappings",
                column: "AppSettingId",
                principalTable: "AppSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameInfoTags_GameInfos_GameInfoId",
                table: "GameInfoTags");

            migrationBuilder.DropForeignKey(
                name: "FK_GameInfoTags_Tags_TagId",
                table: "GameInfoTags");

            migrationBuilder.DropForeignKey(
                name: "FK_TextMappings_AppSettings_AppSettingId",
                table: "TextMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TextMappings",
                table: "TextMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameInfoTags",
                table: "GameInfoTags");

            migrationBuilder.RenameTable(
                name: "TextMappings",
                newName: "TextMapping");

            migrationBuilder.RenameTable(
                name: "GameInfoTags",
                newName: "GameInfoTag");

            migrationBuilder.RenameIndex(
                name: "IX_TextMappings_Original",
                table: "TextMapping",
                newName: "IX_TextMapping_Original");

            migrationBuilder.RenameIndex(
                name: "IX_TextMappings_AppSettingId",
                table: "TextMapping",
                newName: "IX_TextMapping_AppSettingId");

            migrationBuilder.RenameIndex(
                name: "IX_GameInfoTags_TagId",
                table: "GameInfoTag",
                newName: "IX_GameInfoTag_TagId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TextMapping",
                table: "TextMapping",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameInfoTag",
                table: "GameInfoTag",
                columns: new[] { "GameInfoId", "TagId" });

            migrationBuilder.AddForeignKey(
                name: "FK_GameInfoTag_GameInfos_GameInfoId",
                table: "GameInfoTag",
                column: "GameInfoId",
                principalTable: "GameInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameInfoTag_Tags_TagId",
                table: "GameInfoTag",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TextMapping_AppSettings_AppSettingId",
                table: "TextMapping",
                column: "AppSettingId",
                principalTable: "AppSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
