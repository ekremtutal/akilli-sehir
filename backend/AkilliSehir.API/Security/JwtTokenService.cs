using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AkilliSehir.Business.DTOs.Auth;
using Microsoft.IdentityModel.Tokens;

namespace AkilliSehir.API.Security;

/// <summary>
/// HMAC-SHA256 ile imzalanan kısa ömürlü JWT erişim token'larını üretir.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _imzalamaAnahtari;
    private readonly TimeSpan _gecerlilikSuresi;

    public JwtTokenService(IConfiguration configuration)
    {
        _issuer = configuration["Auth:Jwt:Issuer"]
            ?? throw new InvalidOperationException("Auth:Jwt:Issuer yapılandırması tanımlanmalıdır.");
        _audience = configuration["Auth:Jwt:Audience"]
            ?? throw new InvalidOperationException("Auth:Jwt:Audience yapılandırması tanımlanmalıdır.");
        var anahtar = configuration["Auth:Jwt:Anahtar"]
            ?? throw new InvalidOperationException(
                "Auth:Jwt:Anahtar yapılandırması ortam değişkeni veya gizli depoda tanımlanmalıdır.");

        var anahtarBaytSayisi = Encoding.UTF8.GetByteCount(anahtar);
        if (anahtarBaytSayisi < 32)
        {
            throw new InvalidOperationException("Auth:Jwt:Anahtar en az 32 bayt uzunluğunda olmalıdır.");
        }

        var gecerlilikDakikasi = int.TryParse(
            configuration["Auth:Jwt:TokenGecerlilikDakikasi"],
            out var ayarlananGecerlilikDakikasi)
            ? ayarlananGecerlilikDakikasi
            : 60;
        if (gecerlilikDakikasi is < 5 or > 1_440)
        {
            throw new InvalidOperationException(
                "Auth:Jwt:TokenGecerlilikDakikasi 5 ile 1440 arasında olmalıdır.");
        }

        _imzalamaAnahtari = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(anahtar));
        _gecerlilikSuresi = TimeSpan.FromMinutes(gecerlilikDakikasi);
    }

    public JwtTokenSonucu Olustur(AuthKullaniciDto kullanici)
    {
        ArgumentNullException.ThrowIfNull(kullanici);

        var simdi = DateTimeOffset.UtcNow;
        var gecerlilikBitis = simdi.Add(_gecerlilikSuresi);
        var claimler = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, kullanici.Id.ToString()),
            new(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
            new(ClaimTypes.Name, kullanici.AdSoyad),
            new(ClaimTypes.Email, kullanici.Email),
            new(ClaimTypes.Role, kullanici.Rol),
            new(JwtRegisteredClaimNames.UniqueName, kullanici.KullaniciAdi)
        };

        if (!string.IsNullOrWhiteSpace(kullanici.CalistigiBirim))
        {
            claimler.Add(new Claim(JwtClaimTurleri.CalistigiBirim, kullanici.CalistigiBirim));
        }

        var kimlikBilgileri = new SigningCredentials(
            _imzalamaAnahtari,
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claimler,
            notBefore: simdi.UtcDateTime,
            expires: gecerlilikBitis.UtcDateTime,
            signingCredentials: kimlikBilgileri);

        return new JwtTokenSonucu(
            new JwtSecurityTokenHandler().WriteToken(token),
            gecerlilikBitis);
    }
}
