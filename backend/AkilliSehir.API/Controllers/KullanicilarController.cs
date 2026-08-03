using AkilliSehir.Business.Abstract;
using AkilliSehir.Core.Entities;
using AkilliSehir.Core.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliSehir.API.Controllers;

/// <summary>
/// Kullanıcı kayıtları için HTTP uç noktalarını sunar.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(KullaniciRolu.BirimAmiri))]
public class KullanicilarController : ControllerBase
{
    private readonly IKullaniciService _kullaniciService;

    public KullanicilarController(IKullaniciService kullaniciService)
    {
        _kullaniciService = kullaniciService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<KullaniciOzetDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var kullanicilar = await _kullaniciService.GetAllAsync(cancellationToken);
        return Ok(kullanicilar.Select(KullaniciOzetDto.FromEntity));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<KullaniciOzetDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var kullanici = await _kullaniciService.GetByIdAsync(id, cancellationToken);
        return kullanici is null ? NotFound() : Ok(KullaniciOzetDto.FromEntity(kullanici));
    }
}

/// <summary>
/// Hassas kimlik doğrulama alanlarını dışarı vermeyen kullanıcı görünümüdür.
/// </summary>
public sealed record KullaniciOzetDto(
    int Id,
    string AdSoyad,
    string Email,
    string Rol,
    string? CalistigiBirim)
{
    public static KullaniciOzetDto FromEntity(Kullanici kullanici) => new(
        kullanici.Id,
        kullanici.AdSoyad,
        kullanici.Email,
        kullanici.Rol.ToString(),
        kullanici.CalistigiBirim?.ToString());
}
