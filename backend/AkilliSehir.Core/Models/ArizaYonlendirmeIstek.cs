using AkilliSehir.Core.Entities.Enums;

namespace AkilliSehir.Core.Models;

/// <summary>
/// Bir arıza kaydının sorumlu belediye birimine yönlendirilmesi için gereken girdileri taşır.
/// </summary>
public sealed class ArizaYonlendirmeIstek
{
    /// <summary>
    /// Vatandaşın yazdığı kısa arıza başlığıdır.
    /// </summary>
    public string Baslik { get; init; } = string.Empty;

    /// <summary>
    /// Vatandaşın yazdığı ayrıntılı arıza açıklamasıdır.
    /// </summary>
    public string Aciklama { get; init; } = string.Empty;

    /// <summary>
    /// Vatandaşın formda seçtiği arıza türüdür; seçilmediyse null kalabilir.
    /// </summary>
    public ArizaTuru? SecilenArizaTuru { get; init; }

    /// <summary>
    /// Vatandaşın yönlendirilmesini istediği birimdir; karar motoru bunu destekleyici sinyal olarak kullanır.
    /// </summary>
    public BelediyeBirimi? VatandasSecilenBirim { get; init; }

    /// <summary>
    /// Arızaya ait görselin erişilebilir adresidir. Gerçek bir görsel analiz sağlayıcısına aktarılabilir.
    /// </summary>
    public string? FotografUrl { get; init; }
}
