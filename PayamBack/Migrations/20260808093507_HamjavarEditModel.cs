using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class HamjavarEditModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hamjavar1s_AspNetRoles_RoleIdSabtKonandeh",
                table: "Hamjavar1s");

            migrationBuilder.DropForeignKey(
                name: "FK_Hamjavar1s_Faaliats_FaaliatId",
                table: "Hamjavar1s");

            migrationBuilder.DropIndex(
                name: "IX_Hamjavar1_FaaliatId",
                table: "Hamjavar1s");

            migrationBuilder.DropColumn(
                name: "FaaliatId",
                table: "Hamjavar1s");

            migrationBuilder.RenameColumn(
                name: "RoleIdSabtKonandeh",
                table: "Hamjavar1s",
                newName: "HamjavarId1");

            migrationBuilder.RenameIndex(
                name: "IX_Hamjavar1s_RoleIdSabtKonandeh",
                table: "Hamjavar1s",
                newName: "IX_Hamjavar1s_HamjavarId1");

            migrationBuilder.AddColumn<string>(
                name: "RoleMarkazApproved",
                table: "Hamjavars",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleMarkazKhadamatOstan",
                table: "Hamjavars",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleMarkazRaeis",
                table: "Hamjavars",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleMarkazSabtKonandeh",
                table: "Hamjavars",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserIdApproved",
                table: "Hamjavars",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserIdKhadamatOstan",
                table: "Hamjavars",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserIdRaeis",
                table: "Hamjavars",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserIdSabtKonandeh",
                table: "Hamjavars",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VahedMovazaf",
                table: "Hamjavars",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaaliatIds",
                table: "Hamjavar1s",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleMarkazSabtKonandeh",
                table: "Hamjavar1s",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Hamjavar1s_Hamjavars_HamjavarId1",
                table: "Hamjavar1s",
                column: "HamjavarId1",
                principalTable: "Hamjavars",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hamjavar1s_Hamjavars_HamjavarId1",
                table: "Hamjavar1s");

            migrationBuilder.DropColumn(
                name: "RoleMarkazApproved",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "RoleMarkazKhadamatOstan",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "RoleMarkazRaeis",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "RoleMarkazSabtKonandeh",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "UserIdApproved",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "UserIdKhadamatOstan",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "UserIdRaeis",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "UserIdSabtKonandeh",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "VahedMovazaf",
                table: "Hamjavars");

            migrationBuilder.DropColumn(
                name: "FaaliatIds",
                table: "Hamjavar1s");

            migrationBuilder.DropColumn(
                name: "RoleMarkazSabtKonandeh",
                table: "Hamjavar1s");

            migrationBuilder.RenameColumn(
                name: "HamjavarId1",
                table: "Hamjavar1s",
                newName: "RoleIdSabtKonandeh");

            migrationBuilder.RenameIndex(
                name: "IX_Hamjavar1s_HamjavarId1",
                table: "Hamjavar1s",
                newName: "IX_Hamjavar1s_RoleIdSabtKonandeh");

            migrationBuilder.AddColumn<int>(
                name: "FaaliatId",
                table: "Hamjavar1s",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar1_FaaliatId",
                table: "Hamjavar1s",
                column: "FaaliatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hamjavar1s_AspNetRoles_RoleIdSabtKonandeh",
                table: "Hamjavar1s",
                column: "RoleIdSabtKonandeh",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Hamjavar1s_Faaliats_FaaliatId",
                table: "Hamjavar1s",
                column: "FaaliatId",
                principalTable: "Faaliats",
                principalColumn: "Id");
        }
    }
}
