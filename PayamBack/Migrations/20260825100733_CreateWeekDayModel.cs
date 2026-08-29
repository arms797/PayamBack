using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class CreateWeekDayModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TermJari",
                table: "Terms",
                newName: "TermJariShoroo");

            migrationBuilder.AddColumn<bool>(
                name: "IsHaftegiRequired",
                table: "Terms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TermJariPayan",
                table: "Terms",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ModirGrooh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserRoleId = table.Column<int>(type: "int", nullable: false),
                    GrooheAmoozeshiId = table.Column<int>(type: "int", nullable: false),
                    Vazeeat = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModirGrooh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModirGrooh_AspNetUserRoles_AppUserRoleId",
                        column: x => x.AppUserRoleId,
                        principalTable: "AspNetUserRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ModirGrooh_GrooheAmoozeshis_GrooheAmoozeshiId",
                        column: x => x.GrooheAmoozeshiId,
                        principalTable: "GrooheAmoozeshis",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WeekDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsHoliday = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeekDays", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModirGrooh_AppUserRole_Groohe",
                table: "ModirGrooh",
                columns: new[] { "AppUserRoleId", "GrooheAmoozeshiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModirGrooh_GrooheId",
                table: "ModirGrooh",
                column: "GrooheAmoozeshiId");

            migrationBuilder.CreateIndex(
                name: "IX_ModirGrooh_Vazeeat",
                table: "ModirGrooh",
                column: "Vazeeat");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModirGrooh");

            migrationBuilder.DropTable(
                name: "WeekDays");

            migrationBuilder.DropColumn(
                name: "IsHaftegiRequired",
                table: "Terms");

            migrationBuilder.DropColumn(
                name: "TermJariPayan",
                table: "Terms");

            migrationBuilder.RenameColumn(
                name: "TermJariShoroo",
                table: "Terms",
                newName: "TermJari");
        }
    }
}
