using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class AddHamjavarModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElmiTerms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    TermCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserIdSabtKonandeh = table.Column<int>(type: "int", nullable: true),
                    RoleIdSabtKonandeh = table.Column<int>(type: "int", nullable: true),
                    AkharinVazeeat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsEjeari = table.Column<bool>(type: "bit", nullable: true),
                    OnvanEjraei = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FullTime = table.Column<bool>(type: "bit", nullable: true),
                    TedadVahedMovazafi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElmiTerms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElmiTerms_AspNetRoles_RoleIdSabtKonandeh",
                        column: x => x.RoleIdSabtKonandeh,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ElmiTerms_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ElmiTerms_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ElmiTerms_AspNetUsers_UserIdSabtKonandeh",
                        column: x => x.UserIdSabtKonandeh,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ElmiTerms_Terms_TermCode",
                        column: x => x.TermCode,
                        principalTable: "Terms",
                        principalColumn: "CodeTerm");
                });

            migrationBuilder.CreateTable(
                name: "Faaliats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Onvan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NoeAnjam = table.Column<int>(type: "int", nullable: true),
                    MinSaatDarEdari = table.Column<int>(type: "int", nullable: true),
                    MaxSaatDarEdari = table.Column<int>(type: "int", nullable: true),
                    MinSaatDarHafteh = table.Column<int>(type: "int", nullable: true),
                    MaxSaatDarHafteh = table.Column<int>(type: "int", nullable: true),
                    MinDayDarHafteh = table.Column<int>(type: "int", nullable: true),
                    MaxDayDarHafteh = table.Column<int>(type: "int", nullable: true),
                    IsMadove = table.Column<bool>(type: "bit", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Vazeeat = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faaliats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hamjavars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OstadId = table.Column<int>(type: "int", nullable: false),
                    TermCode = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    TedadVahedMahalKhedmat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TedadVahedHamjavar = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TedadVahedMajazi = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Dalil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShahrZendegi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadElmi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmaliatElmi = table.Column<int>(type: "int", nullable: true),
                    NazarElmi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TarikhErsalElmi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TarikhDaryaftRaeis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TozihatRaeis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadRaeis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmaliatRaeis = table.Column<int>(type: "int", nullable: true),
                    NazarRaeis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TarikhErsalRaeis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TarikhDaryaftKhadamat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TozihatKhadamat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadKhadamat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmaliatKhadamat = table.Column<int>(type: "int", nullable: true),
                    NazarKhadamat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TarikhErsalKhadamat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TarikhDaryaftMoaven = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TozihatMoaven = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadMoaven = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmaliatMoaven = table.Column<int>(type: "int", nullable: true),
                    NazarMoaven = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TarikhErsalMoaven = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AKharinBarrasi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AkharinTaghaza = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hamjavars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hamjavars_Ostads_OstadId",
                        column: x => x.OstadId,
                        principalTable: "Ostads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Hamjavars_Terms_TermCode",
                        column: x => x.TermCode,
                        principalTable: "Terms",
                        principalColumn: "CodeTerm");
                });

            migrationBuilder.CreateTable(
                name: "Hamjavar1s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HamjavarId = table.Column<int>(type: "int", nullable: false),
                    UserIdSabtKonandeh = table.Column<int>(type: "int", nullable: true),
                    RoleIdSabtKonandeh = table.Column<int>(type: "int", nullable: true),
                    MarkazId = table.Column<int>(type: "int", nullable: true),
                    InOstan = table.Column<bool>(type: "bit", nullable: true),
                    FaaliatId = table.Column<int>(type: "int", nullable: true),
                    TedadRoozElmi = table.Column<int>(type: "int", nullable: true),
                    TedadRoozRaeis = table.Column<int>(type: "int", nullable: true),
                    TedadRoozKhadamat = table.Column<int>(type: "int", nullable: true),
                    TedadRoozMoaven = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hamjavar1s", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hamjavar1s_AspNetRoles_RoleIdSabtKonandeh",
                        column: x => x.RoleIdSabtKonandeh,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Hamjavar1s_AspNetUsers_UserIdSabtKonandeh",
                        column: x => x.UserIdSabtKonandeh,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Hamjavar1s_Faaliats_FaaliatId",
                        column: x => x.FaaliatId,
                        principalTable: "Faaliats",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Hamjavar1s_Hamjavars_HamjavarId",
                        column: x => x.HamjavarId,
                        principalTable: "Hamjavars",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Hamjavar1s_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElmiTerm_Approve",
                table: "ElmiTerms",
                column: "Approve");

            migrationBuilder.CreateIndex(
                name: "IX_ElmiTerm_UserId_TermCode",
                table: "ElmiTerms",
                columns: new[] { "UserId", "TermCode" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [TermCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ElmiTerms_ApprovedByUserId",
                table: "ElmiTerms",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ElmiTerms_RoleIdSabtKonandeh",
                table: "ElmiTerms",
                column: "RoleIdSabtKonandeh");

            migrationBuilder.CreateIndex(
                name: "IX_ElmiTerms_TermCode",
                table: "ElmiTerms",
                column: "TermCode");

            migrationBuilder.CreateIndex(
                name: "IX_ElmiTerms_UserIdSabtKonandeh",
                table: "ElmiTerms",
                column: "UserIdSabtKonandeh");

            migrationBuilder.CreateIndex(
                name: "IX_Faaliat_Onvan",
                table: "Faaliats",
                column: "Onvan",
                unique: true,
                filter: "[Onvan] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Faaliat_Vazeeat",
                table: "Faaliats",
                column: "Vazeeat");

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar1_FaaliatId",
                table: "Hamjavar1s",
                column: "FaaliatId");

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar1_HamjavarId",
                table: "Hamjavar1s",
                column: "HamjavarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar1s_MarkazId",
                table: "Hamjavar1s",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar1s_RoleIdSabtKonandeh",
                table: "Hamjavar1s",
                column: "RoleIdSabtKonandeh");

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar1s_UserIdSabtKonandeh",
                table: "Hamjavar1s",
                column: "UserIdSabtKonandeh");

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar_AkharinTaghaza",
                table: "Hamjavars",
                column: "AkharinTaghaza");

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar_OstadId",
                table: "Hamjavars",
                column: "OstadId");

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar_TermCode",
                table: "Hamjavars",
                column: "TermCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElmiTerms");

            migrationBuilder.DropTable(
                name: "Hamjavar1s");

            migrationBuilder.DropTable(
                name: "Faaliats");

            migrationBuilder.DropTable(
                name: "Hamjavars");
        }
    }
}
