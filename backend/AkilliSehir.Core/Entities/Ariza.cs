using AkilliSehir.Core.Entities.Enums;

namespace AkilliSehir.Core.Entities;

/// <summary>
/// Vatandaş tarafından iletilen şehir arıza kaydını temsil eder.
/// </summary>
public class Ariza
{
    public int Id { get; set; }

    public string Baslik { get; set; } = string.Empty;

    public string Aciklama { get; set; } = string.Empty;

    public double Enlem { get; set; }

    public double Boylam { get; set; }

    public string FotografUrl { get; set; } = string.Empty;

    public ArizaDurumu Durum { get; set; }

    public AciliyetSeviyesi Aciliyet { get; set; }

    // Vatandaşın formda seçtiği problem türü.
    public ArizaTuru ArizaTuru { get; set; }

    // Vatandaşın tercih ettiği birim, yönlendirme motoru için destekleyici sinyaldir.
    public BelediyeBirimi? VatandasSecilenBirim { get; set; }

    // Akıllı yönlendirme sonucunda sorumlu kabul edilen belediye birimi.
    public BelediyeBirimi YonlendirilenBirim { get; set; }

    // Yönlendirme kararına ait 0-1 aralığındaki güven değeri.
    public decimal? YapayZekaGuvenSkoru { get; set; }

    // Yönlendirme kararının açıklanabilir kısa özeti.
    public string? YapayZekaGerekcesi { get; set; }

    public DateTime KayitTarihi { get; set; }

    // Bildirimin sahibi, vatandaşın yalnızca kendi kayıtlarını takip edebilmesi
    // için sunucuda doğrulanmış JWT kimliğiyle ilişkilendirilir.
    public int? BildirimiYapanVatandasId { get; set; }

    public Kullanici? BildirimiYapanVatandas { get; set; }

    // Atama yapılmadığında null kalabilen yabancı anahtar.
    public int? AtananPersonelId { get; set; }

    // Arızanın atandığı saha personeli için navigation property'si.
    public Kullanici? AtananPersonel { get; set; }
}
