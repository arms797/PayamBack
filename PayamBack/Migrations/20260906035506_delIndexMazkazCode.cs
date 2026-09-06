using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class delIndexMazkazCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Markaz_CodeMarkaz",
                table: "Markazes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Markaz_CodeMarkaz",
                table: "Markazes",
                column: "CodeMarkaz",
                unique: true,
                filter: "[CodeMarkaz] IS NOT NULL");
        }
    }
}
