using System.ComponentModel.DataAnnotations;

namespace AkilliSehir.Business.DTOs.Auth;

/// <summary>
/// Vatandaş ve saha personeli girişlerinde kullanılan parola bilgisidir.
/// Vatandaşlar kullanıcı adı veya e-posta, personeller ise kurumsal e-posta ile giriş yapar.
/// </summary>
public sealed class GirisIstekDto
{
    [Required(ErrorMessage = "Kullanıcı adı veya e-posta zorunludur.")]
    [StringLength(200)]
    public string KullaniciAdiVeyaEmail { get; init; } = string.Empty;

    [Required(ErrorMessage = "Parola zorunludur.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Parola en az 8 karakter olmalıdır.")]
    public string Parola { get; init; } = string.Empty;
}
