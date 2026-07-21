using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class AddLevelMarkaz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OstadMadrak_GrooheAmoozeshis_GrooheAmoozeshiId",
                table: "OstadMadrak");

            migrationBuilder.DropForeignKey(
                name: "FK_OstadMadrak_Ostads_OstadId",
                table: "OstadMadrak");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OstadMadrak",
                table: "OstadMadrak");

            migrationBuilder.DropIndex(
                name: "IX_OstadMadrak_OstadId",
                table: "OstadMadrak");

            migrationBuilder.RenameTable(
                name: "OstadMadrak",
                newName: "OstadMadraks");

            migrationBuilder.RenameIndex(
                name: "IX_Ostads_MarkazId",
                table: "Ostads",
                newName: "IX_Ostad_MarkazId");

            migrationBuilder.RenameIndex(
                name: "IX_OstadMadrak_GrooheAmoozeshiId",
                table: "OstadMadraks",
                newName: "IX_OstadMadraks_GrooheAmoozeshiId");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Markazes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "AspNetRoles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OstadMadraks",
                table: "OstadMadraks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_OstadMadrak_OstadId_PishFarz",
                table: "OstadMadraks",
                columns: new[] { "OstadId", "PishFarz" });

            migrationBuilder.AddForeignKey(
                name: "FK_OstadMadraks_GrooheAmoozeshis_GrooheAmoozeshiId",
                table: "OstadMadraks",
                column: "GrooheAmoozeshiId",
                principalTable: "GrooheAmoozeshis",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OstadMadraks_Ostads_OstadId",
                table: "OstadMadraks",
                column: "OstadId",
                principalTable: "Ostads",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OstadMadraks_GrooheAmoozeshis_GrooheAmoozeshiId",
                table: "OstadMadraks");

            migrationBuilder.DropForeignKey(
                name: "FK_OstadMadraks_Ostads_OstadId",
                table: "OstadMadraks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OstadMadraks",
                table: "OstadMadraks");

            migrationBuilder.DropIndex(
                name: "IX_OstadMadrak_OstadId_PishFarz",
                table: "OstadMadraks");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Markazes");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "OstadMadraks",
                newName: "OstadMadrak");

            migrationBuilder.RenameIndex(
                name: "IX_Ostad_MarkazId",
                table: "Ostads",
                newName: "IX_Ostads_MarkazId");

            migrationBuilder.RenameIndex(
                name: "IX_OstadMadraks_GrooheAmoozeshiId",
                table: "OstadMadrak",
                newName: "IX_OstadMadrak_GrooheAmoozeshiId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OstadMadrak",
                table: "OstadMadrak",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_OstadMadrak_OstadId",
                table: "OstadMadrak",
                column: "OstadId");

            migrationBuilder.AddForeignKey(
                name: "FK_OstadMadrak_GrooheAmoozeshis_GrooheAmoozeshiId",
                table: "OstadMadrak",
                column: "GrooheAmoozeshiId",
                principalTable: "GrooheAmoozeshis",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OstadMadrak_Ostads_OstadId",
                table: "OstadMadrak",
                column: "OstadId",
                principalTable: "Ostads",
                principalColumn: "Id");
        }
    }
}
