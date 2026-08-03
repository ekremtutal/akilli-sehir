using AkilliSehir.Core.Entities;

namespace AkilliSehir.Business.Abstract;

/// <summary>
/// Kullanıcı işlemleri için uygulama katmanı sözleşmesidir.
/// </summary>
public interface IKullaniciService
{
    Task<IReadOnlyList<Kullanici>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Kullanici?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Kullanici> AddAsync(Kullanici kullanici, CancellationToken cancellationToken = default);
}
