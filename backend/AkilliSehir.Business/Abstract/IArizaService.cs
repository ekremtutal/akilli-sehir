using AkilliSehir.Core.Entities;
using AkilliSehir.Core.Entities.Enums;
using AkilliSehir.Business.Models;

namespace AkilliSehir.Business.Abstract;

/// <summary>
/// Arıza işlemlerinin uygulama katmanı sözleşmesidir.
/// </summary>
public interface IArizaService
{
    Task<IReadOnlyList<Ariza>> GetAllAsync(
        BelediyeBirimi? birim = null,
        CancellationToken cancellationToken = default);

    Task<Ariza?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ariza>> GetByVatandasIdAsync(
        int vatandasId,
        CancellationToken cancellationToken = default);

    Task<Ariza> AddAsync(Ariza ariza, CancellationToken cancellationToken = default);

    Task<ArizaDurumGuncellemeSonucu> UpdateStatusAsync(
        int id,
        ArizaDurumu durum,
        int atananPersonelId,
        BelediyeBirimi personelinBirimi,
        CancellationToken cancellationToken = default);
}
