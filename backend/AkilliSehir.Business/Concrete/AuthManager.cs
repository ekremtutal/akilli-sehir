using System.Security.Cryptography;
using System.Text;
using AkilliSehir.Business.Abstract;
using AkilliSehir.Business.DTOs.Auth;
using AkilliSehir.Core.Entities;
using AkilliSehir.Core.Entities.Enums;
using AkilliSehir.DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AkilliSehir.Business.Concrete;

/// <summary>
/// Parolaları PBKDF2 ile saklayan ve vatandaş/personel rollerini sunucu tarafında doğrulayan
/// kimlik doğrulama servisidir.
/// </summary>
public sealed class AuthManager : IAuthService
{
    private const int ParolaTuzUzunlugu = 16;
    private const int ParolaHashUzunlugu = 32;
    private const int ParolaHashTurSayisi = 600_000;
    private const string ParolaHashEtiketi = "PBKDF2-SHA512";

    private readonly CityDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly byte[] _tcKimlikNoHashAnahtari;

    public AuthManager(CityDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _tcKimlikNoHashAnahtari = TcKimlikNoHashAnahtariniOku(configuration);
    }

    public async Task EnsureDevelopmentStaffAsync(
        CancellationToken cancellationToken = default)
    {
        var ayar = _configuration.GetSection("Bootstrap:SahaPersoneli");
        if (!bool.TryParse(ayar["Etkin"], out var etkin) || !etkin)
        {
            return;
        }

        var email = EpostaNormallestir(ayar["Email"] ?? string.Empty);
        var parola = ayar["Parola"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email) || parola.Length < 8 ||
            await _context.Kullanicilar.AnyAsync(kullanici => kullanici.Email == email, cancellationToken))
        {
            return;
        }

        var birim = Enum.TryParse<BelediyeBirimi>(ayar["Birim"], out var parseEdilenBirim)
            ? parseEdilenBirim
            : BelediyeBirimi.Diger;

        var kullanici = new Kullanici
        {
            AdSoyad = ayar["AdSoyad"] ?? "Geliştirme Saha Personeli",
            Email = email,
            KullaniciAdi = KullaniciAdiNormallestir(ayar["KullaniciAdi"] ?? email),
            ParolaHash = ParolaHashle(parola),
            Rol = KullaniciRolu.SahaPersoneli,
            CalistigiBirim = birim,
            AktifMi = true
        };

