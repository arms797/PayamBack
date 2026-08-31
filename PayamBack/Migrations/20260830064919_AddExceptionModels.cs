using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class AddExceptionModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HaftegiExceptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TermCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OstanCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DayCode = table.Column<int>(type: "int", nullable: true),
                    HourCode = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    NoeHamkariMask = table.Column<int>(type: "int", nullable: true),
                    FaaliatIds = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HaftegiExceptions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HaftegiExceptions");
        }
    }
}
