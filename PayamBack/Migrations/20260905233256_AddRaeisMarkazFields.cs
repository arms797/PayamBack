using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class AddRaeisMarkazFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxSaatDarEdari",
                table: "FaaliatGroups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinSaatDarEdari",
                table: "FaaliatGroups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NazarRaeisMarkaz",
                table: "BarnamehHaftegiOstads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleMarkazRaeisMarkaz",
                table: "BarnamehHaftegiOstads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TarikhRaeisMarkaz",
                table: "BarnamehHaftegiOstads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserIdRaeisMarkaz",
                table: "BarnamehHaftegiOstads",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstads_UserIdRaeisMarkaz",
                table: "BarnamehHaftegiOstads",
                column: "UserIdRaeisMarkaz");

            migrationBuilder.AddForeignKey(
                name: "FK_BarnamehHaftegiOstads_AspNetUsers_UserIdRaeisMarkaz",
                table: "BarnamehHaftegiOstads",
                column: "UserIdRaeisMarkaz",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BarnamehHaftegiOstads_AspNetUsers_UserIdRaeisMarkaz",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropIndex(
                name: "IX_BarnamehHaftegiOstads_UserIdRaeisMarkaz",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "MaxSaatDarEdari",
                table: "FaaliatGroups");

            migrationBuilder.DropColumn(
                name: "MinSaatDarEdari",
                table: "FaaliatGroups");

            migrationBuilder.DropColumn(
                name: "NazarRaeisMarkaz",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "RoleMarkazRaeisMarkaz",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "TarikhRaeisMarkaz",
                table: "BarnamehHaftegiOstads");

            migrationBuilder.DropColumn(
                name: "UserIdRaeisMarkaz",
                table: "BarnamehHaftegiOstads");
        }
    }
}
