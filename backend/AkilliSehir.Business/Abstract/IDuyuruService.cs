using AkilliSehir.Core.Entities;

namespace AkilliSehir.Business.Abstract;

/// <summary>
/// Mobil kullanıcıya gösterilecek aktif belediye duyurularının sözleşmesidir.
/// </summary>
public interface IDuyuruService
{
    Task<IReadOnlyList<Duyuru>> GetActiveAsync(
        double? enlem,
        double? boylam,
        CancellationToken cancellationToken = default);
}
