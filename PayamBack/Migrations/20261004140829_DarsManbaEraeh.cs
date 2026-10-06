using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class DarsManbaEraeh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoozTerm",
                table: "TaghvimTermis",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Dars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeDars = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NaamDars = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VahedTeori = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    VahedAmali = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    SaatTeoriOrginal = table.Column<int>(type: "int", nullable: true),
                    SaatAmaliOrginal = table.Column<int>(type: "int", nullable: true),
                    SaatTeori = table.Column<int>(type: "int", nullable: true),
                    SaatAmali = table.Column<int>(type: "int", nullable: true),
                    TermAkhz = table.Column<int>(type: "int", nullable: true),
                    NoeDars = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NoeAzmoon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReshtehId = table.Column<int>(type: "int", nullable: true),
                    Zarfiat = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dars_Reshtehs_ReshtehId",
                        column: x => x.ReshtehId,
                        principalTable: "Reshtehs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SakhtemanKelass",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarkazId = table.Column<int>(type: "int", nullable: false),
                    CodeSakhteman = table.Column<int>(type: "int", nullable: false),
                    NaamSakhteman = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CodeClass = table.Column<int>(type: "int", nullable: false),
                    NammClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NoeClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: false),
                    Zarfiat = table.Column<int>(type: "int", nullable: true),
                    Tabagheh = table.Column<int>(type: "int", nullable: true),
                    ZarfiatEmtahani = table.Column<int>(type: "int", nullable: true),
                    TedadWhiteboard = table.Column<int>(type: "int", nullable: true),
                    Projector = table.Column<bool>(type: "bit", nullable: false),
                    Emkanat = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tozihat = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Telephon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SakhtemanKelass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SakhtemanKelass_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DarsEraeh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeTerm = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MarkazId = table.Column<int>(type: "int", nullable: false),
                    ReshtehId = table.Column<int>(type: "int", nullable: false),
                    DarsId = table.Column<int>(type: "int", nullable: false),
                    CodeDars = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Grooh = table.Column<int>(type: "int", nullable: false),
                    NoeTadris = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserIdMojavezDahandeh = table.Column<int>(type: "int", nullable: true),
                    NaghshMarkazMojavezDahandeh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TarikheEraheh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Jensiat = table.Column<int>(type: "int", nullable: true),
                    VazeeyatDars = table.Column<bool>(type: "bit", nullable: false),
                    NahvehEraehDars = table.Column<int>(type: "int", nullable: true),
                    EmkanAkhzSayerMarakez = table.Column<bool>(type: "bit", nullable: false),
                    EmkanLinkBeSayerMarakez = table.Column<bool>(type: "bit", nullable: false),
                    ErtebatDehiId = table.Column<int>(type: "int", nullable: true),
                    BarnamehRizi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SabtNami = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DarsEraeh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DarsEraeh_DarsEraeh_ErtebatDehiId",
                        column: x => x.ErtebatDehiId,
                        principalTable: "DarsEraeh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DarsEraeh_Dars_DarsId",
                        column: x => x.DarsId,
                        principalTable: "Dars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DarsEraeh_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DarsEraeh_Reshtehs_ReshtehId",
                        column: x => x.ReshtehId,
                        principalTable: "Reshtehs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManbaDars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DarsId = table.Column<int>(type: "int", nullable: false),
                    ShomareManba = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NoeManba = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Onvan = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Nevisandeh = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Motarjem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SalEnteshar = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SalEntesharMiladi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Shabak = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Nasher = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NobateChap = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Vazeeyat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CodePeyvast = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SharhPeyvast = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TermUpdate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManbaDars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManbaDars_Dars_DarsId",
                        column: x => x.DarsId,
                        principalTable: "Dars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DarsEraehOstad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OstadId = table.Column<int>(type: "int", nullable: false),
                    DarsEraehId = table.Column<int>(type: "int", nullable: false),
                    Asli = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DarsEraehOstad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DarsEraehOstad_DarsEraeh_DarsEraehId",
                        column: x => x.DarsEraehId,
                        principalTable: "DarsEraeh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DarsEraehOstad_Ostads_OstadId",
                        column: x => x.OstadId,
                        principalTable: "Ostads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dars_CodeDars_ReshtehId_Unique",
                table: "Dars",
                columns: new[] { "CodeDars", "ReshtehId" },
                unique: true,
                filter: "[ReshtehId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Dars_NaamDars",
                table: "Dars",
                column: "NaamDars");

            migrationBuilder.CreateIndex(
                name: "IX_Dars_Reshteh_Term",
                table: "Dars",
                columns: new[] { "ReshtehId", "TermAkhz" });

            migrationBuilder.CreateIndex(
                name: "IX_Dars_ReshtehId",
                table: "Dars",
                column: "ReshtehId");

            migrationBuilder.CreateIndex(
                name: "IX_Dars_TermAkhz",
                table: "Dars",
                column: "TermAkhz");

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_DarsId",
                table: "DarsEraeh",
                column: "DarsId");

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_ErtebatDehiId",
                table: "DarsEraeh",
                column: "ErtebatDehiId");

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_MarkazId",
                table: "DarsEraeh",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_ReshtehId",
                table: "DarsEraeh",
                column: "ReshtehId");

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_Term_BarnamehRizi",
                table: "DarsEraeh",
                columns: new[] { "CodeTerm", "BarnamehRizi" });

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_Term_CodeDars",
                table: "DarsEraeh",
                columns: new[] { "CodeTerm", "CodeDars" });

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_Term_CodeDars_Grooh",
                table: "DarsEraeh",
                columns: new[] { "CodeTerm", "CodeDars", "Grooh" });

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_Term_Dars",
                table: "DarsEraeh",
                columns: new[] { "CodeTerm", "DarsId" });

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_Term_Markaz",
                table: "DarsEraeh",
                columns: new[] { "CodeTerm", "MarkazId" });

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_Term_Markaz_Reshteh",
                table: "DarsEraeh",
                columns: new[] { "CodeTerm", "MarkazId", "ReshtehId" });

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_Term_Markaz_Vazeeyat",
                table: "DarsEraeh",
                columns: new[] { "CodeTerm", "MarkazId", "VazeeyatDars" });

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_Term_Reshteh",
                table: "DarsEraeh",
                columns: new[] { "CodeTerm", "ReshtehId" });

            migrationBuilder.CreateIndex(
                name: "IX_DarsEraeh_Unique_Grooh",
                table: "DarsEraeh",
                columns: new[] { "CodeTerm", "MarkazId", "DarsId", "Grooh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OstadDars_Dars_Asli",
                table: "DarsEraehOstad",
                columns: new[] { "DarsEraehId", "Asli" });

            migrationBuilder.CreateIndex(
                name: "IX_OstadDars_DarsEraehId",
                table: "DarsEraehOstad",
                column: "DarsEraehId");

            migrationBuilder.CreateIndex(
                name: "IX_OstadDars_OstadId",
                table: "DarsEraehOstad",
                column: "OstadId");

            migrationBuilder.CreateIndex(
                name: "IX_OstadDars_Unique_Dars_Ostad",
                table: "DarsEraehOstad",
                columns: new[] { "DarsEraehId", "OstadId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManbaDars_Dars_Shomare",
                table: "ManbaDars",
                columns: new[] { "DarsId", "ShomareManba" });

            migrationBuilder.CreateIndex(
                name: "IX_ManbaDars_DarsId",
                table: "ManbaDars",
                column: "DarsId");

            migrationBuilder.CreateIndex(
                name: "IX_ManbaDars_Onvan",
                table: "ManbaDars",
                column: "Onvan");

            migrationBuilder.CreateIndex(
                name: "IX_ManbaDars_Vazeeyat",
                table: "ManbaDars",
                column: "Vazeeyat");

            migrationBuilder.CreateIndex(
                name: "IX_SakhtemanKelass_Markaz_CodeSakhteman",
                table: "SakhtemanKelass",
                columns: new[] { "MarkazId", "CodeSakhteman" });

            migrationBuilder.CreateIndex(
                name: "IX_SakhtemanKelass_Markaz_Sakhteman_Class_Unique",
                table: "SakhtemanKelass",
                columns: new[] { "MarkazId", "CodeSakhteman", "CodeClass" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SakhtemanKelass_MarkazId",
                table: "SakhtemanKelass",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_SakhtemanKelass_Vazeeyat",
                table: "SakhtemanKelass",
                column: "Vazeeyat");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DarsEraehOstad");

            migrationBuilder.DropTable(
                name: "ManbaDars");

            migrationBuilder.DropTable(
                name: "SakhtemanKelass");

            migrationBuilder.DropTable(
                name: "DarsEraeh");

            migrationBuilder.DropTable(
                name: "Dars");

            migrationBuilder.DropColumn(
                name: "RoozTerm",
                table: "TaghvimTermis");
        }
    }
}
