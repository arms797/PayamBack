using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class AddAmoozeshiToSakhtemanKelass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NammClass",
                table: "SakhtemanKelass",
                newName: "NaamClass");

            migrationBuilder.AddColumn<bool>(
                name: "Amoozeshi",
                table: "SakhtemanKelass",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amoozeshi",
                table: "SakhtemanKelass");

            migrationBuilder.RenameColumn(
                name: "NaamClass",
                table: "SakhtemanKelass",
                newName: "NammClass");
        }
    }
}
