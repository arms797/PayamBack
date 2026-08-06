using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class ElmiTermEditModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ElmiTerm_Approve",
                table: "ElmiTerms");

            migrationBuilder.DropColumn(
                name: "Approve",
                table: "ElmiTerms");

            migrationBuilder.RenameColumn(
                name: "TedadVahedMovazafi",
                table: "ElmiTerms",
                newName: "TedadSaatMovazafi");

            migrationBuilder.AddColumn<int>(
                name: "ApproveStatus",
                table: "ElmiTerms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproveTozihat",
                table: "ElmiTerms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "ElmiTerms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "ElmiTerms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElmiTerm_Approve",
                table: "ElmiTerms",
                column: "ApproveStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ElmiTerm_Approve",
                table: "ElmiTerms");

            migrationBuilder.DropColumn(
                name: "ApproveStatus",
                table: "ElmiTerms");

            migrationBuilder.DropColumn(
                name: "ApproveTozihat",
                table: "ElmiTerms");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "ElmiTerms");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "ElmiTerms");

            migrationBuilder.RenameColumn(
                name: "TedadSaatMovazafi",
                table: "ElmiTerms",
                newName: "TedadVahedMovazafi");

            migrationBuilder.AddColumn<bool>(
                name: "Approve",
                table: "ElmiTerms",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElmiTerm_Approve",
                table: "ElmiTerms",
                column: "Approve");
        }
    }
}
