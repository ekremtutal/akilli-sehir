namespace AkilliSehir.Core.Entities;

/// <summary>
/// Belirli bir konuma veya tüm şehre yayınlanabilen belediye duyurusudur.
/// </summary>
public class Duyuru
{
    public int Id { get; set; }

    public string Baslik { get; set; } = string.Empty;

    public string Icerik { get; set; } = string.Empty;

    // Örnek: SuKesintisi, YolCalismasi, Etkinlik, AcilDurum.
    public string Kategori { get; set; } = string.Empty;

    // Bilgi, Onemli veya Acil değerleri mobil arayüzde öncelik etiketi olarak gösterilir.
    public string Oncelik { get; set; } = "Bilgi";

    // Konum belirtilmezse duyuru Adana genelinde görünür.
    public double? KonumEnlem { get; set; }

    public double? KonumBoylam { get; set; }

    public int KapsamYaricapiMetre { get; set; } = 5000;

    public DateTime YayinBaslangicTarihi { get; set; }

    public DateTime? YayinBitisTarihi { get; set; }

    public bool AktifMi { get; set; } = true;
}
