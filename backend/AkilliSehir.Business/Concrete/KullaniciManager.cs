using AkilliSehir.Business.Abstract;
using AkilliSehir.Core.Entities;
using AkilliSehir.DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace AkilliSehir.Business.Concrete;

/// <summary>
/// Kullanıcı kayıtları için iş kurallarını ve veri erişim işlemlerini yürütür.
/// </summary>
public sealed class KullaniciManager : IKullaniciService
{
    private readonly CityDbContext _context;

    public KullaniciManager(CityDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Kullanici>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Kullanicilar
            .AsNoTracking()
            .OrderBy(kullanici => kullanici.AdSoyad)
            .ToListAsync(cancellationToken);
    }

    public async Task<Kullanici?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Kullanicilar
            .AsNoTracking()
            .FirstOrDefaultAsync(kullanici => kullanici.Id == id, cancellationToken);
    }

    public async Task<Kullanici> AddAsync(
        Kullanici kullanici,
        CancellationToken cancellationToken = default)
    {
        kullanici.Email = kullanici.Email.Trim().ToLowerInvariant();

        await _context.Kullanicilar.AddAsync(kullanici, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return kullanici;
    }
}
