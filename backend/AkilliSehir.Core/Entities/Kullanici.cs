using AkilliSehir.Core.Entities.Enums;

namespace AkilliSehir.Core.Entities;

/// <summary>
/// Vatandaş, saha personeli veya birim amiri kullanıcı kaydını temsil eder.
/// </summary>
public class Kullanici
{
    public int Id { get; set; }

    public string AdSoyad { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // Vatandaş ve personelin oturum açarken kullanacağı benzersiz kullanıcı adı.
    public string KullaniciAdi { get; set; } = string.Empty;

    // Ham parola hiçbir zaman saklanmaz; yalnızca güvenli PBKDF2 özeti tutulur.
    public string ParolaHash { get; set; } = string.Empty;

    // Vatandaş kaydında zorunludur; personel için kurum kaydından gelebilir.
    public string? TelefonNumarasi { get; set; }

    // TC kimlik numarasının ham değeri değil, tek yönlü özeti tutulur.
    public string? TcKimlikNoHash { get; set; }

    public KullaniciRolu Rol { get; set; }

    // Saha personelinin görev göreceği belediye birimi.
    public BelediyeBirimi? CalistigiBirim { get; set; }

    public bool AktifMi { get; set; } = true;

    // Kullanıcıya atanmış arıza kayıtları için 1-N navigation property'si.
    public ICollection<Ariza> AtananArizalar { get; set; } = new List<Ariza>();

    // Vatandaşın oluşturduğu arıza kayıtları için 1-N navigation property'si.
    public ICollection<Ariza> BildirilenArizalar { get; set; } = new List<Ariza>();

    // Vatandaşın oluşturduğu belediye randevuları için 1-N navigation property'si.
    public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
}
