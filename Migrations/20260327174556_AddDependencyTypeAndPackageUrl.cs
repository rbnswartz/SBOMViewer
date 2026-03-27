using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBOMViewer.Migrations
{
    /// <inheritdoc />
    public partial class AddDependencyTypeAndPackageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PackageUrl",
                table: "Dependencies",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Dependencies",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackageUrl",
                table: "Dependencies");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Dependencies");
        }
    }
}
