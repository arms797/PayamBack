using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class editDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AdminId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DaneshjooId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_KarmandId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OstadId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AdminId",
                table: "AspNetUsers",
                column: "AdminId",
                unique: true,
                filter: "[AdminId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DaneshjooId",
                table: "AspNetUsers",
                column: "DaneshjooId",
                unique: true,
                filter: "[DaneshjooId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_KarmandId",
                table: "AspNetUsers",
                column: "KarmandId",
                unique: true,
                filter: "[KarmandId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OstadId",
                table: "AspNetUsers",
                column: "OstadId",
                unique: true,
                filter: "[OstadId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AdminId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DaneshjooId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_KarmandId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OstadId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AdminId",
                table: "AspNetUsers",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DaneshjooId",
                table: "AspNetUsers",
                column: "DaneshjooId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_KarmandId",
                table: "AspNetUsers",
                column: "KarmandId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OstadId",
                table: "AspNetUsers",
                column: "OstadId");
        }
    }
}
