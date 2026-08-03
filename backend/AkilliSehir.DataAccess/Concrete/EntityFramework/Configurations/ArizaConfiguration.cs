using AkilliSehir.Core.Entities;
using AkilliSehir.Core.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkilliSehir.DataAccess.Concrete.EntityFramework.Configurations;

/// <summary>
/// Ariza tablosunun alan, enum ve ilişki eşlemesini içerir.
/// </summary>
public sealed class ArizaConfiguration : IEntityTypeConfiguration<Ariza>
{
    public void Configure(EntityTypeBuilder<Ariza> builder)
    {
        builder.ToTable("Arizalar");

        builder.HasKey(ariza => ariza.Id);

        builder.Property(ariza => ariza.Baslik)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ariza => ariza.Aciklama)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(ariza => ariza.FotografUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ariza => ariza.Durum)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(ariza => ariza.Aciliyet)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(ariza => ariza.ArizaTuru)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(40)
            .HasDefaultValue(ArizaTuru.Bilinmiyor);

        builder.Property(ariza => ariza.VatandasSecilenBirim)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(ariza => ariza.YonlendirilenBirim)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50)
            .HasSentinel(BelediyeBirimi.Bilinmiyor)
            .HasDefaultValue(BelediyeBirimi.Diger);

        builder.Property(ariza => ariza.YapayZekaGuvenSkoru)
            .HasPrecision(4, 3);

        builder.Property(ariza => ariza.YapayZekaGerekcesi)
            .HasMaxLength(2000);

        builder.Property(ariza => ariza.KayitTarihi)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Arıza geçmişi kaybolmamalıdır. Atanan personel ilişkisi zaten SET NULL
        // kullandığı için SQL Server'ın çoklu cascade path kuralına takılmamak adına
        // vatandaş hesabı silinmeden önce bu ilişki uygulama katmanında yönetilir.
        builder.HasOne(ariza => ariza.BildirimiYapanVatandas)
            .WithMany(kullanici => kullanici.BildirilenArizalar)
            .HasForeignKey(ariza => ariza.BildirimiYapanVatandasId)
            .OnDelete(DeleteBehavior.NoAction);

        // Bir personel birçok arızaya atanabilir; bir arızanın en fazla bir
        // atanmış personeli bulunur. Personel silinirse atama null'a çekilir.
        builder.HasOne(ariza => ariza.AtananPersonel)
            .WithMany(kullanici => kullanici.AtananArizalar)
            .HasForeignKey(ariza => ariza.AtananPersonelId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(ariza => new { ariza.YonlendirilenBirim, ariza.Durum });
        builder.HasIndex(ariza => new { ariza.BildirimiYapanVatandasId, ariza.KayitTarihi });
    }
}
