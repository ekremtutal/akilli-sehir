using AkilliSehir.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkilliSehir.DataAccess.Concrete.EntityFramework.Configurations;

/// <summary>
/// Randevu kayıtlarının vatandaş ve belediye birimiyle ilişkisini tanımlar.
/// </summary>
public sealed class RandevuConfiguration : IEntityTypeConfiguration<Randevu>
{
    public void Configure(EntityTypeBuilder<Randevu> builder)
    {
        builder.ToTable("Randevular");
        builder.HasKey(randevu => randevu.Id);

        builder.Property(randevu => randevu.Birim)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(randevu => randevu.Konu).IsRequired().HasMaxLength(300);
        builder.Property(randevu => randevu.Durum)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);
        builder.Property(randevu => randevu.TarihSaat).HasColumnType("datetime2");
        builder.Property(randevu => randevu.KayitTarihi)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(randevu => randevu.Vatandas)
            .WithMany(kullanici => kullanici.Randevular)
            .HasForeignKey(randevu => randevu.VatandasId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(randevu => new { randevu.Birim, randevu.TarihSaat, randevu.Durum });
        builder.HasIndex(randevu => new { randevu.VatandasId, randevu.TarihSaat });
    }
}
