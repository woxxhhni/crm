using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cls.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredFileCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "category",
                table: "files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "idx_files_category",
                table: "files",
                column: "category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_files_category",
                table: "files");

            migrationBuilder.DropColumn(
                name: "category",
                table: "files");
        }
    }
}
