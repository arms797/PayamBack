using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalFieldsToOstadMadrak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "OstadMadraks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByRoleInfo",
                table: "OstadMadraks",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "OstadMadraks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OstadMadraks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByRoleInfo",
                table: "OstadMadraks",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "OstadMadraks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "OstadMadraks",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OstadMadraks_ApprovedByUserId",
                table: "OstadMadraks",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OstadMadraks_CreatedByUserId",
                table: "OstadMadraks",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OstadMadraks_AspNetUsers_ApprovedByUserId",
                table: "OstadMadraks",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OstadMadraks_AspNetUsers_CreatedByUserId",
                table: "OstadMadraks",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OstadMadraks_AspNetUsers_ApprovedByUserId",
                table: "OstadMadraks");

            migrationBuilder.DropForeignKey(
                name: "FK_OstadMadraks_AspNetUsers_CreatedByUserId",
                table: "OstadMadraks");

            migrationBuilder.DropIndex(
                name: "IX_OstadMadraks_ApprovedByUserId",
                table: "OstadMadraks");

            migrationBuilder.DropIndex(
                name: "IX_OstadMadraks_CreatedByUserId",
                table: "OstadMadraks");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "OstadMadraks");

            migrationBuilder.DropColumn(
                name: "ApprovedByRoleInfo",
                table: "OstadMadraks");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "OstadMadraks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OstadMadraks");

            migrationBuilder.DropColumn(
                name: "CreatedByRoleInfo",
                table: "OstadMadraks");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "OstadMadraks");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "OstadMadraks");
        }
    }
}
