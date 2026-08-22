using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class changeHaftegiElmitermModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ElmiTerms_AspNetRoles_RoleIdSabtKonandeh",
                table: "ElmiTerms");

            migrationBuilder.DropForeignKey(
                name: "FK_ElmiTerms_Terms_TermCode",
                table: "ElmiTerms");

            migrationBuilder.DropIndex(
                name: "IX_ElmiTerm_UserId_TermCode",
                table: "ElmiTerms");

            migrationBuilder.DropIndex(
                name: "IX_ElmiTerms_RoleIdSabtKonandeh",
                table: "ElmiTerms");

            migrationBuilder.DropIndex(
                name: "IX_ElmiTerms_TermCode",
                table: "ElmiTerms");

            migrationBuilder.DropIndex(
                name: "IX_BarnamehHaftegiOstad_CodeOstad_CodeTerm_MarkazId_RoozeHafteh",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "TermCode",
                table: "ElmiTerms");

            migrationBuilder.DropColumn(
                name: "A",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "B",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "C",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "Jozeiat",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "RoozeHafteh",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.RenameColumn(
                name: "RoleIdSabtKonandeh",
                table: "ElmiTerms",
                newName: "RoleMarkazSabtKonandeh");

            migrationBuilder.RenameColumn(
                name: "H",
                table: "BarnamehHaftegiOstads",
                newName: "UserIdModirGrooh");

            migrationBuilder.RenameColumn(
                name: "G",
                table: "BarnamehHaftegiOstads",
                newName: "UserIdMoaven");

            migrationBuilder.RenameColumn(
                name: "F",
                table: "BarnamehHaftegiOstads",
                newName: "NazarModirGrooh");

            migrationBuilder.RenameColumn(
                name: "E",
                table: "BarnamehHaftegiOstads",
                newName: "NazarMoaven");

            migrationBuilder.RenameColumn(
                name: "D",
                table: "BarnamehHaftegiOstads",
                newName: "NararElmi");

            migrationBuilder.AddColumn<int>(
                name: "NoeMarkaz",
                table: "Markazes",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TedadSaatMovazafi",
                table: "ElmiTerms",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AkharinVazeeat",
                table: "ElmiTerms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TedadVahedMovazafi",
                table: "ElmiTerms",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Vazeeat",
                table: "ElmiTerms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "OstadId",
                table: "BarnamehHaftegiOstads",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodeTerm",
                table: "BarnamehHaftegiOstads",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleMarkazMoaven",
                table: "BarnamehHaftegiOstads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleMarkazModirGrooh",
                table: "BarnamehHaftegiOstads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TarikhElmi",
                table: "BarnamehHaftegiOstads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TarikhMoaven",
                table: "BarnamehHaftegiOstads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TarikhModirGrooh",
                table: "BarnamehHaftegiOstads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BarnamehHaftegiOstad1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarnamehHaftegiOstadId = table.Column<int>(type: "int", nullable: false),
                    MarkazId = table.Column<int>(type: "int", nullable: true),
                    RoozeHafteh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    A = table.Column<int>(type: "int", nullable: true),
                    MarkazIdA = table.Column<int>(type: "int", nullable: true),
                    B = table.Column<int>(type: "int", nullable: true),
                    MarkazIdB = table.Column<int>(type: "int", nullable: true),
                    C = table.Column<int>(type: "int", nullable: true),
                    MarkazIdC = table.Column<int>(type: "int", nullable: true),
                    D = table.Column<int>(type: "int", nullable: true),
                    MarkazIdD = table.Column<int>(type: "int", nullable: true),
                    E = table.Column<int>(type: "int", nullable: true),
                    MarkazIdE = table.Column<int>(type: "int", nullable: true),
                    F = table.Column<int>(type: "int", nullable: true),
                    MarkazIdF = table.Column<int>(type: "int", nullable: true),
                    G = table.Column<int>(type: "int", nullable: true),
                    MarkazIdG = table.Column<int>(type: "int", nullable: true),
                    H = table.Column<int>(type: "int", nullable: true),
                    MarkazIdH = table.Column<int>(type: "int", nullable: true),
                    Jozeiat = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarnamehHaftegiOstad1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BarnamehHaftegiOstad1_BarnamehHaftegiOstads_BarnamehHaftegiOstadId",
                        column: x => x.BarnamehHaftegiOstadId,
                        principalTable: "BarnamehHaftegiOstads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElmiTerms_UserId",
                table: "ElmiTerms",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad_NazarMoaven",
                table: "BarnamehHaftegiOstads",
                column: "NazarMoaven");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad_NazarModirGrooh",
                table: "BarnamehHaftegiOstads",
                column: "NazarModirGrooh");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad_OstadId_CodeTerm",
                table: "BarnamehHaftegiOstads",
                columns: new[] { "OstadId", "CodeTerm" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstads_UserIdMoaven",
                table: "BarnamehHaftegiOstads",
                column: "UserIdMoaven");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstads_UserIdModirGrooh",
                table: "BarnamehHaftegiOstads",
                column: "UserIdModirGrooh");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad1_MarkazId",
                table: "BarnamehHaftegiOstad1",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad1_MarkazIdA",
                table: "BarnamehHaftegiOstad1",
                column: "MarkazIdA");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad1_MarkazIdB",
                table: "BarnamehHaftegiOstad1",
                column: "MarkazIdB");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad1_MarkazIdC",
                table: "BarnamehHaftegiOstad1",
                column: "MarkazIdC");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad1_MarkazIdD",
                table: "BarnamehHaftegiOstad1",
                column: "MarkazIdD");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad1_MarkazIdE",
                table: "BarnamehHaftegiOstad1",
                column: "MarkazIdE");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad1_MarkazIdF",
                table: "BarnamehHaftegiOstad1",
                column: "MarkazIdF");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad1_MarkazIdG",
                table: "BarnamehHaftegiOstad1",
                column: "MarkazIdG");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad1_MarkazIdH",
                table: "BarnamehHaftegiOstad1",
                column: "MarkazIdH");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad1_OstadId_RoozeHafteh",
                table: "BarnamehHaftegiOstad1",
                columns: new[] { "BarnamehHaftegiOstadId", "RoozeHafteh" });

            migrationBuilder.AddForeignKey(
                name: "FK_BarnamehHaftegiOstads_AspNetUsers_UserIdMoaven",
                table: "BarnamehHaftegiOstads",
                column: "UserIdMoaven",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BarnamehHaftegiOstads_AspNetUsers_UserIdModirGrooh",
                table: "BarnamehHaftegiOstads",
                column: "UserIdModirGrooh",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BarnamehHaftegiOstads_AspNetUsers_UserIdMoaven",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropForeignKey(
                name: "FK_BarnamehHaftegiOstads_AspNetUsers_UserIdModirGrooh",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropTable(
                name: "BarnamehHaftegiOstad1");

            migrationBuilder.DropIndex(
                name: "IX_ElmiTerms_UserId",
                table: "ElmiTerms");

            migrationBuilder.DropIndex(
                name: "IX_BarnamehHaftegiOstad_NazarMoaven",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropIndex(
                name: "IX_BarnamehHaftegiOstad_NazarModirGrooh",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropIndex(
                name: "IX_BarnamehHaftegiOstad_OstadId_CodeTerm",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropIndex(
                name: "IX_BarnamehHaftegiOstads_UserIdMoaven",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropIndex(
                name: "IX_BarnamehHaftegiOstads_UserIdModirGrooh",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "NoeMarkaz",
                table: "Markazes");

            migrationBuilder.DropColumn(
                name: "TedadVahedMovazafi",
                table: "ElmiTerms");

            migrationBuilder.DropColumn(
                name: "Vazeeat",
                table: "ElmiTerms");

            migrationBuilder.DropColumn(
                name: "RoleMarkazMoaven",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "RoleMarkazModirGrooh",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "TarikhElmi",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "TarikhMoaven",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "TarikhModirGrooh",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.RenameColumn(
                name: "RoleMarkazSabtKonandeh",
                table: "ElmiTerms",
                newName: "RoleIdSabtKonandeh");

            migrationBuilder.RenameColumn(
                name: "UserIdModirGrooh",
                table: "BarnamehHaftegiOstads",
                newName: "H");

            migrationBuilder.RenameColumn(
                name: "UserIdMoaven",
                table: "BarnamehHaftegiOstads",
                newName: "G");

            migrationBuilder.RenameColumn(
                name: "NazarModirGrooh",
                table: "BarnamehHaftegiOstads",
                newName: "F");

            migrationBuilder.RenameColumn(
                name: "NazarMoaven",
                table: "BarnamehHaftegiOstads",
                newName: "E");

            migrationBuilder.RenameColumn(
                name: "NararElmi",
                table: "BarnamehHaftegiOstads",
                newName: "D");

            migrationBuilder.AlterColumn<string>(
                name: "TedadSaatMovazafi",
                table: "ElmiTerms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AkharinVazeeat",
                table: "ElmiTerms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermCode",
                table: "ElmiTerms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OstadId",
                table: "BarnamehHaftegiOstads",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CodeTerm",
                table: "BarnamehHaftegiOstads",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "A",
                table: "BarnamehHaftegiOstads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "B",
                table: "BarnamehHaftegiOstads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "C",
                table: "BarnamehHaftegiOstads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Jozeiat",
                table: "BarnamehHaftegiOstads",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoozeHafteh",
                table: "BarnamehHaftegiOstads",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElmiTerm_UserId_TermCode",
                table: "ElmiTerms",
                columns: new[] { "UserId", "TermCode" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [TermCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ElmiTerms_RoleIdSabtKonandeh",
                table: "ElmiTerms",
                column: "RoleIdSabtKonandeh");

            migrationBuilder.CreateIndex(
                name: "IX_ElmiTerms_TermCode",
                table: "ElmiTerms",
                column: "TermCode");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad_CodeOstad_CodeTerm_MarkazId_RoozeHafteh",
                table: "BarnamehHaftegiOstads",
                columns: new[] { "OstadId", "CodeTerm", "MarkazId", "RoozeHafteh" },
                unique: true,
                filter: "[OstadId] IS NOT NULL AND [CodeTerm] IS NOT NULL AND [MarkazId] IS NOT NULL AND [RoozeHafteh] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ElmiTerms_AspNetRoles_RoleIdSabtKonandeh",
                table: "ElmiTerms",
                column: "RoleIdSabtKonandeh",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ElmiTerms_Terms_TermCode",
                table: "ElmiTerms",
                column: "TermCode",
                principalTable: "Terms",
                principalColumn: "CodeTerm");
        }
    }
}
