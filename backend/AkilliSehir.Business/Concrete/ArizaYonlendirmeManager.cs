using System.Globalization;
using System.Text;
using AkilliSehir.Business.Abstract;
using AkilliSehir.Core.Entities.Enums;
using AkilliSehir.Core.Models;

namespace AkilliSehir.Business.Concrete;

/// <summary>
/// Türkçe anahtar kelimeler ile açıklanabilir, yerel bir arıza yönlendirme önerisi üretir.
/// </summary>
/// <remarks>
/// Bu sınıf dış servise veya API anahtarına ihtiyaç duymaz. IArizaYonlendirmeService
/// üzerinden gerçek bir yapay zekâ/görsel analiz sağlayıcısı ile değiştirilebilir.
/// </remarks>
public sealed class ArizaYonlendirmeManager : IArizaYonlendirmeService
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    // Başlıktaki eşleşmeler açıklamaya göre daha güçlü sinyal kabul edilir.
    private const decimal BaslikAnahtarKelimePuani = 2m;
    private const decimal AciklamaAnahtarKelimePuani = 1m;
    private const decimal ArizaTuruPuani = 3m;
    private const decimal VatandasBirimiPuani = 1.5m;

    private static readonly IReadOnlyDictionary<BelediyeBirimi, string[]> BirimAnahtarKelimeleri =
        new Dictionary<BelediyeBirimi, string[]>
        {
            [BelediyeBirimi.YolVeAltyapi] =
            [
                "yol", "asfalt", "çukur", "kaldırım", "kasis", "rögar", "mazgal",
                "çökme", "yol çökmesi", "bordür", "kaldırım taşı", "menfez"
            ],
            [BelediyeBirimi.SuVeKanalizasyon] =
            [
                "su", "su borusu", "boru patlağı", "su kaçağı", "kanalizasyon", "kanal",
                "logar", "rögar taşıyor", "atıksu", "musluk", "tazyik"
            ],
            [BelediyeBirimi.ElektrikVeAydinlatma] =
            [
                "lamba", "sokak lambası", "aydınlatma", "direk", "elektrik", "ampul",
                "karanlık", "yanmıyor", "yanmiyor", "aydınlatma direği"
            ],
            [BelediyeBirimi.ParkVeBahceler] =
            [
                "park", "bahçe", "ağaç", "dal", "yeşil alan", "çim", "oyun grubu",
                "sulama", "budama", "çocuk parkı"
            ],
            [BelediyeBirimi.CevreKorumaVeTemizlik] =
            [
                "çöp", "çöp konteyneri", "atık", "temizlik", "moloz", "koku", "süpürme",
                "çevre kirliliği", "çöp yığını", "geri dönüşüm"
            ],
            [BelediyeBirimi.UlasimHizmetleri] =
            [
                "trafik", "sinyalizasyon", "kavşak", "otobüs", "durak", "park yasağı",
                "yaya geçidi", "yol çizgisi", "ulaşım", "minibüs"
            ],
            [BelediyeBirimi.Zabita] =
            [
                "zabıta", "işgal", "seyyar satıcı", "gürültü", "ruhsat", "kaldırım işgali",
                "fahiş fiyat", "izinsiz", "dilenci"
            ]
        };

    private static readonly IReadOnlyDictionary<ArizaTuru, BelediyeBirimi> ArizaTuruBirimleri =
        new Dictionary<ArizaTuru, BelediyeBirimi>
        {
            [ArizaTuru.YolVeKaldirim] = BelediyeBirimi.YolVeAltyapi,
            [ArizaTuru.SuVeKanalizasyon] = BelediyeBirimi.SuVeKanalizasyon,
            [ArizaTuru.Aydinlatma] = BelediyeBirimi.ElektrikVeAydinlatma,
            [ArizaTuru.ParkVeYesilAlan] = BelediyeBirimi.ParkVeBahceler,
            [ArizaTuru.TemizlikVeAtik] = BelediyeBirimi.CevreKorumaVeTemizlik,
            [ArizaTuru.TrafikVeUlasim] = BelediyeBirimi.UlasimHizmetleri,
            [ArizaTuru.Zabita] = BelediyeBirimi.Zabita,
            [ArizaTuru.Diger] = BelediyeBirimi.Diger
        };

    public Task<ArizaYonlendirmeSonucu> YonlendirAsync(
        ArizaYonlendirmeIstek istek,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(istek);
        cancellationToken.ThrowIfCancellationRequested();

        var puanlar = BirimAnahtarKelimeleri.Keys.ToDictionary(birim => birim, _ => 0m);
        var baslik = MetniNormallestir(istek.Baslik);
        var aciklama = MetniNormallestir(istek.Aciklama);
        var gerekceler = new List<string>();

        foreach (var (birim, anahtarKelimeler) in BirimAnahtarKelimeleri)
        {
            var baslikEslesmeleri = EslesenAnahtarKelimeler(baslik, anahtarKelimeler);
            var aciklamaEslesmeleri = EslesenAnahtarKelimeler(aciklama, anahtarKelimeler);

            puanlar[birim] += baslikEslesmeleri.Count * BaslikAnahtarKelimePuani;
            puanlar[birim] += aciklamaEslesmeleri.Count * AciklamaAnahtarKelimePuani;

            if (baslikEslesmeleri.Count + aciklamaEslesmeleri.Count > 0)
            {
                var eslesmeler = baslikEslesmeleri
                    .Concat(aciklamaEslesmeleri)
                    .Distinct(StringComparer.Ordinal)
                    .Take(3);

                gerekceler.Add(
                    $"Başlık/açıklamadaki '{string.Join(", ", eslesmeler)}' ifadeleri " +
                    $"{BirimAdi(birim)} birimiyle eşleşti.");
            }
        }

        var arizaTurundenGelenBirim = BirimBul(istek.SecilenArizaTuru);
        if (arizaTurundenGelenBirim is not null)
        {
            PuanEkle(puanlar, arizaTurundenGelenBirim.Value, ArizaTuruPuani);
            gerekceler.Add(
                $"Seçilen '{ArizaTuruAdi(istek.SecilenArizaTuru!.Value)}' türü, " +
                $"{BirimAdi(arizaTurundenGelenBirim.Value)} birimini işaret ediyor.");
        }

        if (GecerliBirim(istek.VatandasSecilenBirim))
        {
            PuanEkle(puanlar, istek.VatandasSecilenBirim!.Value, VatandasBirimiPuani);
            gerekceler.Add(
                $"Vatandaşın '{BirimAdi(istek.VatandasSecilenBirim.Value)}' birimi tercihi " +
                "destekleyici sinyal olarak dikkate alındı.");
        }

        var siraliPuanlar = puanlar
            .Where(kayit => kayit.Value > 0)
            .OrderByDescending(kayit => kayit.Value)
            .ThenBy(kayit => kayit.Key)
            .ToList();

        var onerilenBirim = siraliPuanlar.Count > 0
            ? siraliPuanlar[0].Key
            : VarsayilanBirim(istek.VatandasSecilenBirim, arizaTurundenGelenBirim);

        if (siraliPuanlar.Count == 0)
        {
            gerekceler.Add(onerilenBirim == BelediyeBirimi.Diger
                ? "Yeterli sınıflandırma sinyali bulunamadığı için kayıt inceleme kuyruğuna yönlendirildi."
                : $"Metinsel eşleşme bulunamadığı için mevcut seçim {BirimAdi(onerilenBirim)} birimine yönlendirme için kullanıldı.");
        }

        if (!string.IsNullOrWhiteSpace(istek.FotografUrl))
        {
            // Yerel kural motoru URL'yi saklar ancak görsel içeriğini analiz etmez.
            gerekceler.Add("Fotoğraf bağlantısı alındı; mevcut yerel kural motoru görsel içeriğini analiz etmez.");
        }

        var guvenSkoru = GuvenSkorunuHesapla(siraliPuanlar, onerilenBirim);
        var sonuc = new ArizaYonlendirmeSonucu
        {
            OnerilenBirim = onerilenBirim,
            GuvenSkoru = guvenSkoru,
            Gerekce = string.Join(" ", gerekceler)
        };

        return Task.FromResult(sonuc);
    }

    private static List<string> EslesenAnahtarKelimeler(string metin, IEnumerable<string> anahtarKelimeler)
    {
        return anahtarKelimeler
            .Where(anahtarKelime => metin.Contains(MetniNormallestir(anahtarKelime), StringComparison.Ordinal))
            .ToList();
    }

    private static BelediyeBirimi? BirimBul(ArizaTuru? arizaTuru)
    {
        return arizaTuru is not null && ArizaTuruBirimleri.TryGetValue(arizaTuru.Value, out var birim)
            ? birim
            : null;
    }

    private static void PuanEkle(
        IDictionary<BelediyeBirimi, decimal> puanlar,
        BelediyeBirimi birim,
        decimal puan)
    {
        if (!GecerliBirim(birim))
        {
            return;
        }

        if (!puanlar.TryAdd(birim, puan))
        {
            puanlar[birim] += puan;
        }
    }

    private static bool GecerliBirim(BelediyeBirimi? birim)
    {
        return birim is not null && birim is not BelediyeBirimi.Bilinmiyor and not BelediyeBirimi.Diger;
    }

    private static BelediyeBirimi VarsayilanBirim(
        BelediyeBirimi? vatandasSecilenBirim,
        BelediyeBirimi? arizaTurundenGelenBirim)
    {
        if (GecerliBirim(vatandasSecilenBirim))
        {
            return vatandasSecilenBirim!.Value;
        }

        if (GecerliBirim(arizaTurundenGelenBirim))
        {
            return arizaTurundenGelenBirim!.Value;
        }

        return BelediyeBirimi.Diger;
    }

    private static decimal GuvenSkorunuHesapla(
        IReadOnlyList<KeyValuePair<BelediyeBirimi, decimal>> siraliPuanlar,
        BelediyeBirimi onerilenBirim)
    {
        if (siraliPuanlar.Count == 0)
        {
            return onerilenBirim == BelediyeBirimi.Diger ? 0.30m : 0.55m;
        }

        var enYuksekPuan = siraliPuanlar[0].Value;
        var ikinciPuan = siraliPuanlar.Count > 1 ? siraliPuanlar[1].Value : 0m;

        // Puan arttıkça güven artar; yakın ikinci aday varsa belirsizlik nedeniyle düşer.
        var guven = 0.50m + Math.Min(enYuksekPuan * 0.05m, 0.35m);
        if (ikinciPuan > 0 && ikinciPuan >= enYuksekPuan * 0.80m)
        {
            guven -= 0.12m;
        }

        return decimal.Clamp(guven, 0.35m, 0.95m);
    }

    private static string MetniNormallestir(string? metin)
    {
        if (string.IsNullOrWhiteSpace(metin))
        {
            return string.Empty;
        }

        var kucukHarfliMetin = metin.ToLower(TurkishCulture)
            .Replace('ç', 'c')
            .Replace('ğ', 'g')
            .Replace('ı', 'i')
            .Replace('ö', 'o')
            .Replace('ş', 's')
            .Replace('ü', 'u');

        var metinKurucu = new StringBuilder(kucukHarfliMetin.Length);
        foreach (var karakter in kucukHarfliMetin)
        {
            metinKurucu.Append(char.IsLetterOrDigit(karakter) ? karakter : ' ');
        }

        return string.Join(' ', metinKurucu
            .ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string BirimAdi(BelediyeBirimi birim)
    {
        return birim switch
        {
            BelediyeBirimi.YolVeAltyapi => "Yol ve Altyapı",
            BelediyeBirimi.SuVeKanalizasyon => "Su ve Kanalizasyon",
            BelediyeBirimi.ElektrikVeAydinlatma => "Elektrik ve Aydınlatma",
            BelediyeBirimi.ParkVeBahceler => "Park ve Bahçeler",
            BelediyeBirimi.CevreKorumaVeTemizlik => "Çevre Koruma ve Temizlik",
            BelediyeBirimi.UlasimHizmetleri => "Ulaşım Hizmetleri",
            BelediyeBirimi.Zabita => "Zabıta",
            _ => "Diğer"
        };
    }

    private static string ArizaTuruAdi(ArizaTuru arizaTuru)
    {
        return arizaTuru switch
        {
            ArizaTuru.YolVeKaldirim => "Yol ve Kaldırım",
            ArizaTuru.SuVeKanalizasyon => "Su ve Kanalizasyon",
            ArizaTuru.Aydinlatma => "Aydınlatma",
            ArizaTuru.ParkVeYesilAlan => "Park ve Yeşil Alan",
            ArizaTuru.TemizlikVeAtik => "Temizlik ve Atık",
            ArizaTuru.TrafikVeUlasim => "Trafik ve Ulaşım",
            ArizaTuru.Zabita => "Zabıta",
            _ => "Diğer"
        };
    }
}
