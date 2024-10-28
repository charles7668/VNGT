using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GameManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGameInfoModel_241031 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameChineseName",
                table: "GameInfos",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameEnglishName",
                table: "GameInfos",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScreenShots",
                table: "GameInfos",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalName = table.Column<string>(type: "TEXT", nullable: true),
                    Alias = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Age = table.Column<string>(type: "TEXT", nullable: true),
                    Sex = table.Column<string>(type: "TEXT", nullable: true),
                    Birthday = table.Column<string>(type: "TEXT", nullable: true),
                    BloodType = table.Column<string>(type: "TEXT", nullable: true),
                    GameInfoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_GameInfos_GameInfoId",
                        column: x => x.GameInfoId,
                        principalTable: "GameInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelatedSite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    GameInfoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatedSite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelatedSite_GameInfos_GameInfoId",
                        column: x => x.GameInfoId,
                        principalTable: "GameInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameInfoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReleaseName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ReleaseLanguage = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Platforms = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AgeRating = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseInfos_GameInfos_GameInfoId",
                        column: x => x.GameInfoId,
                        principalTable: "GameInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Label = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ReleaseInfoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalLink_ReleaseInfos_ReleaseInfoId",
                        column: x => x.ReleaseInfoId,
                        principalTable: "ReleaseInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Staffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StaffRoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Staffs_StaffRoles_StaffRoleId",
                        column: x => x.StaffRoleId,
                        principalTable: "StaffRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameInfoStaffs",
                columns: table => new
                {
                    GameInfoId = table.Column<int>(type: "INTEGER", nullable: false),
                    StaffId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameInfoStaffs", x => new { x.GameInfoId, x.StaffId });
                    table.ForeignKey(
                        name: "FK_GameInfoStaffs_GameInfos_GameInfoId",
                        column: x => x.GameInfoId,
                        principalTable: "GameInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameInfoStaffs_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "StaffRoles",
                columns: new[] { "Id", "RoleName" },
                values: new object[,]
                {
                    { 1, "scenario" },
                    { 2, "director" },
                    { 3, "character design" },
                    { 4, "artist" },
                    { 5, "music" },
                    { 6, "song" },
                    { 99, "staff" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_GameInfoId",
                table: "Characters",
                column: "GameInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLink_ReleaseInfoId",
                table: "ExternalLink",
                column: "ReleaseInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_GameInfoStaffs_StaffId",
                table: "GameInfoStaffs",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_RelatedSite_GameInfoId",
                table: "RelatedSite",
                column: "GameInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseInfos_GameInfoId",
                table: "ReleaseInfos",
                column: "GameInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffRoles_RoleName",
                table: "StaffRoles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_Name",
                table: "Staffs",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_Name_StaffRoleId",
                table: "Staffs",
                columns: new[] { "Name", "StaffRoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_StaffRoleId",
                table: "Staffs",
                column: "StaffRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_StaffRoleId_Name",
                table: "Staffs",
                columns: new[] { "StaffRoleId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "ExternalLink");

            migrationBuilder.DropTable(
                name: "GameInfoStaffs");

            migrationBuilder.DropTable(
                name: "RelatedSite");

            migrationBuilder.DropTable(
                name: "ReleaseInfos");

            migrationBuilder.DropTable(
                name: "Staffs");

            migrationBuilder.DropTable(
                name: "StaffRoles");

            migrationBuilder.DropColumn(
                name: "GameChineseName",
                table: "GameInfos");

            migrationBuilder.DropColumn(
                name: "GameEnglishName",
                table: "GameInfos");

            migrationBuilder.DropColumn(
                name: "ScreenShots",
                table: "GameInfos");
        }
    }
}
