using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class InitDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeRole = table.Column<int>(type: "int", nullable: true),
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: true),
                    Emza = table.Column<bool>(type: "bit", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrooheAmoozeshis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeDaneshkade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NaamDaneshkadeh = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CodeGrooheAmoozeshi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OnvanGrooheAmoozeshi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrooheAmoozeshis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Markazes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeOstan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NaamOstan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CodeMarkaz = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NaamMarkaz = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VahedMarkaz = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Nahiyeh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MahalMarkaz = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CodePosti = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WebSite = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: true),
                    Dakheli = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markazes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Path = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PermissionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: true),
                    Vazeeat = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Menus_Menus_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Menus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MoshakhasatAdmins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeMelli = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Naam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NaameKhanevadeghi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TelefonMostaghim = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TelefonGhayreMostaghim = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TelefonDakheli = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Mobile2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CodePosti = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoshakhasatAdmins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Resource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaatBargozariKelashas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OnvanSaat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CodeSaat = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    SaatShoroo = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    SaatPayan = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Hozoori = table.Column<bool>(type: "bit", nullable: true),
                    Majazi = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaatBargozariKelashas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaghvimTermis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeTerm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tarikh = table.Column<DateOnly>(type: "date", nullable: true),
                    CodeRooz = table.Column<int>(type: "int", nullable: true),
                    RoozHafteh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CodeHafteh = table.Column<int>(type: "int", nullable: true),
                    Hafteh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CodeSaateTatili = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OnvanMonasebat = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Tozihat = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaghvimTermis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Terms",
                columns: table => new
                {
                    CodeTerm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OnvanTerm = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TermJari = table.Column<DateOnly>(type: "date", nullable: true),
                    TarikheDastrasi = table.Column<DateOnly>(type: "date", nullable: true),
                    TarikheEraeeDars = table.Column<DateOnly>(type: "date", nullable: true),
                    TarikhePayanDars = table.Column<DateOnly>(type: "date", nullable: true),
                    TarikheShorooClass = table.Column<DateOnly>(type: "date", nullable: true),
                    TarikhePayanClass = table.Column<DateOnly>(type: "date", nullable: true),
                    TarikheShorooMojavezMarakez = table.Column<DateOnly>(type: "date", nullable: true),
                    TarikhePayanMojavezMarakez = table.Column<DateOnly>(type: "date", nullable: true),
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terms", x => x.CodeTerm);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reshtehs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeMaghta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Maghta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GrooheAmoozeshiId = table.Column<int>(type: "int", nullable: true),
                    CodeReshteDoRaghami = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    CodeReshte = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OnvanReshte = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TermVorood = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TermEamal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reshtehs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reshtehs_GrooheAmoozeshis_GrooheAmoozeshiId",
                        column: x => x.GrooheAmoozeshiId,
                        principalTable: "GrooheAmoozeshis",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Karmands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeMelli = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Naam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NaameKhanevadeghi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MarkazId = table.Column<int>(type: "int", nullable: true),
                    MarkazAsliId = table.Column<int>(type: "int", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Mobile2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TelefonMostaghim = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TelefonGhayreMostaghim = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TelefonDakheli = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Emza = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Karmands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Karmands_Markazes_MarkazAsliId",
                        column: x => x.MarkazAsliId,
                        principalTable: "Markazes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Karmands_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Ostads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarkazId = table.Column<int>(type: "int", nullable: true),
                    MarkazAsliId = table.Column<int>(type: "int", nullable: true),
                    CodeOstadi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NaamKhanevadegi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Naam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Jens = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NaamPedar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TarikhTavalod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShomareShenasname = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShomareMelli = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Mobile2 = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    MartabeElmi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SazmanMarboote = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MahalEshteghal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Emza = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Vazeeat = table.Column<bool>(type: "bit", nullable: true),
                    NoeHamkari = table.Column<int>(type: "int", nullable: true),
                    NoeBimeh = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ShomarehBimeh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ostads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ostads_Markazes_MarkazAsliId",
                        column: x => x.MarkazAsliId,
                        principalTable: "Markazes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ostads_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: true),
                    PermissionId = table.Column<int>(type: "int", nullable: true),
                    Vazeeat = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Daneshjoos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarkazId = table.Column<int>(type: "int", nullable: true),
                    MarkazAzmoonId = table.Column<int>(type: "int", nullable: true),
                    MarkazTermiId = table.Column<int>(type: "int", nullable: true),
                    ReshtehId = table.Column<int>(type: "int", nullable: true),
                    ShomareDaneshjooee = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NaamKhanevadegi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Naam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Jens = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Naampedar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShomareMelli = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ShomareShenasname = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShomareGozarnameYaKartHoviyat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShenasayeFaragirAtbaaKhareji = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MahalSodoor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TarikhTavalod = table.Column<DateOnly>(type: "date", nullable: true),
                    TermVorood = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShomareParvande = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VazeeyatParvande = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChapDast = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ShomareDavtalabi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CodeReshteMahalGhabooliSanjesh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShomareSanjesh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Daneshjoos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Daneshjoos_Markazes_MarkazAzmoonId",
                        column: x => x.MarkazAzmoonId,
                        principalTable: "Markazes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Daneshjoos_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Daneshjoos_Markazes_MarkazTermiId",
                        column: x => x.MarkazTermiId,
                        principalTable: "Markazes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Daneshjoos_Reshtehs_ReshtehId",
                        column: x => x.ReshtehId,
                        principalTable: "Reshtehs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BarnamehHaftegiOstads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OstadId = table.Column<int>(type: "int", nullable: true),
                    MarkazId = table.Column<int>(type: "int", nullable: true),
                    CodeTerm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RoozeHafteh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    A = table.Column<int>(type: "int", nullable: true),
                    B = table.Column<int>(type: "int", nullable: true),
                    C = table.Column<int>(type: "int", nullable: true),
                    D = table.Column<int>(type: "int", nullable: true),
                    E = table.Column<int>(type: "int", nullable: true),
                    F = table.Column<int>(type: "int", nullable: true),
                    G = table.Column<int>(type: "int", nullable: true),
                    H = table.Column<int>(type: "int", nullable: true),
                    Jozeiat = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarnamehHaftegiOstads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BarnamehHaftegiOstads_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BarnamehHaftegiOstads_Ostads_OstadId",
                        column: x => x.OstadId,
                        principalTable: "Ostads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BarnamehHaftegiOstads_Terms_CodeTerm",
                        column: x => x.CodeTerm,
                        principalTable: "Terms",
                        principalColumn: "CodeTerm");
                });

            migrationBuilder.CreateTable(
                name: "BarnamehTermiOstads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OstadId = table.Column<int>(type: "int", nullable: true),
                    MarkazId = table.Column<int>(type: "int", nullable: true),
                    CodeTerm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RoozeHafteh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tarikh = table.Column<DateOnly>(type: "date", nullable: true),
                    A = table.Column<int>(type: "int", nullable: true),
                    B = table.Column<int>(type: "int", nullable: true),
                    C = table.Column<int>(type: "int", nullable: true),
                    D = table.Column<int>(type: "int", nullable: true),
                    E = table.Column<int>(type: "int", nullable: true),
                    F = table.Column<int>(type: "int", nullable: true),
                    G = table.Column<int>(type: "int", nullable: true),
                    H = table.Column<int>(type: "int", nullable: true),
                    TA = table.Column<bool>(type: "bit", nullable: true),
                    TB = table.Column<bool>(type: "bit", nullable: true),
                    TC = table.Column<bool>(type: "bit", nullable: true),
                    TD = table.Column<bool>(type: "bit", nullable: true),
                    TE = table.Column<bool>(type: "bit", nullable: true),
                    TF = table.Column<bool>(type: "bit", nullable: true),
                    TG = table.Column<bool>(type: "bit", nullable: true),
                    TH = table.Column<bool>(type: "bit", nullable: true),
                    Vazeeat = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarnamehTermiOstads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BarnamehTermiOstads_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BarnamehTermiOstads_Ostads_OstadId",
                        column: x => x.OstadId,
                        principalTable: "Ostads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BarnamehTermiOstads_Terms_CodeTerm",
                        column: x => x.CodeTerm,
                        principalTable: "Terms",
                        principalColumn: "CodeTerm");
                });

            migrationBuilder.CreateTable(
                name: "OstadMadrak",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OstadId = table.Column<int>(type: "int", nullable: true),
                    GrooheAmoozeshiId = table.Column<int>(type: "int", nullable: true),
                    Reshteh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Grayesh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Maghta = table.Column<int>(type: "int", nullable: true),
                    PishFarz = table.Column<bool>(type: "bit", nullable: true),
                    MahalAkhz = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TasvirMadrak = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OstadMadrak", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OstadMadrak_GrooheAmoozeshis_GrooheAmoozeshiId",
                        column: x => x.GrooheAmoozeshiId,
                        principalTable: "GrooheAmoozeshis",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OstadMadrak_Ostads_OstadId",
                        column: x => x.OstadId,
                        principalTable: "Ostads",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OstadId = table.Column<int>(type: "int", nullable: true),
                    KarmandId = table.Column<int>(type: "int", nullable: true),
                    DaneshjooId = table.Column<int>(type: "int", nullable: true),
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: true),
                    VazeeyatMovaghat = table.Column<bool>(type: "bit", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Daneshjoos_DaneshjooId",
                        column: x => x.DaneshjooId,
                        principalTable: "Daneshjoos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Karmands_KarmandId",
                        column: x => x.KarmandId,
                        principalTable: "Karmands",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Ostads_OstadId",
                        column: x => x.OstadId,
                        principalTable: "Ostads",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    MarkazId = table.Column<int>(type: "int", nullable: true),
                    RolePishFarz = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Markazes_MarkazId",
                        column: x => x.MarkazId,
                        principalTable: "Markazes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sabeghes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Dastgah = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Moroorgar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ZamanLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Table = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IdRecordTagirDahande = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RoozHafte = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ZamanTagir = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TozihTagirat = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ZamanLogOut = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sabeghes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sabeghes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRole_UserId_RoleId_MarkazId",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId", "MarkazId" },
                unique: true,
                filter: "[MarkazId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_MarkazId",
                table: "AspNetUserRoles",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

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

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstad_CodeOstad_CodeTerm_MarkazId_RoozeHafteh",
                table: "BarnamehHaftegiOstads",
                columns: new[] { "OstadId", "CodeTerm", "MarkazId", "RoozeHafteh" },
                unique: true,
                filter: "[OstadId] IS NOT NULL AND [CodeTerm] IS NOT NULL AND [MarkazId] IS NOT NULL AND [RoozeHafteh] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstads_CodeTerm",
                table: "BarnamehHaftegiOstads",
                column: "CodeTerm");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstads_MarkazId",
                table: "BarnamehHaftegiOstads",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehTermiOstad_CodeOstad_CodeTerm_MarkazId_Tarikh",
                table: "BarnamehTermiOstads",
                columns: new[] { "OstadId", "CodeTerm", "MarkazId", "Tarikh" },
                unique: true,
                filter: "[OstadId] IS NOT NULL AND [CodeTerm] IS NOT NULL AND [MarkazId] IS NOT NULL AND [Tarikh] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehTermiOstads_CodeTerm",
                table: "BarnamehTermiOstads",
                column: "CodeTerm");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehTermiOstads_MarkazId",
                table: "BarnamehTermiOstads",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_Daneshjoos_MarkazAzmoonId",
                table: "Daneshjoos",
                column: "MarkazAzmoonId");

            migrationBuilder.CreateIndex(
                name: "IX_Daneshjoos_MarkazId",
                table: "Daneshjoos",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_Daneshjoos_MarkazTermiId",
                table: "Daneshjoos",
                column: "MarkazTermiId");

            migrationBuilder.CreateIndex(
                name: "IX_Daneshjoos_ReshtehId",
                table: "Daneshjoos",
                column: "ReshtehId");

            migrationBuilder.CreateIndex(
                name: "IX_Karmands_MarkazAsliId",
                table: "Karmands",
                column: "MarkazAsliId");

            migrationBuilder.CreateIndex(
                name: "IX_Karmands_MarkazId",
                table: "Karmands",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_Markaz_CodeMarkaz",
                table: "Markazes",
                column: "CodeMarkaz",
                unique: true,
                filter: "[CodeMarkaz] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_ParentId",
                table: "Menus",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OstadMadrak_GrooheAmoozeshiId",
                table: "OstadMadrak",
                column: "GrooheAmoozeshiId");

            migrationBuilder.CreateIndex(
                name: "IX_OstadMadrak_OstadId",
                table: "OstadMadrak",
                column: "OstadId");

            migrationBuilder.CreateIndex(
                name: "IX_Ostad_CodeOstadi",
                table: "Ostads",
                column: "CodeOstadi",
                unique: true,
                filter: "[CodeOstadi] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Ostads_MarkazAsliId",
                table: "Ostads",
                column: "MarkazAsliId");

            migrationBuilder.CreateIndex(
                name: "IX_Ostads_MarkazId",
                table: "Ostads",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Name",
                table: "Permissions",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reshtehs_GrooheAmoozeshiId",
                table: "Reshtehs",
                column: "GrooheAmoozeshiId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true,
                filter: "[RoleId] IS NOT NULL AND [PermissionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sabeghes_UserId",
                table: "Sabeghes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BarnamehHaftegiOstads");

            migrationBuilder.DropTable(
                name: "BarnamehTermiOstads");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "MoshakhasatAdmins");

            migrationBuilder.DropTable(
                name: "OstadMadrak");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SaatBargozariKelashas");

            migrationBuilder.DropTable(
                name: "Sabeghes");

            migrationBuilder.DropTable(
                name: "TaghvimTermis");

            migrationBuilder.DropTable(
                name: "Terms");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Daneshjoos");

            migrationBuilder.DropTable(
                name: "Karmands");

            migrationBuilder.DropTable(
                name: "Ostads");

            migrationBuilder.DropTable(
                name: "Reshtehs");

            migrationBuilder.DropTable(
                name: "Markazes");

            migrationBuilder.DropTable(
                name: "GrooheAmoozeshis");
        }
    }
}
