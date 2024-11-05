using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyStamp_241105 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExternalLink_ReleaseInfos_ReleaseInfoId",
                table: "ExternalLink");

            migrationBuilder.DropForeignKey(
                name: "FK_RelatedSite_GameInfos_GameInfoId",
                table: "RelatedSite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RelatedSite",
                table: "RelatedSite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExternalLink",
                table: "ExternalLink");

            migrationBuilder.RenameTable(
                name: "RelatedSite",
                newName: "RelatedSites");

            migrationBuilder.RenameTable(
                name: "ExternalLink",
                newName: "ExternalLinks");

            migrationBuilder.RenameIndex(
                name: "IX_RelatedSite_GameInfoId",
                table: "RelatedSites",
                newName: "IX_RelatedSites_GameInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_ExternalLink_ReleaseInfoId",
                table: "ExternalLinks",
                newName: "IX_ExternalLinks_ReleaseInfoId");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "ReleaseInfos",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "GuideSite",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Characters",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "RelatedSites",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "ExternalLinks",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RelatedSites",
                table: "RelatedSites",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExternalLinks",
                table: "ExternalLinks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_Name",
                table: "Characters",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_OriginalName",
                table: "Characters",
                column: "OriginalName");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalLinks_ReleaseInfos_ReleaseInfoId",
                table: "ExternalLinks",
                column: "ReleaseInfoId",
                principalTable: "ReleaseInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RelatedSites_GameInfos_GameInfoId",
                table: "RelatedSites",
                column: "GameInfoId",
                principalTable: "GameInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExternalLinks_ReleaseInfos_ReleaseInfoId",
                table: "ExternalLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_RelatedSites_GameInfos_GameInfoId",
                table: "RelatedSites");

            migrationBuilder.DropIndex(
                name: "IX_Characters_Name",
                table: "Characters");

            migrationBuilder.DropIndex(
                name: "IX_Characters_OriginalName",
                table: "Characters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RelatedSites",
                table: "RelatedSites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExternalLinks",
                table: "ExternalLinks");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "ReleaseInfos");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "GuideSite");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "RelatedSites");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "ExternalLinks");

            migrationBuilder.RenameTable(
                name: "RelatedSites",
                newName: "RelatedSite");

            migrationBuilder.RenameTable(
                name: "ExternalLinks",
                newName: "ExternalLink");

            migrationBuilder.RenameIndex(
                name: "IX_RelatedSites_GameInfoId",
                table: "RelatedSite",
                newName: "IX_RelatedSite_GameInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_ExternalLinks_ReleaseInfoId",
                table: "ExternalLink",
                newName: "IX_ExternalLink_ReleaseInfoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RelatedSite",
                table: "RelatedSite",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExternalLink",
                table: "ExternalLink",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalLink_ReleaseInfos_ReleaseInfoId",
                table: "ExternalLink",
                column: "ReleaseInfoId",
                principalTable: "ReleaseInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RelatedSite_GameInfos_GameInfoId",
                table: "RelatedSite",
                column: "GameInfoId",
                principalTable: "GameInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
