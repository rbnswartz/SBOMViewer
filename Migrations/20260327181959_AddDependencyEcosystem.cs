using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBOMViewer.Migrations
{
    /// <inheritdoc />
    public partial class AddDependencyEcosystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ecosystem",
                table: "Dependencies",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ecosystem",
                table: "Dependencies");
        }
    }
}
