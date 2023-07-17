using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jenga.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddGonderiPaketiTableToDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BirimTanim_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Adi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KisaAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: false),
                    AmirId = table.Column<int>(type: "int", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BirimTanim_Table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DagitimYeriTanim_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Adi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DagitimYeriTanim_Table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepoTanim_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Adi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepoTanim_Table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GorevTanim_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BirimId = table.Column<long>(type: "bigint", nullable: false),
                    Adi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KisaAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonelId = table.Column<int>(type: "int", nullable: true),
                    Vekil = table.Column<bool>(type: "bit", nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GorevTanim_Table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Il_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IlAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlakaKodu = table.Column<int>(type: "int", nullable: false),
                    IngIlAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bolge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Il_Table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KaynakTanim_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Adi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KaynakTanim_Table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuTanim_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Adi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UstMenuId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Webpart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuTanim_Table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModulTanim_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Adi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebpartAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModulTanim_Table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personel_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Adi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Soyadi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SicilNo = table.Column<int>(type: "int", nullable: false),
                    Tahsili = table.Column<int>(type: "int", nullable: true),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Asker_sivil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personel_Table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnvanTanim_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GorTipId = table.Column<short>(type: "smallint", nullable: false),
                    Adi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KisaAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnvanTanim_Table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ilce_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IlId = table.Column<int>(type: "int", nullable: false),
                    IlAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IlceId = table.Column<int>(type: "int", nullable: false),
                    IlceAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ilce_Table", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ilce_Table_Il_Table_IlId",
                        column: x => x.IlId,
                        principalTable: "Il_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AniObjesiTanim_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Adi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StokluMu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    KaynakId = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AniObjesiTanim_Table", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AniObjesiTanim_Table_KaynakTanim_Table_KaynakId",
                        column: x => x.KaynakId,
                        principalTable: "KaynakTanim_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonelMenu_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonelId = table.Column<int>(type: "int", nullable: false),
                    MenuTanimId = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonelMenu_Table", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonelMenu_Table_MenuTanim_Table_MenuTanimId",
                        column: x => x.MenuTanimId,
                        principalTable: "MenuTanim_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonelMenu_Table_Personel_Table_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personel_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IsBilgileri_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonelId = table.Column<int>(type: "int", nullable: false),
                    UnvanId = table.Column<int>(type: "int", nullable: false),
                    GorevId = table.Column<int>(type: "int", nullable: false),
                    BirimId = table.Column<int>(type: "int", nullable: false),
                    BaslamaTar = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CalismaDurumu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AyrilmaTar = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AyrilmaSebebi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SGKSicilNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SGKBasTar = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VakifOncesiPrimGunSayisi = table.Column<int>(type: "int", nullable: false),
                    EmeklilikTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IzinDonemiBasTar = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IsBilgileri_Table", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IsBilgileri_Table_BirimTanim_Table_BirimId",
                        column: x => x.BirimId,
                        principalTable: "BirimTanim_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IsBilgileri_Table_GorevTanim_Table_GorevId",
                        column: x => x.GorevId,
                        principalTable: "GorevTanim_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IsBilgileri_Table_Personel_Table_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personel_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IsBilgileri_Table_UnvanTanim_Table_UnvanId",
                        column: x => x.UnvanId,
                        principalTable: "UnvanTanim_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GonderiPaketi_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Etiket = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DagitimYeriTanimId = table.Column<int>(type: "int", nullable: false),
                    GondermeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GondermeAraci = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GonderiTakipNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IlId = table.Column<int>(type: "int", nullable: false),
                    IlceId = table.Column<int>(type: "int", nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GonderiPaketi_Table", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GonderiPaketi_Table_DepoTanim_Table_DagitimYeriTanimId",
                        column: x => x.DagitimYeriTanimId,
                        principalTable: "DepoTanim_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GonderiPaketi_Table_Il_Table_IlId",
                        column: x => x.IlId,
                        principalTable: "Il_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GonderiPaketi_Table_Ilce_Table_IlceId",
                        column: x => x.IlceId,
                        principalTable: "Ilce_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepoHareket_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AniObjesiId = table.Column<int>(type: "int", nullable: false),
                    KaynakId = table.Column<int>(type: "int", nullable: false),
                    KaynakDepoId = table.Column<int>(type: "int", nullable: false),
                    HedefDepoId = table.Column<int>(type: "int", nullable: false),
                    Adet = table.Column<int>(type: "int", nullable: false),
                    GirisCikis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IslemYapan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepoHareket_Table", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepoHareket_Table_AniObjesiTanim_Table_AniObjesiId",
                        column: x => x.AniObjesiId,
                        principalTable: "AniObjesiTanim_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepoHareket_Table_DepoTanim_Table_HedefDepoId",
                        column: x => x.HedefDepoId,
                        principalTable: "DepoTanim_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepoHareket_Table_DepoTanim_Table_KaynakDepoId",
                        column: x => x.KaynakDepoId,
                        principalTable: "DepoTanim_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepoHareket_Table_DepoTanim_Table_KaynakId",
                        column: x => x.KaynakId,
                        principalTable: "DepoTanim_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepoStok_Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AniObjesiId = table.Column<int>(type: "int", nullable: false),
                    DepoId = table.Column<int>(type: "int", nullable: false),
                    SonAdet = table.Column<int>(type: "int", nullable: false),
                    SonIslemTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SonIslemYapan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Olusturan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Degistiren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegistirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepoStok_Table", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepoStok_Table_AniObjesiTanim_Table_AniObjesiId",
                        column: x => x.AniObjesiId,
                        principalTable: "AniObjesiTanim_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepoStok_Table_DepoTanim_Table_DepoId",
                        column: x => x.DepoId,
                        principalTable: "DepoTanim_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AniObjesiTanim_Table_KaynakId",
                table: "AniObjesiTanim_Table",
                column: "KaynakId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoHareket_Table_AniObjesiId",
                table: "DepoHareket_Table",
                column: "AniObjesiId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoHareket_Table_HedefDepoId",
                table: "DepoHareket_Table",
                column: "HedefDepoId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoHareket_Table_KaynakDepoId",
                table: "DepoHareket_Table",
                column: "KaynakDepoId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoHareket_Table_KaynakId",
                table: "DepoHareket_Table",
                column: "KaynakId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoStok_Table_AniObjesiId",
                table: "DepoStok_Table",
                column: "AniObjesiId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoStok_Table_DepoId",
                table: "DepoStok_Table",
                column: "DepoId");

            migrationBuilder.CreateIndex(
                name: "IX_GonderiPaketi_Table_DagitimYeriTanimId",
                table: "GonderiPaketi_Table",
                column: "DagitimYeriTanimId");

            migrationBuilder.CreateIndex(
                name: "IX_GonderiPaketi_Table_IlceId",
                table: "GonderiPaketi_Table",
                column: "IlceId");

            migrationBuilder.CreateIndex(
                name: "IX_GonderiPaketi_Table_IlId",
                table: "GonderiPaketi_Table",
                column: "IlId");

            migrationBuilder.CreateIndex(
                name: "IX_Ilce_Table_IlId",
                table: "Ilce_Table",
                column: "IlId");

            migrationBuilder.CreateIndex(
                name: "IX_IsBilgileri_Table_BirimId",
                table: "IsBilgileri_Table",
                column: "BirimId");

            migrationBuilder.CreateIndex(
                name: "IX_IsBilgileri_Table_GorevId",
                table: "IsBilgileri_Table",
                column: "GorevId");

            migrationBuilder.CreateIndex(
                name: "IX_IsBilgileri_Table_PersonelId",
                table: "IsBilgileri_Table",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_IsBilgileri_Table_UnvanId",
                table: "IsBilgileri_Table",
                column: "UnvanId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonelMenu_Table_MenuTanimId",
                table: "PersonelMenu_Table",
                column: "MenuTanimId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonelMenu_Table_PersonelId",
                table: "PersonelMenu_Table",
                column: "PersonelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DagitimYeriTanim_Table");

            migrationBuilder.DropTable(
                name: "DepoHareket_Table");

            migrationBuilder.DropTable(
                name: "DepoStok_Table");

            migrationBuilder.DropTable(
                name: "GonderiPaketi_Table");

            migrationBuilder.DropTable(
                name: "IsBilgileri_Table");

            migrationBuilder.DropTable(
                name: "ModulTanim_Table");

            migrationBuilder.DropTable(
                name: "PersonelMenu_Table");

            migrationBuilder.DropTable(
                name: "AniObjesiTanim_Table");

            migrationBuilder.DropTable(
                name: "DepoTanim_Table");

            migrationBuilder.DropTable(
                name: "Ilce_Table");

            migrationBuilder.DropTable(
                name: "BirimTanim_Table");

            migrationBuilder.DropTable(
                name: "GorevTanim_Table");

            migrationBuilder.DropTable(
                name: "UnvanTanim_Table");

            migrationBuilder.DropTable(
                name: "MenuTanim_Table");

            migrationBuilder.DropTable(
                name: "Personel_Table");

            migrationBuilder.DropTable(
                name: "KaynakTanim_Table");

            migrationBuilder.DropTable(
                name: "Il_Table");
        }
    }
}
