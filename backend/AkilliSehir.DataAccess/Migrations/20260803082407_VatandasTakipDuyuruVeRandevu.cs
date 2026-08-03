using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AkilliSehir.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class VatandasTakipDuyuruVeRandevu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BildirimiYapanVatandasId",
                table: "Arizalar",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Duyurular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Icerik = table.Column<string>(type: "nvarchar(1600)", maxLength: 1600, nullable: false),
                    Kategori = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Oncelik = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    KonumEnlem = table.Column<double>(type: "float", nullable: true),
                    KonumBoylam = table.Column<double>(type: "float", nullable: true),
                    KapsamYaricapiMetre = table.Column<int>(type: "int", nullable: false),
                    YayinBaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YayinBitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duyurular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Randevular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VatandasId = table.Column<int>(type: "int", nullable: false),
                    Birim = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TarihSaat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Konu = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Randevular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Randevular_Kullanicilar_VatandasId",
                        column: x => x.VatandasId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Duyurular",
                columns: new[] { "Id", "AktifMi", "Baslik", "Icerik", "KapsamYaricapiMetre", "Kategori", "KonumBoylam", "KonumEnlem", "Oncelik", "YayinBaslangicTarihi", "YayinBitisTarihi" },
                values: new object[,]
                {
                    { 1, true, "Planlı su bakım çalışması", "Seyhan çevresinde gece saatlerinde basınç düşüşü yaşanabilir. Ekipler bakım çalışmalarını sabah tamamlayacaktır.", 8000, "SuKesintisi", 35.321300000000001, 37.0, "Onemli", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2030, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, true, "Merkezde yol düzenleme çalışması", "Atatürk Caddesi çevresinde şerit daraltma uygulanacaktır. Sürücülerin alternatif güzergâhları kullanması rica olunur.", 6000, "YolCalismasi", 35.325000000000003, 37.005000000000003, "Bilgi", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2030, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, true, "Hafta sonu kültür etkinlikleri", "Belediye kültür merkezlerinde hafta sonu çocuk atölyeleri ve açık hava gösterileri düzenlenecektir.", 0, "Etkinlik", null, null, "Bilgi", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2030, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Arizalar_BildirimiYapanVatandasId_KayitTarihi",
                table: "Arizalar",
                columns: new[] { "BildirimiYapanVatandasId", "KayitTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Duyurular_AktifMi_YayinBaslangicTarihi",
                table: "Duyurular",
                columns: new[] { "AktifMi", "YayinBaslangicTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_Birim_TarihSaat_Durum",
                table: "Randevular",
                columns: new[] { "Birim", "TarihSaat", "Durum" });

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_VatandasId_TarihSaat",
                table: "Randevular",
                columns: new[] { "VatandasId", "TarihSaat" });

            migrationBuilder.AddForeignKey(
                name: "FK_Arizalar_Kullanicilar_BildirimiYapanVatandasId",
                table: "Arizalar",
                column: "BildirimiYapanVatandasId",
                principalTable: "Kullanicilar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arizalar_Kullanicilar_BildirimiYapanVatandasId",
                table: "Arizalar");

            migrationBuilder.DropTable(
                name: "Duyurular");

            migrationBuilder.DropTable(
                name: "Randevular");

            migrationBuilder.DropIndex(
                name: "IX_Arizalar_BildirimiYapanVatandasId_KayitTarihi",
                table: "Arizalar");

            migrationBuilder.DropColumn(
                name: "BildirimiYapanVatandasId",
                table: "Arizalar");
        }
    }
}
