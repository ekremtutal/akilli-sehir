using AkilliSehir.Core.Models;

namespace AkilliSehir.Business.Abstract;

/// <summary>
/// Arızaları uygun belediye birimine yönlendirmek için uygulama katmanı sözleşmesidir.
/// </summary>
/// <remarks>
/// Bu soyutlama, yerel kural motorunun ileride dışarıdaki bir yapay zekâ veya görsel analiz
/// sağlayıcısıyla değiştirilmesine olanak verir.
/// </remarks>
public interface IArizaYonlendirmeService
{
    /// <summary>
    /// Arıza verisini değerlendirir ve önerilen belediye birimini döndürür.
    /// </summary>
    Task<ArizaYonlendirmeSonucu> YonlendirAsync(
        ArizaYonlendirmeIstek istek,
        CancellationToken cancellationToken = default);
}
