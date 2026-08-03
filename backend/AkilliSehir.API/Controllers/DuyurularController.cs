using AkilliSehir.Business.Abstract;
using AkilliSehir.Core.Entities;
using AkilliSehir.Core.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliSehir.API.Controllers;

/// <summary>
/// Vatandaşa şehir geneli veya yakın çevresindeki aktif duyuruları sunar.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(KullaniciRolu.Vatandas))]
public class DuyurularController : ControllerBase
{
    private readonly IDuyuruService _duyuruService;

    public DuyurularController(IDuyuruService duyuruService)
    {
        _duyuruService = duyuruService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DuyuruDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DuyuruDto>>> GetActive(
        [FromQuery] double? enlem,
        [FromQuery] double? boylam,
        CancellationToken cancellationToken)
    {
        // Konum isteğe bağlıdır. İstemci paylaşmazsa yalnızca genel akış sunulur;
        // kullanıcı izni olmadan konum verisi kalıcı olarak saklanmaz.
        var duyurular = await _duyuruService.GetActiveAsync(enlem, boylam, cancellationToken);
        return Ok(duyurular.Select(DuyuruDto.FromEntity));
    }
}

/// <summary>
/// Mobil ekrana gereken, hassas yönetim alanlarını içermeyen duyuru görünümüdür.
/// </summary>
public sealed record DuyuruDto(
    int Id,
    string Baslik,
    string Icerik,
    string Kategori,
    string Oncelik,
    DateTime YayinBaslangicTarihi,
    DateTime? YayinBitisTarihi)
{
    public static DuyuruDto FromEntity(Duyuru duyuru) => new(
        duyuru.Id,
        duyuru.Baslik,
        duyuru.Icerik,
        duyuru.Kategori,
        duyuru.Oncelik,
        duyuru.YayinBaslangicTarihi,
        duyuru.YayinBitisTarihi);
}
