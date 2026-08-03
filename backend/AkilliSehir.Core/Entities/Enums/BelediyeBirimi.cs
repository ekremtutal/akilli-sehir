namespace AkilliSehir.Core.Entities.Enums;

/// <summary>
/// Arıza kayıtlarının yönlendirilebileceği belediye birimlerini tanımlar.
/// </summary>
public enum BelediyeBirimi
{
    Bilinmiyor = 0,
    YolVeAltyapi = 1,
    SuVeKanalizasyon = 2,
    ElektrikVeAydinlatma = 3,
    ParkVeBahceler = 4,
    CevreKorumaVeTemizlik = 5,
    UlasimHizmetleri = 6,
    Zabita = 7,
    Diger = 8
}
