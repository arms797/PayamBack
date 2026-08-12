using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class EditHamjarAnd1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Hamjavar_AkharinTaghaza",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "AKharinBarrasi",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "AkharinTaghaza",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "AmaliatElmi",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "AmaliatKhadamat",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "AmaliatMoaven",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "AmaliatRaeis",
                table: "Hamjavars");

            migrationBuilder.AlterColumn<int>(
                name: "NazarRaeis",
                table: "Hamjavars",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NazarMoaven",
                table: "Hamjavars",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NazarKhadamat",
                table: "Hamjavars",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NazarElmi",
                table: "Hamjavars",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NazarRaeis",
                table: "Hamjavars",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NazarMoaven",
                table: "Hamjavars",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NazarKhadamat",
                table: "Hamjavars",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NazarElmi",
                table: "Hamjavars",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AKharinBarrasi",
                table: "Hamjavars",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AkharinTaghaza",
                table: "Hamjavars",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AmaliatElmi",
                table: "Hamjavars",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AmaliatKhadamat",
                table: "Hamjavars",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AmaliatMoaven",
                table: "Hamjavars",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AmaliatRaeis",
                table: "Hamjavars",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar_AkharinTaghaza",
                table: "Hamjavars",
                column: "AkharinTaghaza");
        }
    }
}