        await _context.Kullanicilar.AddAsync(kullanici, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthSonucDto> RegisterCitizenAsync(
        VatandasKayitIstekDto istek,
        CancellationToken cancellationToken = default)
    {
        var adSoyad = istek.AdSoyad.Trim();
        var email = EpostaNormallestir(istek.Email);
        var telefonNumarasi = TelefonNumarasiniNormallestir(istek.TelefonNumarasi);
        var tcKimlikNo = istek.TcKimlikNo.Trim();
        var kullaniciAdi = KullaniciAdiNormallestir(istek.KullaniciAdi);

        // İstemci doğrulaması atlansa bile kullanıcıya hangi alanın düzeltilmesi
        // gerektiğini söyleyen, alan bazlı geri bildirimler üretilir.
        if (adSoyad.Length < 3)
        {
            return AuthSonucDto.Basarisiz("Ad soyad en az 3 karakter olmalıdır.");
        }

        if (!GecerliTelefonNumarasiMi(telefonNumarasi))
        {
            return AuthSonucDto.Basarisiz("Telefon numarası 10 ile 15 rakam arasında olmalıdır.");
        }

        if (!GecerliTcKimlikNoMu(tcKimlikNo))
        {
            return AuthSonucDto.Basarisiz(
                "Geçerli bir 11 haneli T.C. kimlik numarası giriniz.");
        }

        if (kullaniciAdi.Length < 3 || kullaniciAdi.Length > 50 ||
            !KullaniciAdiGecerliMi(kullaniciAdi))
        {
            return AuthSonucDto.Basarisiz(
                "Kullanıcı adı 3-50 karakter olmalı; yalnız harf, rakam, nokta, alt çizgi veya tire içermelidir.");
        }

        if (istek.Parola.Length < 8)
        {
            return AuthSonucDto.Basarisiz("Parola en az 8 karakter olmalıdır.");
        }

        var tcKimlikNoHash = TcKimlikNoHashle(tcKimlikNo);

        // Tekil indeksler eş zamanlı isteklerde son güvenlik katmanıdır; bu sorgular ise
        // kullanıcıya anlaşılır geri bildirim sağlayabilmek içindir.
        if (await _context.Kullanicilar.AnyAsync(
                kullanici => kullanici.KullaniciAdi == kullaniciAdi,
                cancellationToken))
        {
            return AuthSonucDto.Basarisiz("Bu kullanıcı adı zaten kullanılıyor.");
        }

        if (await _context.Kullanicilar.AnyAsync(
                kullanici => kullanici.Email == email,
                cancellationToken))
        {
            return AuthSonucDto.Basarisiz("Bu e-posta adresi zaten kullanılıyor.");
        }

        if (await _context.Kullanicilar.AnyAsync(
                kullanici => kullanici.TcKimlikNoHash == tcKimlikNoHash,
                cancellationToken))
        {
            return AuthSonucDto.Basarisiz("Bu T.C. kimlik numarası ile daha önce kayıt oluşturulmuş.");
        }

        var kullanici = new Kullanici
        {
            AdSoyad = adSoyad,
            Email = email,
            TelefonNumarasi = telefonNumarasi,
            TcKimlikNoHash = tcKimlikNoHash,
            KullaniciAdi = kullaniciAdi,
            ParolaHash = ParolaHashle(istek.Parola),
            // İstemciden hiçbir rol bilgisi alınmaz; açık kayıt yalnızca vatandaşa açıktır.
            Rol = KullaniciRolu.Vatandas,
            AktifMi = true,
        };

        await _context.Kullanicilar.AddAsync(kullanici, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return AuthSonucDto.Basari(
            "Kaydınız oluşturuldu.",
            KullaniciOzetineDonustur(kullanici));
    }

    public async Task<AuthSonucDto> LoginAsync(
        GirisIstekDto istek,
        KullaniciRolu beklenenRol,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(istek.KullaniciAdiVeyaEmail) ||
            string.IsNullOrWhiteSpace(istek.Parola))
        {
            return AuthSonucDto.Basarisiz("Kullanıcı bilgileri hatalı.");
        }

        Kullanici? kullanici;

        if (beklenenRol == KullaniciRolu.Vatandas)
        {
            var kullaniciAdiVeyaEmail = KullaniciAdiVeyaEpostaNormallestir(
                istek.KullaniciAdiVeyaEmail);

            // Vatandaşlar kullanıcı adı veya e-posta ile giriş yapabilir.
            kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(
                kayit => kayit.KullaniciAdi == kullaniciAdiVeyaEmail ||
                         kayit.Email == kullaniciAdiVeyaEmail,
                cancellationToken);
        }
        else
        {
            // Saha personelinin hesabı kurum tarafından tanımlanır; giriş kurumsal e-posta ile yapılır.
            var email = EpostaNormallestir(istek.KullaniciAdiVeyaEmail);
            kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(
                kayit => kayit.Email == email,
                cancellationToken);
        }

        // Hata mesajı kullanıcı adı, e-posta veya parola hakkında ipucu vermez.
        if (kullanici is null || kullanici.Rol != beklenenRol ||
            !ParolaDogrula(istek.Parola, kullanici.ParolaHash))
        {
            return AuthSonucDto.Basarisiz("Kullanıcı bilgileri hatalı.");
        }

        if (!kullanici.AktifMi)
        {
            return AuthSonucDto.Basarisiz("Bu kullanıcı hesabı pasiftir.");
        }

        return AuthSonucDto.Basari(
            "Giriş başarılı.",
            KullaniciOzetineDonustur(kullanici));
    }

    private static AuthKullaniciDto KullaniciOzetineDonustur(Kullanici kullanici) =>
        new(
            kullanici.Id,
            kullanici.AdSoyad,
            kullanici.Email,
            kullanici.KullaniciAdi,
            kullanici.Rol.ToString(),
            kullanici.CalistigiBirim?.ToString());

    private static string ParolaHashle(string parola)
    {
        var tuz = RandomNumberGenerator.GetBytes(ParolaTuzUzunlugu);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            parola,
            tuz,
            ParolaHashTurSayisi,
            HashAlgorithmName.SHA512,
            ParolaHashUzunlugu);

        try
        {
            return string.Join(
                '$',
                ParolaHashEtiketi,
                ParolaHashTurSayisi,
                Convert.ToBase64String(tuz),
                Convert.ToBase64String(hash));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(tuz);
            CryptographicOperations.ZeroMemory(hash);
        }
    }

