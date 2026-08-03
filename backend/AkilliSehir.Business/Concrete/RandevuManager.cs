using AkilliSehir.Business.Abstract;
using AkilliSehir.Business.Models;
using AkilliSehir.Core.Entities;
using AkilliSehir.Core.Entities.Enums;
using AkilliSehir.DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace AkilliSehir.Business.Concrete;

/// <summary>
/// Randevu çakışmalarını ve vatandaş sahiplik denetimini uygulama katmanında yönetir.
/// </summary>
public sealed class RandevuManager : IRandevuService
{
    private static readonly TimeOnly[] GunlukSaatler =
    [
        new(9, 0), new(10, 0), new(11, 0), new(13, 0), new(14, 0), new(15, 0), new(16, 0)
    ];

    private readonly CityDbContext _context;

    public RandevuManager(CityDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Randevu>> GetByVatandasIdAsync(
        int vatandasId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Randevular
            .AsNoTracking()
            .Where(randevu => randevu.VatandasId == vatandasId)
            .OrderBy(randevu => randevu.Durum == RandevuDurumu.Planlandi ? 0 : 1)
            .ThenBy(randevu => randevu.TarihSaat)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TimeOnly>> GetUygunSaatlerAsync(
        BelediyeBirimi birim,
        DateOnly tarih,
        CancellationToken cancellationToken = default)
    {
        if (birim == BelediyeBirimi.Bilinmiyor || tarih < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return [];
        }

        var baslangic = tarih.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var bitis = baslangic.AddDays(1);
        var doluSaatler = await _context.Randevular
            .AsNoTracking()
            .Where(randevu => randevu.Birim == birim
                && randevu.Durum == RandevuDurumu.Planlandi
                && randevu.TarihSaat >= baslangic
                && randevu.TarihSaat < bitis)
            .Select(randevu => randevu.TarihSaat)
            .ToListAsync(cancellationToken);

        var simdi = DateTime.UtcNow;
        return GunlukSaatler
            .Where(saat => !doluSaatler.Any(dolu => TimeOnly.FromDateTime(dolu) == saat))
            .Where(saat => baslangic.Date > simdi.Date || saat > TimeOnly.FromDateTime(simdi))
            .ToList();
    }

    public async Task<RandevuOlusturmaSonucu> CreateAsync(
        int vatandasId,
        BelediyeBirimi birim,
        DateTime tarihSaat,
        string konu,
        CancellationToken cancellationToken = default)
    {
        var utcTarihSaat = tarihSaat.Kind == DateTimeKind.Utc
            ? tarihSaat
            : tarihSaat.ToUniversalTime();
        var tarih = DateOnly.FromDateTime(utcTarihSaat);
        var saat = TimeOnly.FromDateTime(utcTarihSaat);
        var uygunSaatler = await GetUygunSaatlerAsync(birim, tarih, cancellationToken);

        if (!uygunSaatler.Contains(saat))
        {
            return new RandevuOlusturmaSonucu(null, "Seçtiğiniz saat artık uygun değil. Lütfen başka bir saat seçiniz.");
        }

        var randevu = new Randevu
        {
            VatandasId = vatandasId,
            Birim = birim,
            TarihSaat = utcTarihSaat,
            Konu = konu.Trim(),
            Durum = RandevuDurumu.Planlandi,
            KayitTarihi = DateTime.UtcNow
        };
        await _context.Randevular.AddAsync(randevu, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new RandevuOlusturmaSonucu(randevu, null);
    }

    public async Task<RandevuIptalSonucu> CancelAsync(
        int randevuId,
        int vatandasId,
        CancellationToken cancellationToken = default)
    {
        var randevu = await _context.Randevular
            .FirstOrDefaultAsync(kayit => kayit.Id == randevuId, cancellationToken);
        if (randevu is null)
        {
            return RandevuIptalSonucu.Bulunamadi;
        }
        if (randevu.VatandasId != vatandasId)
        {
            return RandevuIptalSonucu.Yetkisiz;
        }
        if (randevu.Durum != RandevuDurumu.Planlandi || randevu.TarihSaat <= DateTime.UtcNow.AddHours(2))
        {
            return RandevuIptalSonucu.IptalEdilemez;
        }

        randevu.Durum = RandevuDurumu.IptalEdildi;
        await _context.SaveChangesAsync(cancellationToken);
        return RandevuIptalSonucu.Basarili;
    }
}
