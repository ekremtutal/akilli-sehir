using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkilliSehir.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MobilKimlikVeYonlendirme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AktifMi",
                table: "Kullanicilar",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "CalistigiBirim",
                table: "Kullanicilar",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KullaniciAdi",
                table: "Kullanicilar",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ParolaHash",
                table: "Kullanicilar",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TcKimlikNoHash",
                table: "Kullanicilar",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonNumarasi",
                table: "Kullanicilar",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArizaTuru",
                table: "Arizalar",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VatandasSecilenBirim",
                table: "Arizalar",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YapayZekaGerekcesi",
                table: "Arizalar",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "YapayZekaGuvenSkoru",
                table: "Arizalar",
                type: "decimal(4,3)",
                precision: 4,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YonlendirilenBirim",
                table: "Arizalar",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Diger");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_TcKimlikNoHash",
                table: "Kullanicilar",
                column: "TcKimlikNoHash",
                unique: true,
                filter: "[TcKimlikNoHash] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Arizalar_YonlendirilenBirim_Durum",
                table: "Arizalar",
                columns: new[] { "YonlendirilenBirim", "Durum" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar");

            migrationBuilder.DropIndex(
                name: "IX_Kullanicilar_TcKimlikNoHash",
                table: "Kullanicilar");

            migrationBuilder.DropIndex(
                name: "IX_Arizalar_YonlendirilenBirim_Durum",
                table: "Arizalar");

            migrationBuilder.DropColumn(
                name: "AktifMi",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "CalistigiBirim",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "KullaniciAdi",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "ParolaHash",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "TcKimlikNoHash",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "TelefonNumarasi",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "ArizaTuru",
                table: "Arizalar");

            migrationBuilder.DropColumn(
                name: "VatandasSecilenBirim",
                table: "Arizalar");

            migrationBuilder.DropColumn(
                name: "YapayZekaGerekcesi",
                table: "Arizalar");

            migrationBuilder.DropColumn(
                name: "YapayZekaGuvenSkoru",
                table: "Arizalar");

            migrationBuilder.DropColumn(
                name: "YonlendirilenBirim",
                table: "Arizalar");
        }
    }
}
