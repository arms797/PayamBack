using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAndUserRoleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BarayeMogheeLogin",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CodeNoeUser",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "LoginMibashad",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Vazeeyat",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VazeeyatMovaghat",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CodeMarkaz",
                table: "AspNetUserRoles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodeOstan",
                table: "AspNetUserRoles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RolePishFarz",
                table: "AspNetUserRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CodeGrooheKarbari",
                table: "AspNetRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Emza",
                table: "AspNetRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Vazeeyat",
                table: "AspNetRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BarayeMogheeLogin",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CodeNoeUser",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LoginMibashad",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Vazeeyat",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VazeeyatMovaghat",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CodeMarkaz",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "CodeOstan",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "RolePishFarz",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "CodeGrooheKarbari",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "Emza",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "Vazeeyat",
                table: "AspNetRoles");
        }
    }
}
