using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class Update_MarkazReshteh_UniqueIndex_With_Level : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_MarkazReshteh_Markaz_Reshteh",
                table: "MarkazReshteh");

            migrationBuilder.CreateIndex(
                name: "UX_MarkazReshteh_Markaz_Reshteh_Level",
                table: "MarkazReshteh",
                columns: new[] { "MarkazId", "ReshtehId", "Level" },
                unique: true,
                filter: "[Level] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_MarkazReshteh_Markaz_Reshteh_Level",
                table: "MarkazReshteh");

            migrationBuilder.CreateIndex(
                name: "UX_MarkazReshteh_Markaz_Reshteh",
                table: "MarkazReshteh",
                columns: new[] { "MarkazId", "ReshtehId" },
                unique: true);
        }
    }
}
