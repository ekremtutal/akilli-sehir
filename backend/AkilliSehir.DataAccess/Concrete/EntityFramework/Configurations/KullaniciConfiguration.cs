using AkilliSehir.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkilliSehir.DataAccess.Concrete.EntityFramework.Configurations;

/// <summary>
/// Kullanici tablosunun veritabanı eşlemesini içerir.
/// </summary>
public sealed class KullaniciConfiguration : IEntityTypeConfiguration<Kullanici>
{
    public void Configure(EntityTypeBuilder<Kullanici> builder)
    {
        builder.ToTable("Kullanicilar");

        builder.HasKey(kullanici => kullanici.Id);

        builder.Property(kullanici => kullanici.AdSoyad)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(kullanici => kullanici.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(kullanici => kullanici.KullaniciAdi)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(kullanici => kullanici.ParolaHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(kullanici => kullanici.TelefonNumarasi)
            .HasMaxLength(20);

        builder.Property(kullanici => kullanici.TcKimlikNoHash)
            .HasMaxLength(128);

        // Enum metin olarak saklanır; veritabanı kayıtları daha okunabilir olur.
        builder.Property(kullanici => kullanici.Rol)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(kullanici => kullanici.CalistigiBirim)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(kullanici => kullanici.AktifMi)
            .HasDefaultValue(true);

        // Aynı e-posta ile birden fazla kullanıcı oluşmasını önler.
        builder.HasIndex(kullanici => kullanici.Email)
            .IsUnique();

        builder.HasIndex(kullanici => kullanici.KullaniciAdi)
            .IsUnique();

        builder.HasIndex(kullanici => kullanici.TcKimlikNoHash)
            .IsUnique()
            // SQL Server'da birden fazla null değere izin verilir.
            .HasFilter("[TcKimlikNoHash] IS NOT NULL");
    }
}
