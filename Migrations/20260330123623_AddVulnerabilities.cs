using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBOMViewer.Migrations
{
    /// <inheritdoc />
    public partial class AddVulnerabilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vulnerabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CveId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: true),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CvssScore = table.Column<decimal>(type: "TEXT", nullable: true),
                    CvssVector = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PublishedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastFetchedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vulnerabilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DependencyVulnerabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DependencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    VulnerabilityId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DependencyVulnerabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DependencyVulnerabilities_Dependencies_DependencyId",
                        column: x => x.DependencyId,
                        principalTable: "Dependencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DependencyVulnerabilities_Vulnerabilities_VulnerabilityId",
                        column: x => x.VulnerabilityId,
                        principalTable: "Vulnerabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DependencyVulnerabilities_DependencyId_VulnerabilityId",
                table: "DependencyVulnerabilities",
                columns: new[] { "DependencyId", "VulnerabilityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DependencyVulnerabilities_VulnerabilityId",
                table: "DependencyVulnerabilities",
                column: "VulnerabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Vulnerabilities_CveId",
                table: "Vulnerabilities",
                column: "CveId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DependencyVulnerabilities");

            migrationBuilder.DropTable(
                name: "Vulnerabilities");
        }
    }
}
