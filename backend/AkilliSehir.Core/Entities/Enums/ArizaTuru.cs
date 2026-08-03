namespace AkilliSehir.Core.Entities.Enums;

/// <summary>
/// Vatandaşın bildirdiği arızanın genel türünü tanımlar.
/// </summary>
public enum ArizaTuru
{
    Bilinmiyor = 0,
    YolVeKaldirim = 1,
    SuVeKanalizasyon = 2,
    Aydinlatma = 3,
    ParkVeYesilAlan = 4,
    TemizlikVeAtik = 5,
    TrafikVeUlasim = 6,
    Zabita = 7,
    Diger = 8
}
