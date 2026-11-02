using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class AddLevelFieldToMarkazReshteh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "MarkazReshteh",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "DarsEraeh",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "MarkazReshteh");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "DarsEraeh");
        }
    }
}
