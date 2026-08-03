using AkilliSehir.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AkilliSehir.DataAccess.Concrete.EntityFramework;

/// <summary>
/// Akıllı Şehir uygulamasının EF Core veri erişim bağlamıdır.
/// Bağlantı ayarları DI aracılığıyla dış katmandan alınır.
/// </summary>
public class CityDbContext : DbContext
{
    public CityDbContext(DbContextOptions<CityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();

    public DbSet<Ariza> Arizalar => Set<Ariza>();

    public DbSet<Duyuru> Duyurular => Set<Duyuru>();

    public DbSet<Randevu> Randevular => Set<Randevu>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API sınıfları otomatik olarak uygulanır.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CityDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
