using AkilliSehir.Core.Entities;

namespace AkilliSehir.Business.Models;

/// <summary>
/// Randevu saatinin uygunluk kontrolü sonucunu ve oluşan kaydı taşır.
/// </summary>
public sealed record RandevuOlusturmaSonucu(Randevu? Randevu, string? HataMesaji)
{
    public bool Basarili => Randevu is not null;
}

public enum RandevuIptalSonucu
{
    Basarili,
    Bulunamadi,
    Yetkisiz,
    IptalEdilemez
}
