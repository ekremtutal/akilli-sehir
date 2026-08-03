using AkilliSehir.Business.Models;
using AkilliSehir.Core.Entities;
using AkilliSehir.Core.Entities.Enums;

namespace AkilliSehir.Business.Abstract;

/// <summary>
/// Vatandaş randevularının uygunluk ve sahiplik kurallarıyla yönetilmesini sağlar.
/// </summary>
public interface IRandevuService
{
    Task<IReadOnlyList<Randevu>> GetByVatandasIdAsync(
        int vatandasId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeOnly>> GetUygunSaatlerAsync(
        BelediyeBirimi birim,
        DateOnly tarih,
        CancellationToken cancellationToken = default);

    Task<RandevuOlusturmaSonucu> CreateAsync(
        int vatandasId,
        BelediyeBirimi birim,
        DateTime tarihSaat,
        string konu,
        CancellationToken cancellationToken = default);

    Task<RandevuIptalSonucu> CancelAsync(
        int randevuId,
        int vatandasId,
        CancellationToken cancellationToken = default);
}
