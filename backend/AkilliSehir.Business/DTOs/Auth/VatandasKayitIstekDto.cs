using System.ComponentModel.DataAnnotations;

namespace AkilliSehir.Business.DTOs.Auth;

/// <summary>
/// Yalnızca vatandaşların kendi hesaplarını oluştururken gönderebildiği veridir.
/// Rol bilgisi istemci tarafından alınmaz; servis tarafında her zaman Vatandas atanır.
/// </summary>
public sealed class VatandasKayitIstekDto
{
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Ad soyad 3 ile 150 karakter arasında olmalıdır.")]
    public string AdSoyad { get; init; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    public string TelefonNumarasi { get; init; } = string.Empty;

    [Required(ErrorMessage = "T.C. kimlik numarası zorunludur.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "T.C. kimlik numarası 11 haneden oluşmalıdır.")]
    public string TcKimlikNo { get; init; } = string.Empty;

    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3 ile 50 karakter arasında olmalıdır.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Kullanıcı adı yalnızca harf, rakam, nokta, alt çizgi ve tire içerebilir.")]
    public string KullaniciAdi { get; init; } = string.Empty;

    [Required(ErrorMessage = "Parola zorunludur.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Parola en az 8 karakter olmalıdır.")]
    public string Parola { get; init; } = string.Empty;
}