    private static bool ParolaDogrula(string parola, string saklananHash)
    {
        var bolumler = saklananHash.Split('$', StringSplitOptions.None);

        if (bolumler.Length != 4 || bolumler[0] != ParolaHashEtiketi ||
            !int.TryParse(bolumler[1], out var turSayisi) || turSayisi < 100_000)
        {
            return false;
        }

        try
        {
            var tuz = Convert.FromBase64String(bolumler[2]);
            var beklenenHash = Convert.FromBase64String(bolumler[3]);
            var hesaplananHash = Rfc2898DeriveBytes.Pbkdf2(
                parola,
                tuz,
                turSayisi,
                HashAlgorithmName.SHA512,
                beklenenHash.Length);

            try
            {
                return CryptographicOperations.FixedTimeEquals(hesaplananHash, beklenenHash);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(tuz);
                CryptographicOperations.ZeroMemory(beklenenHash);
                CryptographicOperations.ZeroMemory(hesaplananHash);
            }
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private string TcKimlikNoHashle(string tcKimlikNo)
    {
        var veri = Encoding.UTF8.GetBytes(tcKimlikNo);

        try
        {
            return Convert.ToBase64String(
                HMACSHA256.HashData(_tcKimlikNoHashAnahtari, veri));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(veri);
        }
    }

    private static byte[] TcKimlikNoHashAnahtariniOku(IConfiguration configuration)
    {
        var anahtarMetni = configuration["Auth:TcKimlikNoHashAnahtari"];

        if (string.IsNullOrWhiteSpace(anahtarMetni))
        {
            throw new InvalidOperationException(
                "Auth:TcKimlikNoHashAnahtari yapılandırması tanımlanmalıdır.");
        }

        try
        {
            var anahtar = Convert.FromBase64String(anahtarMetni);

            if (anahtar.Length < 32)
            {
                throw new InvalidOperationException(
                    "Auth:TcKimlikNoHashAnahtari en az 32 bayt olmalıdır.");
            }

            return anahtar;
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "Auth:TcKimlikNoHashAnahtari Base64 formatında olmalıdır.",
                exception);
        }
    }

    private static string EpostaNormallestir(string email) => email.Trim().ToLowerInvariant();

    private static string KullaniciAdiNormallestir(string kullaniciAdi) =>
        kullaniciAdi.Trim().ToLowerInvariant();

    private static string KullaniciAdiVeyaEpostaNormallestir(string deger) =>
        deger.Trim().ToLowerInvariant();

    private static bool KullaniciAdiGecerliMi(string kullaniciAdi) =>
        kullaniciAdi.All(character =>
            char.IsAsciiLetterOrDigit(character) || character is '.' or '_' or '-');

    private static string TelefonNumarasiniNormallestir(string telefonNumarasi) =>
        new(telefonNumarasi.Where(char.IsDigit).ToArray());

    private static bool GecerliTelefonNumarasiMi(string telefonNumarasi) =>
        telefonNumarasi.Length is >= 10 and <= 15;

    private static bool GecerliTcKimlikNoMu(string tcKimlikNo)
    {
        if (tcKimlikNo.Length != 11 || tcKimlikNo[0] == '0' ||
            tcKimlikNo.Any(hane => hane is < '0' or > '9'))
        {
            return false;
        }

        var haneler = tcKimlikNo.Select(hane => hane - '0').ToArray();
        var tekHanelerToplami = haneler[0] + haneler[2] + haneler[4] + haneler[6] + haneler[8];
        var ciftHanelerToplami = haneler[1] + haneler[3] + haneler[5] + haneler[7];
        var onuncuHane = ((tekHanelerToplami * 7) - ciftHanelerToplami) % 10;

        if (onuncuHane < 0)
        {
            onuncuHane += 10;
        }

        var onBirinciHane = haneler.Take(10).Sum() % 10;
        return haneler[9] == onuncuHane && haneler[10] == onBirinciHane;
    }
}
