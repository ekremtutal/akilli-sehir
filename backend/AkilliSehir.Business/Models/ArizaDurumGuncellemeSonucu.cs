namespace AkilliSehir.Business.Models;

/// <summary>
/// Saha personelinin arıza durumu güncelleme girişiminin sonucunu belirtir.
/// Controller katmanının bulunamayan kayıt ile yetkisiz erişimi ayırt etmesini sağlar.
/// </summary>
public enum ArizaDurumGuncellemeSonucu
{
    Basarili,
    Bulunamadi,
    Yetkisiz
}
