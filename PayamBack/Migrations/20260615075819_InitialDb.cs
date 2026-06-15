using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class InitialDb : Migration
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
                    CodeGrooheKarbari = table.Column<int>(type: "int", nullable: false),
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: false),
                    Emza = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emkanats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<int>(type: "int", nullable: false),
                    NaamEmkanat = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SarTitrEmkanat = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Component = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TartibNamayeshSarTitr = table.Column<int>(type: "int", nullable: false),
                    TartibNamayeshEmkan = table.Column<int>(type: "int", nullable: false),
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emkanats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrooheAmoozeshis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeDaneshkade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NaamDaneshkadeh = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CodeGrooheAmoozeshi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OnvanGrooheAmoozeshi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CodeTarkibi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
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
                    CodeOstan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NaamOstan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CodeMarkaz = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NaamMarkaz = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VahedMarkaz = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nahiyeh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MahalMarkaz = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CodePosti = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WebSite = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markazes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MoshakhasatAdmins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeMelli = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Naam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NaameKhanevadeghi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                name: "SaatBargozariKelashas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OnvanSaat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CodeSaat = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    SaatShoroo = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    SaatPayan = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Hozoori = table.Column<bool>(type: "bit", nullable: false),
                    Majazi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaatBargozariKelashas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sabeghes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Dastgah = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Moroorgar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ZamanLogin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Table = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    User = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdRecordTagirDahande = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoozHafte = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ZamanTagir = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TozihTagirat = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ZamanLogOut = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sabeghes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaghvimTermis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeTerm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tarikh = table.Column<DateOnly>(type: "date", nullable: false),
                    CodeRooz = table.Column<int>(type: "int", nullable: false),
                    RoozHafteh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CodeHafteh = table.Column<int>(type: "int", nullable: false),
                    Hafteh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeSaateTatili = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OnvanMonasebat = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Tozihat = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: false)
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
                    OnvanTerm = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TermJari = table.Column<DateOnly>(type: "date", nullable: false),
                    TarikheDastrasi = table.Column<DateOnly>(type: "date", nullable: false),
                    TarikheEraeeDars = table.Column<DateOnly>(type: "date", nullable: false),
                    TarikhePayanDars = table.Column<DateOnly>(type: "date", nullable: false),
                    TarikheShorooClass = table.Column<DateOnly>(type: "date", nullable: false),
                    TarikhePayanClass = table.Column<DateOnly>(type: "date", nullable: false),
                    TarikheShorooMojavezMarakez = table.Column<DateOnly>(type: "date", nullable: false),
                    TarikhePayanMojavezMarakez = table.Column<DateOnly>(type: "date", nullable: false),
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: false)
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
                name: "RoleEmkanats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    EmkanatId = table.Column<int>(type: "int", nullable: false),
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleEmkanats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleEmkanats_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleEmkanats_Emkanats_EmkanatId",
                        column: x => x.EmkanatId,
                        principalTable: "Emkanats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reshtehs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeMaghta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Maghta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GrooheAmoozeshiId = table.Column<int>(type: "int", nullable: false),
                    CodeReshteDoRaghami = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CodeReshte = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OnvanReshte = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TermVorood = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TermEamal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
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
                    CodeMelli = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Naam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NaameKhanevadeghi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MarkazId = table.Column<int>(type: "int", nullable: false),
                    MarkazAsliId = table.Column<int>(type: "int", nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Mobile2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TelefonMostaghim = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TelefonGhayreMostaghim = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TelefonDakheli = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Emza = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
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
                    CodeDaneshkade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeGrooheAmoozeshi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reshteh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MarkazId = table.Column<int>(type: "int", nullable: false),
                    MarkazAsliId = table.Column<int>(type: "int", nullable: true),
                    CodeOstadi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NaamKhanevadegi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Naam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Jens = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NaamPedar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TarikhTavalod = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShomareShenasname = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShomareMelli = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Mobile2 = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    MartabeElmi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SazmanMarboote = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MahalEshteghal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Emza = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Vazeeat = table.Column<bool>(type: "bit", nullable: false),
                    NoeHamkari = table.Column<int>(type: "int", nullable: false)
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
                name: "Daneshjoos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarkazId = table.Column<int>(type: "int", nullable: false),
                    MarkazAzmoonId = table.Column<int>(type: "int", nullable: false),
                    MarkazTermiId = table.Column<int>(type: "int", nullable: false),
                    ReshtehId = table.Column<int>(type: "int", nullable: false),
                    ShomareDaneshjooee = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NaamKhanevadegi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Naam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Jens = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Naampedar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShomareMelli = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShomareShenasname = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShomareGozarnameYaKartHoviyat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShenasayeFaragirAtbaaKhareji = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MahalSodoor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TarikhTavalod = table.Column<DateOnly>(type: "date", nullable: true),
                    TermVorood = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShomareParvande = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VazeeyatParvande = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChapDast = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShomareDavtalabi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeReshteMahalGhabooliSanjesh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShomareSanjesh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
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
                    OstadId = table.Column<int>(type: "int", nullable: false),
                    CodeOstad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MarkazId = table.Column<int>(type: "int", nullable: false),
                    CodeTerm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoozeHafteh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    A = table.Column<int>(type: "int", nullable: false),
                    B = table.Column<int>(type: "int", nullable: false),
                    C = table.Column<int>(type: "int", nullable: false),
                    D = table.Column<int>(type: "int", nullable: false),
                    E = table.Column<int>(type: "int", nullable: false),
                    F = table.Column<int>(type: "int", nullable: false),
                    G = table.Column<int>(type: "int", nullable: false),
                    H = table.Column<int>(type: "int", nullable: false),
                    Jozeiat = table.Column<bool>(type: "bit", nullable: false),
                    Tozihat = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
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
                        principalColumn: "CodeTerm",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BarnamehTermiOstads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OstadId = table.Column<int>(type: "int", nullable: false),
                    CodeOstad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MarkazId = table.Column<int>(type: "int", nullable: false),
                    CodeTerm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoozeHafteh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tarikh = table.Column<DateOnly>(type: "date", nullable: false),
                    A = table.Column<int>(type: "int", nullable: false),
                    B = table.Column<int>(type: "int", nullable: false),
                    C = table.Column<int>(type: "int", nullable: false),
                    D = table.Column<int>(type: "int", nullable: false),
                    E = table.Column<int>(type: "int", nullable: false),
                    F = table.Column<int>(type: "int", nullable: false),
                    G = table.Column<int>(type: "int", nullable: false),
                    H = table.Column<int>(type: "int", nullable: false),
                    TA = table.Column<bool>(type: "bit", nullable: false),
                    TB = table.Column<bool>(type: "bit", nullable: false),
                    TC = table.Column<bool>(type: "bit", nullable: false),
                    TD = table.Column<bool>(type: "bit", nullable: false),
                    TE = table.Column<bool>(type: "bit", nullable: false),
                    TF = table.Column<bool>(type: "bit", nullable: false),
                    TG = table.Column<bool>(type: "bit", nullable: false),
                    TH = table.Column<bool>(type: "bit", nullable: false),
                    Faal = table.Column<bool>(type: "bit", nullable: false)
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
                        principalColumn: "CodeTerm",
                        onDelete: ReferentialAction.Cascade);
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
                    Vazeeyat = table.Column<bool>(type: "bit", nullable: false),
                    VazeeyatMovaghat = table.Column<bool>(type: "bit", nullable: false),
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
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
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
                name: "IX_AspNetUserRoles_MarkazId",
                table: "AspNetUserRoles",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId_RoleId_MarkazId",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId", "MarkazId" },
                unique: true,
                filter: "[MarkazId] IS NOT NULL");

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
                name: "IX_BarnamehHaftegiOstads_CodeOstad_CodeTerm_MarkazId_RoozeHafteh",
                table: "BarnamehHaftegiOstads",
                columns: new[] { "CodeOstad", "CodeTerm", "MarkazId", "RoozeHafteh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstads_CodeTerm",
                table: "BarnamehHaftegiOstads",
                column: "CodeTerm");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstads_MarkazId",
                table: "BarnamehHaftegiOstads",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehHaftegiOstads_OstadId",
                table: "BarnamehHaftegiOstads",
                column: "OstadId");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehTermiOstads_CodeOstad_CodeTerm_MarkazId_Tarikh",
                table: "BarnamehTermiOstads",
                columns: new[] { "CodeOstad", "CodeTerm", "MarkazId", "Tarikh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehTermiOstads_CodeTerm",
                table: "BarnamehTermiOstads",
                column: "CodeTerm");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehTermiOstads_MarkazId",
                table: "BarnamehTermiOstads",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_BarnamehTermiOstads_OstadId",
                table: "BarnamehTermiOstads",
                column: "OstadId");

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
                name: "IX_Markazes_CodeMarkaz",
                table: "Markazes",
                column: "CodeMarkaz",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ostads_CodeOstadi",
                table: "Ostads",
                column: "CodeOstadi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ostads_MarkazAsliId",
                table: "Ostads",
                column: "MarkazAsliId");

            migrationBuilder.CreateIndex(
                name: "IX_Ostads_MarkazId",
                table: "Ostads",
                column: "MarkazId");

            migrationBuilder.CreateIndex(
                name: "IX_Reshtehs_GrooheAmoozeshiId",
                table: "Reshtehs",
                column: "GrooheAmoozeshiId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleEmkanats_EmkanatId",
                table: "RoleEmkanats",
                column: "EmkanatId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleEmkanats_RoleId",
                table: "RoleEmkanats",
                column: "RoleId");
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
                name: "MoshakhasatAdmins");

            migrationBuilder.DropTable(
                name: "RoleEmkanats");

            migrationBuilder.DropTable(
                name: "SaatBargozariKelashas");

            migrationBuilder.DropTable(
                name: "Sabeghes");

            migrationBuilder.DropTable(
                name: "TaghvimTermis");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Terms");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Emkanats");

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
