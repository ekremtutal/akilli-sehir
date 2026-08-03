using AkilliSehir.Core.Entities.Enums;

namespace AkilliSehir.Core.Entities;

/// <summary>
/// Vatandaş ile ilgili belediye birimi arasındaki dijital randevu kaydıdır.
/// </summary>
public class Randevu
{
    public int Id { get; set; }

    public int VatandasId { get; set; }

    public Kullanici Vatandas { get; set; } = null!;

    public BelediyeBirimi Birim { get; set; }

    public DateTime TarihSaat { get; set; }

    public string Konu { get; set; } = string.Empty;

    public RandevuDurumu Durum { get; set; } = RandevuDurumu.Planlandi;

    public DateTime KayitTarihi { get; set; }
}
