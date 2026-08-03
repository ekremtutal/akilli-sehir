using AkilliSehir.API.Security;
using AkilliSehir.Business.Abstract;
using AkilliSehir.Business.Models;
using AkilliSehir.Core.Entities;
using AkilliSehir.Core.Entities.Enums;
using AkilliSehir.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AkilliSehir.API.Controllers;

/// <summary>
/// Arıza kayıtları için HTTP uç noktalarını sunar.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ArizalarController : ControllerBase
{
    private readonly IArizaService _arizaService;
    private readonly IArizaYonlendirmeService _yonlendirmeService;

    public ArizalarController(
        IArizaService arizaService,
        IArizaYonlendirmeService yonlendirmeService)
    {
        _arizaService = arizaService;
        _yonlendirmeService = yonlendirmeService;
    }

    [HttpGet]
    [Authorize(Roles = nameof(KullaniciRolu.SahaPersoneli))]
    [ProducesResponseType(typeof(IReadOnlyList<ArizaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ArizaDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        // İstemcinin query string ile birim seçmesine güvenilmez; yalnızca JWT içindeki
        // personel birimine yönlendirilmiş kayıtlar döndürülür.
        if (!User.TryGetCalistigiBirim(out var personelinBirimi))
        {
            return Forbid();
        }

        var arizalar = await _arizaService.GetAllAsync(personelinBirimi, cancellationToken);
        return Ok(arizalar.Select(ArizaDto.FromEntity));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = nameof(KullaniciRolu.SahaPersoneli))]
    [ProducesResponseType(typeof(ArizaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ArizaDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCalistigiBirim(out var personelinBirimi))
        {
            return Forbid();
        }

        var ariza = await _arizaService.GetByIdAsync(id, cancellationToken);
        // Başka birimin kaydının varlığı hakkında dahi bilgi vermemek için 404 dönülür.
        return ariza is null || ariza.YonlendirilenBirim != personelinBirimi
            ? NotFound()
            : Ok(ArizaDto.FromEntity(ariza));
    }

    [HttpPost]
    [Authorize(Roles = nameof(KullaniciRolu.Vatandas))]
    [ProducesResponseType(typeof(ArizaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ArizaDto>> Add(
        [FromBody] ArizaOlusturmaIstek istek,
        CancellationToken cancellationToken)
    {
        // Yönlendirme kararı; metin, kategori, vatandaş tercihi ve fotoğraf URL'si
        // üzerinden uygulama katmanında üretilir.
        var yonlendirme = await _yonlendirmeService.YonlendirAsync(
            new ArizaYonlendirmeIstek
            {
                Baslik = istek.Baslik,
                Aciklama = istek.Aciklama,
                SecilenArizaTuru = istek.ArizaTuru,
                VatandasSecilenBirim = istek.VatandasSecilenBirim,
                FotografUrl = istek.FotografUrl
            },
            cancellationToken);

        var ariza = new Ariza
        {
            Baslik = istek.Baslik,
            Aciklama = istek.Aciklama,
            Enlem = istek.Enlem,
            Boylam = istek.Boylam,
            FotografUrl = istek.FotografUrl ?? string.Empty,
            Aciliyet = istek.Aciliyet,
            Durum = ArizaDurumu.Beklemede,
            ArizaTuru = istek.ArizaTuru,
            VatandasSecilenBirim = istek.VatandasSecilenBirim,
            YonlendirilenBirim = yonlendirme.OnerilenBirim,
            YapayZekaGuvenSkoru = yonlendirme.GuvenSkoru,
            YapayZekaGerekcesi = yonlendirme.Gerekce
        };

        var olusturulanAriza = await _arizaService.AddAsync(ariza, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = olusturulanAriza.Id },
            ArizaDto.FromEntity(olusturulanAriza));
    }

    [HttpPut("{id:int}/durum")]
    [Authorize(Roles = nameof(KullaniciRolu.SahaPersoneli))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] ArizaDurumGuncellemeIstek istek,
        CancellationToken cancellationToken)
    {
        // Atanan personel kimliği HTTP isteğinden alınmaz. İmzalı JWT claim'i dışarıdan
        // değiştirilemeyeceği için bir saha personeli başka biri adına görev alamaz.
        if (!User.TryGetKullaniciId(out var personelId) ||
            !User.TryGetCalistigiBirim(out var personelinBirimi))
        {
            return Forbid();
        }

        var sonuc = await _arizaService.UpdateStatusAsync(
            id,
            istek.Durum,
            personelId,
            personelinBirimi,
            cancellationToken);

        return sonuc switch
        {
            ArizaDurumGuncellemeSonucu.Basarili => NoContent(),
            ArizaDurumGuncellemeSonucu.Bulunamadi => NotFound(),
            _ => Forbid()
        };
    }
}

/// <summary>
/// Arıza durum güncelleme isteğinin veri modelidir.
/// </summary>
public sealed class ArizaDurumGuncellemeIstek
{
    public ArizaDurumu Durum { get; set; }
}

/// <summary>
/// Vatandaşın mobil formdan gönderdiği arıza oluşturma isteğidir.
/// </summary>
public sealed class ArizaOlusturmaIstek
{
    [Required, MaxLength(200)]
    public string Baslik { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Aciklama { get; set; } = string.Empty;

    public double Enlem { get; set; }

    public double Boylam { get; set; }

    [MaxLength(500)]
    public string? FotografUrl { get; set; }

    public ArizaTuru ArizaTuru { get; set; }

    public BelediyeBirimi? VatandasSecilenBirim { get; set; }

    public AciliyetSeviyesi Aciliyet { get; set; } = AciliyetSeviyesi.Orta;
}

/// <summary>
/// Arıza yanıtında yalnızca ekrana gerekli verileri döndürür. Atanan personele
/// ait parola özeti, T.C. özeti ve telefon numarası hiçbir koşulda serileştirilmez.
/// </summary>
public sealed record ArizaDto(
    int Id,
    string Baslik,
    string Aciklama,
    double Enlem,
    double Boylam,
    string FotografUrl,
    ArizaDurumu Durum,
    AciliyetSeviyesi Aciliyet,
    ArizaTuru ArizaTuru,
    BelediyeBirimi? VatandasSecilenBirim,
    BelediyeBirimi YonlendirilenBirim,
    decimal? YapayZekaGuvenSkoru,
    string? YapayZekaGerekcesi,
    DateTime KayitTarihi,
    int? AtananPersonelId,
    AtananPersonelOzetDto? AtananPersonel)
{
    public static ArizaDto FromEntity(Ariza ariza) => new(
        ariza.Id,
        ariza.Baslik,
        ariza.Aciklama,
        ariza.Enlem,
        ariza.Boylam,
        ariza.FotografUrl,
        ariza.Durum,
        ariza.Aciliyet,
        ariza.ArizaTuru,
        ariza.VatandasSecilenBirim,
        ariza.YonlendirilenBirim,
        ariza.YapayZekaGuvenSkoru,
        ariza.YapayZekaGerekcesi,
        ariza.KayitTarihi,
        ariza.AtananPersonelId,
        ariza.AtananPersonel is null
            ? null
            : AtananPersonelOzetDto.FromEntity(ariza.AtananPersonel));
}

/// <summary>
/// Arıza detayında gösterilebilen, hassas alanları dışarıda bırakan personel özetidir.
/// </summary>
public sealed record AtananPersonelOzetDto(
    int Id,
    string AdSoyad,
    string Email,
    string? CalistigiBirim)
{
    public static AtananPersonelOzetDto FromEntity(Kullanici kullanici) => new(
        kullanici.Id,
        kullanici.AdSoyad,
        kullanici.Email,
        kullanici.CalistigiBirim?.ToString());
}
