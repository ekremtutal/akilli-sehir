using AkilliSehir.API.Security;
using AkilliSehir.Business.Abstract;
using AkilliSehir.Business.DTOs.Auth;
using AkilliSehir.Core.Entities.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AkilliSehir.API.Controllers;

/// <summary>
/// Vatandaş kaydı ile vatandaş ve saha personeli giriş uç noktalarını sunar.
/// Saha personeli hesabı bu controller üzerinden oluşturulamaz.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        IAuthService authService,
        IJwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Yeni bir vatandaş hesabı oluşturur.
    /// </summary>
    [HttpPost("vatandas-kayit")]
    [ProducesResponseType(typeof(AuthSonucDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(AuthSonucDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthSonucDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthSonucDto>> VatandasKayit(
        [FromBody] VatandasKayitIstekDto istek,
        CancellationToken cancellationToken)
    {
        var sonuc = await _authService.RegisterCitizenAsync(istek, cancellationToken);

        if (sonuc.Basarili)
        {
            return StatusCode(StatusCodes.Status201Created, TokenliSonuc(sonuc));
        }

        // İş kuralı çakışmaları (tekrarlanan kullanıcı adı, e-posta veya T.C.) için 409 döner.
        return sonuc.Mesaj.Contains("zaten", StringComparison.OrdinalIgnoreCase)
            ? Conflict(sonuc)
            : BadRequest(sonuc);
    }

    /// <summary>
    /// Vatandaşın kullanıcı adı veya e-posta ile giriş yapmasını sağlar.
    /// </summary>
    [HttpPost("vatandas-giris")]
    [ProducesResponseType(typeof(AuthSonucDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthSonucDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthSonucDto>> VatandasGiris(
        [FromBody] GirisIstekDto istek,
        CancellationToken cancellationToken)
    {
        var sonuc = await _authService.LoginAsync(
            istek,
            KullaniciRolu.Vatandas,
            cancellationToken);

        return sonuc.Basarili ? Ok(TokenliSonuc(sonuc)) : Unauthorized(sonuc);
    }

    /// <summary>
    /// Kurum tarafından tanımlanmış saha personelinin e-posta ile giriş yapmasını sağlar.
    /// </summary>
    [HttpPost("personel-giris")]
    [ProducesResponseType(typeof(AuthSonucDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthSonucDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthSonucDto>> PersonelGiris(
        [FromBody] GirisIstekDto istek,
        CancellationToken cancellationToken)
    {
        var sonuc = await _authService.LoginAsync(
            istek,
            KullaniciRolu.SahaPersoneli,
            cancellationToken);

        return sonuc.Basarili ? Ok(TokenliSonuc(sonuc)) : Unauthorized(sonuc);
    }

    /// <summary>
    /// Başarılı kayıt ve girişlerde istemcinin korumalı uç noktalara erişebilmesi
    /// için kısa ömürlü imzalı erişim token'ı ekler.
    /// </summary>
    private AuthSonucDto TokenliSonuc(AuthSonucDto sonuc)
    {
        if (!sonuc.Basarili || sonuc.Kullanici is null)
        {
            return sonuc;
        }

        var token = _jwtTokenService.Olustur(sonuc.Kullanici);
        return sonuc with
        {
            Token = token.Token,
            TokenGecerlilikBitis = token.GecerlilikBitis
        };
    }
}
