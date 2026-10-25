using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class AddAnyModelsToEdu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Telephon",
                table: "SakhtemanKelass");

            migrationBuilder.CreateTable(
                name: "HozoorElmi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OstadId = table.Column<int>(type: "int", nullable: false),
                    MarkazId = table.Column<int>(type: "int", nullable: false),
                    UserIdIn = table.Column<int>(type: "int", nullable: true),
                    UserIdOut = table.Column<int>(type: "int", nullable: true),
                    RoleMarkazIn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RoleMarkazOut = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TarikhIn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TarikhOut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TarikhSabtIn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TarikhSabtOut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TozihatSabtKonandeh = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TozihatElmi = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Madrak = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TarikhElmi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecordAsli = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsLock = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TozihatLock = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Hozoor = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HozoorElmi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HozoorElmi_AspNetUsers_UserIdIn",
                        column: x => x.UserIdIn,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HozoorElmi_AspNetUsers_UserIdOut",
                        column: x => x.UserIdOut,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HozoorElmi_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HozoorElmi_Ostads_OstadId",
                        column: x => x.OstadId,
                        principalTable: "Ostads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarkazReshteh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarkazId = table.Column<int>(type: "int", nullable: false),
                    ReshtehId = table.Column<int>(type: "int", nullable: false),
                    TedadDaneshjoo = table.Column<int>(type: "int", nullable: true),
                    Vazeeat = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkazReshteh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarkazReshteh_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarkazReshteh_Reshtehs_ReshtehId",
                        column: x => x.ReshtehId,
                        principalTable: "Reshtehs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Servici",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Onvan = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MarkazId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false, defaultValue: 4)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servici", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Servici_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KarbarReshteh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserRoleId = table.Column<int>(type: "int", nullable: false),
                    ReshtehId = table.Column<int>(type: "int", nullable: true),
                    ServiciId = table.Column<int>(type: "int", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false, defaultValue: 4),
                    MarkazId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KarbarReshteh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KarbarReshteh_AspNetUserRoles_AppUserRoleId",
                        column: x => x.AppUserRoleId,
                        principalTable: "AspNetUserRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KarbarReshteh_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KarbarReshteh_Reshtehs_ReshtehId",
                        column: x => x.ReshtehId,
                        principalTable: "Reshtehs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KarbarReshteh_Servici_ServiciId",
                        column: x => x.ServiciId,
                        principalTable: "Servici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiciDetailes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiciId = table.Column<int>(type: "int", nullable: false),
                    DarsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiciDetailes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiciDetailes_Dars_DarsId",
                        column: x => x.DarsId,
                        principalTable: "Dars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiciDetailes_Servici_ServiciId",
                        column: x => x.ServiciId,
                        principalTable: "Servici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reshteh_CodeReshteh_CodeMaghta_TermVorood_TermEamal",
                table: "Reshtehs",
                columns: new[] { "CodeReshte", "CodeMaghta", "TermVorood", "TermEamal" },
                unique: true,
                filter: "[CodeReshte] IS NOT NULL AND [CodeMaghta] IS NOT NULL AND [TermVorood] IS NOT NULL AND [TermEamal] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HozoorElmi_Hozoor",
                table: "HozoorElmi",
                column: "Hozoor");

            migrationBuilder.CreateIndex(
                name: "IX_HozoorElmi_IsLock",
                table: "HozoorElmi",
                column: "IsLock");

            migrationBuilder.CreateIndex(
                name: "IX_HozoorElmi_MarkazId",
                table: "HozoorElmi",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_HozoorElmi_Ostad_Markaz",
                table: "HozoorElmi",
                columns: new[] { "OstadId", "MarkazId" });

            migrationBuilder.CreateIndex(
                name: "IX_HozoorElmi_TarikhIn",
                table: "HozoorElmi",
                column: "TarikhIn");

            migrationBuilder.CreateIndex(
                name: "IX_HozoorElmi_UserIdIn",
                table: "HozoorElmi",
                column: "UserIdIn");

            migrationBuilder.CreateIndex(
                name: "IX_HozoorElmi_UserIdOut",
                table: "HozoorElmi",
                column: "UserIdOut");

            migrationBuilder.CreateIndex(
                name: "IX_KarbarReshteh_AppUserRoleId",
                table: "KarbarReshteh",
                column: "AppUserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_KarbarReshteh_MarkazId",
                table: "KarbarReshteh",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_KarbarReshteh_ReshtehId",
                table: "KarbarReshteh",
                column: "ReshtehId");

            migrationBuilder.CreateIndex(
                name: "IX_KarbarReshteh_ServiciId",
                table: "KarbarReshteh",
                column: "ServiciId");

            migrationBuilder.CreateIndex(
                name: "IX_KarbarReshteh_User_Markaz",
                table: "KarbarReshteh",
                columns: new[] { "AppUserRoleId", "MarkazId" });

            migrationBuilder.CreateIndex(
                name: "IX_MarkazReshteh_ReshtehId",
                table: "MarkazReshteh",
                column: "ReshtehId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkazReshteh_Vazeeat",
                table: "MarkazReshteh",
                column: "Vazeeat");

            migrationBuilder.CreateIndex(
                name: "UX_MarkazReshteh_Markaz_Reshteh",
                table: "MarkazReshteh",
                columns: new[] { "MarkazId", "ReshtehId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servici_Level",
                table: "Servici",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Servici_Markaz_Onvan",
                table: "Servici",
                columns: new[] { "MarkazId", "Onvan" });

            migrationBuilder.CreateIndex(
                name: "IX_Servici_MarkazId",
                table: "Servici",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiciDetailes_DarsId",
                table: "ServiciDetailes",
                column: "DarsId");

            migrationBuilder.CreateIndex(
                name: "UX_ServiciDetailes_Servici_Dars",
                table: "ServiciDetailes",
                columns: new[] { "ServiciId", "DarsId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HozoorElmi");

            migrationBuilder.DropTable(
                name: "KarbarReshteh");

            migrationBuilder.DropTable(
                name: "MarkazReshteh");

            migrationBuilder.DropTable(
                name: "ServiciDetailes");

            migrationBuilder.DropTable(
                name: "Servici");

            migrationBuilder.DropIndex(
                name: "IX_Reshteh_CodeReshteh_CodeMaghta_TermVorood_TermEamal",
                table: "Reshtehs");

            migrationBuilder.AddColumn<string>(
                name: "Telephon",
                table: "SakhtemanKelass",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
