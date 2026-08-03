using AkilliSehir.Business.Abstract;
using AkilliSehir.Core.Entities;
using AkilliSehir.DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace AkilliSehir.Business.Concrete;

/// <summary>
/// Duyuruları yayın süresine ve talep edilen konuma göre filtreler.
/// </summary>
public sealed class DuyuruManager : IDuyuruService
{
    private readonly CityDbContext _context;

    public DuyuruManager(CityDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Duyuru>> GetActiveAsync(
        double? enlem,
        double? boylam,
        CancellationToken cancellationToken = default)
    {
        var simdi = DateTime.UtcNow;
        var duyurular = await _context.Duyurular
            .AsNoTracking()
            .Where(duyuru => duyuru.AktifMi
                && duyuru.YayinBaslangicTarihi <= simdi
                && (duyuru.YayinBitisTarihi == null || duyuru.YayinBitisTarihi >= simdi))
            .OrderByDescending(duyuru => duyuru.Oncelik == "Acil")
            .ThenByDescending(duyuru => duyuru.YayinBaslangicTarihi)
            .ToListAsync(cancellationToken);

        // Konum paylaşılmadığında şehir geneli duyuruların yanında tüm aktif
        // duyurular gösterilir. Konum paylaşılırsa uzak duyurular gizlenir.
        if (enlem is null || boylam is null)
        {
            return duyurular;
        }

        return duyurular
            .Where(duyuru => DuyuruKapsamindaMi(duyuru, enlem.Value, boylam.Value))
            .ToList();
    }

    private static bool DuyuruKapsamindaMi(Duyuru duyuru, double enlem, double boylam)
    {
        if (duyuru.KonumEnlem is null || duyuru.KonumBoylam is null)
        {
            return true;
        }

        const double dunyaYaricapiMetre = 6371000;
        var enlemFarki = DereceyiRadyanaCevir(duyuru.KonumEnlem.Value - enlem);
        var boylamFarki = DereceyiRadyanaCevir(duyuru.KonumBoylam.Value - boylam);
        var a = Math.Sin(enlemFarki / 2) * Math.Sin(enlemFarki / 2)
            + Math.Cos(DereceyiRadyanaCevir(enlem))
            * Math.Cos(DereceyiRadyanaCevir(duyuru.KonumEnlem.Value))
            * Math.Sin(boylamFarki / 2) * Math.Sin(boylamFarki / 2);
        var mesafe = dunyaYaricapiMetre * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return mesafe <= duyuru.KapsamYaricapiMetre;
    }

    private static double DereceyiRadyanaCevir(double derece) => derece * Math.PI / 180;
}
