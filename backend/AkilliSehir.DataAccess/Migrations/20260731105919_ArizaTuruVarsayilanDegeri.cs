using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkilliSehir.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ArizaTuruVarsayilanDegeri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Önceki şemada oluşturulmuş kayıtların boş tür değeri, enumun
            // güvenli varsayılanı olan Bilinmiyor'a dönüştürülür.
            migrationBuilder.Sql(
                "UPDATE [Arizalar] SET [ArizaTuru] = N'Bilinmiyor' WHERE [ArizaTuru] = N'';");

            migrationBuilder.AlterColumn<string>(
                name: "ArizaTuru",
                table: "Arizalar",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Bilinmiyor",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ArizaTuru",
                table: "Arizalar",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldDefaultValue: "Bilinmiyor");
        }
    }
}
