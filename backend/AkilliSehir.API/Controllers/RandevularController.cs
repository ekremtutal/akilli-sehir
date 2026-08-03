using AkilliSehir.API.Security;
using AkilliSehir.Business.Abstract;
using AkilliSehir.Business.Models;
using AkilliSehir.Core.Entities;
using AkilliSehir.Core.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AkilliSehir.API.Controllers;

/// <summary>
/// Vatandaşların belediye birimleri için uygun saat bulup randevu oluşturmasını sağlar.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(KullaniciRolu.Vatandas))]
public class RandevularController : ControllerBase
{
    private readonly IRandevuService _randevuService;

    public RandevularController(IRandevuService randevuService)
    {
        _randevuService = randevuService;
    }

    [HttpGet("benim")]
    [ProducesResponseType(typeof(IReadOnlyList<RandevuDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RandevuDto>>> GetMine(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetKullaniciId(out var vatandasId))
        {
            return Forbid();
        }

        var randevular = await _randevuService.GetByVatandasIdAsync(vatandasId, cancellationToken);
        return Ok(randevular.Select(RandevuDto.FromEntity));
    }

    [HttpGet("uygun-saatler")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetAvailableTimes(
        [FromQuery] BelediyeBirimi birim,
        [FromQuery] DateOnly tarih,
        CancellationToken cancellationToken)
    {
        var saatler = await _randevuService.GetUygunSaatlerAsync(birim, tarih, cancellationToken);
        return Ok(saatler.Select(saat => saat.ToString("HH:mm")));
    }

    [HttpPost]
    [ProducesResponseType(typeof(RandevuDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RandevuDto>> Create(
        [FromBody] RandevuOlusturmaIstek istek,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetKullaniciId(out var vatandasId))
        {
            return Forbid();
        }

        var sonuc = await _randevuService.CreateAsync(
            vatandasId,
            istek.Birim,
            istek.TarihSaat,
            istek.Konu,
            cancellationToken);

        if (!sonuc.Basarili)
        {
            return BadRequest(new { mesaj = sonuc.HataMesaji });
        }

        return CreatedAtAction(
            nameof(GetMine),
            new { id = sonuc.Randevu!.Id },
            RandevuDto.FromEntity(sonuc.Randevu));
    }

    [HttpPut("{id:int}/iptal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetKullaniciId(out var vatandasId))
        {
            return Forbid();
        }

        var sonuc = await _randevuService.CancelAsync(id, vatandasId, cancellationToken);
        return sonuc switch
        {
            RandevuIptalSonucu.Basarili => NoContent(),
            RandevuIptalSonucu.Bulunamadi => NotFound(),
            RandevuIptalSonucu.IptalEdilemez => BadRequest(new
            {
                mesaj = "Randevular en geç iki saat öncesine kadar iptal edilebilir."
            }),
            _ => Forbid()
        };
    }
}

/// <summary>
/// Mobil randevu oluşturma formundan gelen veri sözleşmesidir.
/// </summary>
public sealed class RandevuOlusturmaIstek
{
    public BelediyeBirimi Birim { get; set; }

    public DateTime TarihSaat { get; set; }

    [Required, MinLength(4), MaxLength(300)]
    public string Konu { get; set; } = string.Empty;
}

/// <summary>
/// Vatandaşın kendi randevu kartında ihtiyaç duyduğu alanları taşır.
/// </summary>
public sealed record RandevuDto(
    int Id,
    BelediyeBirimi Birim,
    DateTime TarihSaat,
    string Konu,
    RandevuDurumu Durum)
{
    public static RandevuDto FromEntity(Randevu randevu) => new(
        randevu.Id,
        randevu.Birim,
        randevu.TarihSaat,
        randevu.Konu,
        randevu.Durum);
}
