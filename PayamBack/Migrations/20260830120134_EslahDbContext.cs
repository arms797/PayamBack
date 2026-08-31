using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class EslahDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faaliats_FaaliatGroups_FaaliatGroupId1",
                table: "Faaliats");

            migrationBuilder.DropIndex(
                name: "IX_Faaliats_FaaliatGroupId1",
                table: "Faaliats");

            migrationBuilder.DropColumn(
                name: "FaaliatGroupId1",
                table: "Faaliats");

            migrationBuilder.CreateIndex(
                name: "IX_FaaliatGroup_IsActive",
                table: "FaaliatGroups",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FaaliatGroup_Title",
                table: "FaaliatGroups",
                column: "Title",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FaaliatGroup_IsActive",
                table: "FaaliatGroups");

            migrationBuilder.DropIndex(
                name: "IX_FaaliatGroup_Title",
                table: "FaaliatGroups");

            migrationBuilder.AddColumn<int>(
                name: "FaaliatGroupId1",
                table: "Faaliats",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faaliats_FaaliatGroupId1",
                table: "Faaliats",
                column: "FaaliatGroupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Faaliats_FaaliatGroups_FaaliatGroupId1",
                table: "Faaliats",
                column: "FaaliatGroupId1",
                principalTable: "FaaliatGroups",
                principalColumn: "Id");
        }
    }
}
