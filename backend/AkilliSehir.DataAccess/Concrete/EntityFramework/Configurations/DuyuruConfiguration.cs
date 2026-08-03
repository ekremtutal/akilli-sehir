using AkilliSehir.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkilliSehir.DataAccess.Concrete.EntityFramework.Configurations;

/// <summary>
/// Duyuruların şehir geneli veya konum çevresinde yayımlanmasını sağlayan eşlemedir.
/// </summary>
public sealed class DuyuruConfiguration : IEntityTypeConfiguration<Duyuru>
{
    public void Configure(EntityTypeBuilder<Duyuru> builder)
    {
        builder.ToTable("Duyurular");
        builder.HasKey(duyuru => duyuru.Id);

        builder.Property(duyuru => duyuru.Baslik).IsRequired().HasMaxLength(180);
        builder.Property(duyuru => duyuru.Icerik).IsRequired().HasMaxLength(1600);
        builder.Property(duyuru => duyuru.Kategori).IsRequired().HasMaxLength(50);
        builder.Property(duyuru => duyuru.Oncelik).IsRequired().HasMaxLength(20);
        builder.Property(duyuru => duyuru.YayinBaslangicTarihi).HasColumnType("datetime2");
        builder.Property(duyuru => duyuru.YayinBitisTarihi).HasColumnType("datetime2");

        builder.HasIndex(duyuru => new { duyuru.AktifMi, duyuru.YayinBaslangicTarihi });

        // Geliştirme ortamında mobil uygulamanın boş görünmemesi için örnek şehir duyuruları.
        builder.HasData(
            new Duyuru
            {
                Id = 1,
                Baslik = "Planlı su bakım çalışması",
                Icerik = "Seyhan çevresinde gece saatlerinde basınç düşüşü yaşanabilir. Ekipler bakım çalışmalarını sabah tamamlayacaktır.",
                Kategori = "SuKesintisi",
                Oncelik = "Onemli",
                KonumEnlem = 37.0000,
                KonumBoylam = 35.3213,
                KapsamYaricapiMetre = 8000,
                YayinBaslangicTarihi = new DateTime(2026, 1, 1),
                YayinBitisTarihi = new DateTime(2030, 12, 31),
                AktifMi = true
            },
            new Duyuru
            {
                Id = 2,
                Baslik = "Merkezde yol düzenleme çalışması",
                Icerik = "Atatürk Caddesi çevresinde şerit daraltma uygulanacaktır. Sürücülerin alternatif güzergâhları kullanması rica olunur.",
                Kategori = "YolCalismasi",
                Oncelik = "Bilgi",
                KonumEnlem = 37.0050,
                KonumBoylam = 35.3250,
                KapsamYaricapiMetre = 6000,
                YayinBaslangicTarihi = new DateTime(2026, 1, 1),
                YayinBitisTarihi = new DateTime(2030, 12, 31),
                AktifMi = true
            },
            new Duyuru
            {
                Id = 3,
                Baslik = "Hafta sonu kültür etkinlikleri",
                Icerik = "Belediye kültür merkezlerinde hafta sonu çocuk atölyeleri ve açık hava gösterileri düzenlenecektir.",
                Kategori = "Etkinlik",
                Oncelik = "Bilgi",
                KonumEnlem = null,
                KonumBoylam = null,
                KapsamYaricapiMetre = 0,
                YayinBaslangicTarihi = new DateTime(2026, 1, 1),
                YayinBitisTarihi = new DateTime(2030, 12, 31),
                AktifMi = true
            });
    }
}
