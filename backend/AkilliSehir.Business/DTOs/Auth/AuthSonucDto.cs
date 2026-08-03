namespace AkilliSehir.Business.DTOs.Auth;

/// <summary>
/// Kimlik doğrulama işlemlerinin istemciye güvenli ve açıklayıcı biçimde dönmesini sağlar.
/// </summary>
public sealed record AuthSonucDto(
    bool Basarili,
    string Mesaj,
    AuthKullaniciDto? Kullanici = null,
    string? Token = null,
    DateTimeOffset? TokenGecerlilikBitis = null)
{
    public static AuthSonucDto Basari(string mesaj, AuthKullaniciDto kullanici) =>
        new(true, mesaj, kullanici);

    public static AuthSonucDto Basarisiz(string mesaj) =>
        new(false, mesaj);
}
