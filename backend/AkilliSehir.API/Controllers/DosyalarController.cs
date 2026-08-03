using AkilliSehir.Core.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliSehir.API.Controllers;

/// <summary>
/// Arıza kayıtlarına ait görsellerin güvenli biçimde yüklenmesini sağlar.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(KullaniciRolu.Vatandas))]
public sealed class DosyalarController : ControllerBase
{
    private const long MaksimumDosyaBoyutu = 5 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> IzinVerilenGorselTurleri =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private readonly IWebHostEnvironment _environment;

    public DosyalarController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Arıza fotoğrafını yükler ve istemcinin arıza oluşturma isteğinde kullanacağı göreli URL'yi döndürür.
    /// </summary>
    [HttpPost("ariza-fotografi")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaksimumDosyaBoyutu)]
    [ProducesResponseType(typeof(ArizaFotografiYuklemeSonucu), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<ArizaFotografiYuklemeSonucu>> ArizaFotografiYukle(
        // IFormFile, ASP.NET Core tarafından otomatik olarak multipart/form-data
        // kaynağından bağlanır. [FromForm] eklenmediğinde Swagger bu alanı doğru
        // şekilde dosya seçici olarak belgeleyebilir.
        IFormFile? fotograf,
        CancellationToken cancellationToken)
    {
        if (fotograf is null || fotograf.Length == 0)
        {
            return DogrulamaHatasi("fotograf", "Yüklenecek fotoğraf boş olamaz.");
        }

        if (fotograf.Length > MaksimumDosyaBoyutu)
        {
            return DogrulamaHatasi(
                "fotograf",
                "Fotoğraf boyutu en fazla 5 MB olabilir.");
        }

        var uzanti = Path.GetExtension(fotograf.FileName).ToLowerInvariant();
        if (!IzinVerilenGorselTurleri.TryGetValue(uzanti, out var beklenenIcerikTuru))
        {
            return DogrulamaHatasi(
                "fotograf",
                "Yalnızca JPG, JPEG, PNG veya WEBP formatında fotoğraf yükleyebilirsiniz.");
        }

        var gelenIcerikTuru = fotograf.ContentType?
            .Split(';', 2, StringSplitOptions.TrimEntries)[0] ?? string.Empty;
        if (!beklenenIcerikTuru.Equals(gelenIcerikTuru, StringComparison.OrdinalIgnoreCase))
        {
            return DogrulamaHatasi(
                "fotograf",
                "Fotoğrafın dosya uzantısı ile içerik türü uyuşmuyor.");
        }

        if (!await GecerliGorselImzasiMiAsync(fotograf, uzanti, cancellationToken))
        {
            return DogrulamaHatasi(
                "fotograf",
                "Yüklenen dosya geçerli bir görsel imzası taşımıyor.");
        }

        var webKokDizini = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;
        var hedefDizin = Path.Combine(webKokDizini, "uploads", "arizalar");
        Directory.CreateDirectory(hedefDizin);

        // İstemcinin gönderdiği dosya adı hiç kullanılmaz; tahmin edilemeyen benzersiz ad üretilir.
        var dosyaAdi = $"{Guid.NewGuid():N}{uzanti}";
        var hedefDosyaYolu = Path.Combine(hedefDizin, dosyaAdi);

        try
        {
            await using var hedefAkis = new FileStream(
                hedefDosyaYolu,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            await fotograf.CopyToAsync(hedefAkis, cancellationToken);
        }
        catch
        {
            GuvenliDosyaSil(hedefDosyaYolu);
            throw;
        }

        var fotografUrl = $"/uploads/arizalar/{dosyaAdi}";
        var sonuc = new ArizaFotografiYuklemeSonucu(fotografUrl);

        return Created(fotografUrl, sonuc);
    }

    private static ActionResult<ArizaFotografiYuklemeSonucu> DogrulamaHatasi(
        string alan,
        string hata)
    {
        return new BadRequestObjectResult(new ValidationProblemDetails(
            new Dictionary<string, string[]>
            {
                [alan] = [hata]
            }));
    }

    private static async Task<bool> GecerliGorselImzasiMiAsync(
        IFormFile fotograf,
        string uzanti,
        CancellationToken cancellationToken)
    {
        const int GerekliBaslikUzunlugu = 12;
        var baslik = new byte[GerekliBaslikUzunlugu];
        var okunanBaytSayisi = 0;

        await using var kaynakAkis = fotograf.OpenReadStream();
        while (okunanBaytSayisi < baslik.Length)
        {
            var buTurdeOkunanBaytSayisi = await kaynakAkis.ReadAsync(
                baslik.AsMemory(okunanBaytSayisi),
                cancellationToken);

            if (buTurdeOkunanBaytSayisi == 0)
            {
                break;
            }

            okunanBaytSayisi += buTurdeOkunanBaytSayisi;
        }

        return uzanti switch
        {
            ".jpg" or ".jpeg" => okunanBaytSayisi >= 3
                && baslik[0] == 0xFF
                && baslik[1] == 0xD8
                && baslik[2] == 0xFF,
            ".png" => okunanBaytSayisi >= 8
                && baslik[0] == 0x89
                && baslik[1] == 0x50
                && baslik[2] == 0x4E
                && baslik[3] == 0x47
                && baslik[4] == 0x0D
                && baslik[5] == 0x0A
                && baslik[6] == 0x1A
                && baslik[7] == 0x0A,
            ".webp" => okunanBaytSayisi >= 12
                && baslik[0] == 0x52
                && baslik[1] == 0x49
                && baslik[2] == 0x46
                && baslik[3] == 0x46
                && baslik[8] == 0x57
                && baslik[9] == 0x45
                && baslik[10] == 0x42
                && baslik[11] == 0x50,
            _ => false
        };
    }

    private static void GuvenliDosyaSil(string dosyaYolu)
    {
        try
        {
            if (System.IO.File.Exists(dosyaYolu))
            {
                System.IO.File.Delete(dosyaYolu);
            }
        }
        catch (IOException)
        {
            // Birincil yükleme hatasını gölgelememek için temizlik hatası bastırılır.
        }
    }
}

/// <summary>
/// Fotoğraf yükleme işleminin istemciye dönen sonucudur.
/// </summary>
public sealed record ArizaFotografiYuklemeSonucu(string FotografUrl);
