using AkilliSehir.Business.Abstract;
using AkilliSehir.Business.Models;
using AkilliSehir.Core.Entities;
using AkilliSehir.Core.Entities.Enums;
using AkilliSehir.DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace AkilliSehir.Business.Concrete;

/// <summary>
/// Arıza kayıtları için iş kurallarını ve veri erişim işlemlerini yürütür.
/// </summary>
public sealed class ArizaManager : IArizaService
{
    private readonly CityDbContext _context;

    public ArizaManager(CityDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Ariza>> GetAllAsync(
        BelediyeBirimi? birim = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Ariza> sorgu = _context.Arizalar
            .AsNoTracking()
            .Include(ariza => ariza.AtananPersonel);

        // Personel yalnızca kendi birimine yönlendirilmiş arızaları görüntüler.
        if (birim is not null && birim is not BelediyeBirimi.Bilinmiyor)
        {
            sorgu = sorgu.Where(ariza => ariza.YonlendirilenBirim == birim);
        }

        return await sorgu
            .OrderByDescending(ariza => ariza.KayitTarihi)
            .ToListAsync(cancellationToken);
    }

    public async Task<Ariza?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Arizalar
            .AsNoTracking()
            .Include(ariza => ariza.AtananPersonel)
            .FirstOrDefaultAsync(ariza => ariza.Id == id, cancellationToken);
    }

    public async Task<Ariza> AddAsync(
        Ariza ariza,
        CancellationToken cancellationToken = default)
    {
        // İstemci tarih göndermediyse kayıt anı UTC olarak belirlenir.
        if (ariza.KayitTarihi == default)
        {
            ariza.KayitTarihi = DateTime.UtcNow;
        }

        await _context.Arizalar.AddAsync(ariza, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ariza;
    }

    public async Task<ArizaDurumGuncellemeSonucu> UpdateStatusAsync(
        int id,
        ArizaDurumu durum,
        int atananPersonelId,
        BelediyeBirimi personelinBirimi,
        CancellationToken cancellationToken = default)
    {
        var ariza = await _context.Arizalar
            .FirstOrDefaultAsync(kayit => kayit.Id == id, cancellationToken);

        if (ariza is null)
        {
            return ArizaDurumGuncellemeSonucu.Bulunamadi;
        }

        // Personel, başka birimin işini veya başka bir personele atanmış işi yönetemez.
        if (ariza.YonlendirilenBirim != personelinBirimi ||
            (ariza.AtananPersonelId is not null && ariza.AtananPersonelId != atananPersonelId))
        {
            return ArizaDurumGuncellemeSonucu.Yetkisiz;
        }

        // Token eski kalmış olabileceği için personelin aktiflik, rol ve birim bilgisi
        // veritabanından da doğrulanır.
        var personelVarMi = await _context.Kullanicilar.AnyAsync(
            kullanici => kullanici.Id == atananPersonelId
                && kullanici.Rol == KullaniciRolu.SahaPersoneli
                && kullanici.AktifMi
                && kullanici.CalistigiBirim == personelinBirimi,
            cancellationToken);

        if (!personelVarMi)
        {
            return ArizaDurumGuncellemeSonucu.Yetkisiz;
        }

        ariza.Durum = durum;
        // Atama kimliği HTTP gövdesinden değil, doğrulanmış JWT claim'inden gelir.
        ariza.AtananPersonelId = atananPersonelId;

        await _context.SaveChangesAsync(cancellationToken);

        return ArizaDurumGuncellemeSonucu.Basarili;
    }
}
