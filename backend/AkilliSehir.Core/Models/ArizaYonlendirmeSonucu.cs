using AkilliSehir.Core.Entities.Enums;

namespace AkilliSehir.Core.Models;

/// <summary>
/// Yönlendirme motorunun önerisini ve kararın açıklanabilir özetini taşır.
/// </summary>
public sealed class ArizaYonlendirmeSonucu
{
    /// <summary>
    /// Arızanın gönderilmesi önerilen belediye birimidir.
    /// </summary>
    public required BelediyeBirimi OnerilenBirim { get; init; }

    /// <summary>
    /// Kararın güven skoru 0 ile 1 arasındadır.
    /// </summary>
    public required decimal GuvenSkoru { get; init; }

    /// <summary>
    /// Kararı destekleyen okunabilir açıklamadır.
    /// </summary>
    public required string Gerekce { get; init; }
}
