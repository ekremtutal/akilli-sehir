using System.Security.Claims;
using AkilliSehir.Core.Entities.Enums;

namespace AkilliSehir.API.Security;

/// <summary>
/// Doğrulanmış JWT claim'lerini güvenli türlere dönüştüren yardımcı metotlardır.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static bool TryGetKullaniciId(this ClaimsPrincipal kullanici, out int kullaniciId)
    {
        return int.TryParse(
            kullanici.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            out kullaniciId) && kullaniciId > 0;
    }

    public static bool TryGetCalistigiBirim(
        this ClaimsPrincipal kullanici,
        out BelediyeBirimi birim)
    {
        var claimDegeri = kullanici.FindFirst(JwtClaimTurleri.CalistigiBirim)?.Value;
        return Enum.TryParse(claimDegeri, ignoreCase: false, out birim) &&
               birim != BelediyeBirimi.Bilinmiyor;
    }
}
