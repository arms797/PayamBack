using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class addModelFaaliatGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FaaliatGroupId",
                table: "Faaliats",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaaliatGroupId1",
                table: "Faaliats",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FaaliatGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinSaatDarHafteh = table.Column<int>(type: "int", nullable: true),
                    MaxSaatDarHafteh = table.Column<int>(type: "int", nullable: true),
                    MinDayDarHafteh = table.Column<int>(type: "int", nullable: true),
                    MaxDayDarHafteh = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaaliatGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Faaliats_FaaliatGroupId",
                table: "Faaliats",
                column: "FaaliatGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Faaliats_FaaliatGroupId1",
                table: "Faaliats",
                column: "FaaliatGroupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Faaliats_FaaliatGroups_FaaliatGroupId",
                table: "Faaliats",
                column: "FaaliatGroupId",
                principalTable: "FaaliatGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Faaliats_FaaliatGroups_FaaliatGroupId1",
                table: "Faaliats",
                column: "FaaliatGroupId1",
                principalTable: "FaaliatGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faaliats_FaaliatGroups_FaaliatGroupId",
                table: "Faaliats");

            migrationBuilder.DropForeignKey(
                name: "FK_Faaliats_FaaliatGroups_FaaliatGroupId1",
                table: "Faaliats");

            migrationBuilder.DropTable(
                name: "FaaliatGroups");

            migrationBuilder.DropIndex(
                name: "IX_Faaliats_FaaliatGroupId",
                table: "Faaliats");

            migrationBuilder.DropIndex(
                name: "IX_Faaliats_FaaliatGroupId1",
                table: "Faaliats");

            migrationBuilder.DropColumn(
                name: "FaaliatGroupId",
                table: "Faaliats");

            migrationBuilder.DropColumn(
                name: "FaaliatGroupId1",
                table: "Faaliats");
        }
    }
}
