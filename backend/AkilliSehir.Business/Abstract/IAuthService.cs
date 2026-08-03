using AkilliSehir.Business.DTOs.Auth;
using AkilliSehir.Core.Entities.Enums;

namespace AkilliSehir.Business.Abstract;

/// <summary>
/// Vatandaş kaydı ve rol doğrulamalı giriş işlemleri için uygulama katmanı sözleşmesidir.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Herkese açık vatandaş kaydıdır. Bu metot ile saha personeli hesabı oluşturulamaz.
    /// </summary>
    Task<AuthSonucDto> RegisterCitizenAsync(
        VatandasKayitIstekDto istek,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kimlik bilgilerini ve hedef rolü doğrular. Saha personeli girişinde e-posta kullanılır.
    /// </summary>
    Task<AuthSonucDto> LoginAsync(
        GirisIstekDto istek,
        KullaniciRolu beklenenRol,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yalnızca geliştirme yapılandırmasında tanımlanan başlangıç saha hesabını oluşturur.
    /// Üretimde personel hesapları kurumun yetkili yönetim sürecinden gelir.
    /// </summary>
    Task EnsureDevelopmentStaffAsync(CancellationToken cancellationToken = default);
}
