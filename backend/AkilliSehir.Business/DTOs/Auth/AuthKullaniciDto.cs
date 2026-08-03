namespace AkilliSehir.Business.DTOs.Auth;

/// <summary>
/// Giriş sonrasında istemciye döndürülen, parola ve hassas kimlik bilgisi içermeyen kullanıcı özetidir.
/// </summary>
public sealed record AuthKullaniciDto(
    int Id,
    string AdSoyad,
    string Email,
    string KullaniciAdi,
    string Rol,
    string? CalistigiBirim);
