using AkilliSehir.Business.DTOs.Auth;

namespace AkilliSehir.API.Security;

/// <summary>
/// Kimliği doğrulanmış kullanıcı için imzalı erişim token'ı üretir.
/// </summary>
public interface IJwtTokenService
{
    JwtTokenSonucu Olustur(AuthKullaniciDto kullanici);
}

/// <summary>
/// İstemciye döndürülecek erişim token'ı ve geçerlilik bilgisidir.
/// </summary>
public sealed record JwtTokenSonucu(string Token, DateTimeOffset GecerlilikBitis);
