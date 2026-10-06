using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class AddVazeeatToGroohAmoozeshiReshteh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Vazeeat",
                table: "Reshtehs",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Vazeeat",
                table: "GrooheAmoozeshis",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vazeeat",
                table: "Reshtehs");

            migrationBuilder.DropColumn(
                name: "Vazeeat",
                table: "GrooheAmoozeshis");
        }
    }
}
