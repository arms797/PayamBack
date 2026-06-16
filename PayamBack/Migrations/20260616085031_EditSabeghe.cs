using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class EditSabeghe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "User",
                table: "Sabeghes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ZamanTagir",
                table: "Sabeghes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ZamanLogin",
                table: "Sabeghes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "TozihTagirat",
                table: "Sabeghes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Table",
                table: "Sabeghes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "RoozHafte",
                table: "Sabeghes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "IdRecordTagirDahande",
                table: "Sabeghes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "AppUserId",
                table: "Sabeghes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Sabeghes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sabeghes_AppUserId",
                table: "Sabeghes",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sabeghes_UserId",
                table: "Sabeghes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sabeghes_AspNetUsers_AppUserId",
                table: "Sabeghes",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sabeghes_AspNetUsers_UserId",
                table: "Sabeghes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sabeghes_AspNetUsers_AppUserId",
                table: "Sabeghes");

            migrationBuilder.DropForeignKey(
                name: "FK_Sabeghes_AspNetUsers_UserId",
                table: "Sabeghes");

            migrationBuilder.DropIndex(
                name: "IX_Sabeghes_AppUserId",
                table: "Sabeghes");

            migrationBuilder.DropIndex(
                name: "IX_Sabeghes_UserId",
                table: "Sabeghes");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Sabeghes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Sabeghes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ZamanTagir",
                table: "Sabeghes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ZamanLogin",
                table: "Sabeghes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TozihTagirat",
                table: "Sabeghes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Table",
                table: "Sabeghes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RoozHafte",
                table: "Sabeghes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdRecordTagirDahande",
                table: "Sabeghes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "User",
                table: "Sabeghes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
