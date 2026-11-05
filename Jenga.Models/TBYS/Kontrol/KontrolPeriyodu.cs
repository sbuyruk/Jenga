namespace Jenga.Models.TBYS.Kontrol
{
    /// <summary>Kontrol periyodu hesapları. Ay/yıl eklemede takvim kuralları (ay sonu, artık yıl) .NET tarafından uygulanır.</summary>
    public static class KontrolPeriyodu
    {
        public static bool Gecerli(int? sure, PeriyotBirimi? birim) => sure is > 0 && birim.HasValue;

        public static DateTime Ekle(DateTime baslangic, int sure, PeriyotBirimi birim)
        {
            if (sure <= 0) throw new ArgumentOutOfRangeException(nameof(sure), "Periyot süresi pozitif olmalıdır.");
            var t = baslangic.Date;
            return birim switch
            {
                PeriyotBirimi.Gun => t.AddDays(sure),
                PeriyotBirimi.Hafta => t.AddDays(7L * sure),
                PeriyotBirimi.Ay => t.AddMonths(sure),
                PeriyotBirimi.Yil => t.AddYears(sure),
                _ => throw new ArgumentOutOfRangeException(nameof(birim))
            };
        }

        public static string Etiket(int? sure, PeriyotBirimi? birim)
        {
            if (!Gecerli(sure, birim)) return "Tanımsız";
            var b = birim!.Value switch
            {
                PeriyotBirimi.Gun => "gün",
                PeriyotBirimi.Hafta => "hafta",
                PeriyotBirimi.Ay => "ay",
                PeriyotBirimi.Yil => "yıl",
                _ => "?"
            };
            return $"{sure} {b}";
        }

        public static bool Ayni(int? s1, PeriyotBirimi? b1, int? s2, PeriyotBirimi? b2)
            => s1 == s2 && b1 == b2;
    }

    /// <summary>
    /// Form alanlarının taşınmazdan beslenebileceği izin verilen alanlar.
    /// Serbest ifade/SQL yoktur; yalnızca bu listedeki anahtarlar eşlenebilir.
    /// </summary>
    public static class TasinmazVeriKaynagi
    {
        public sealed record Alan(string Anahtar, string Etiket, Func<Tasinmaz, string?> Oku);

        private static string? D(decimal? v) => v?.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
        private static string? T(DateTime? v) => v?.ToString("yyyy-MM-dd");
        private static string? B(bool? v) => v.HasValue ? (v.Value ? "Evet" : "Hayır") : null;

        public static readonly IReadOnlyList<Alan> Alanlar = new List<Alan>
        {
            new("EmlakSicilNo", "Emlak Sicil No", t => t.EmlakSicilNo),
            new("Cinsi", "Cinsi", t => t.Cinsi),
            new("Ili", "İli", t => t.Ili),
            new("Ilcesi", "İlçesi", t => t.Ilcesi),
            new("Mahalle", "Mahalle", t => t.Mahalle),
            new("Koy", "Köy", t => t.Koy),
            new("Mevki", "Mevki", t => t.Mevki),
            new("Cadde", "Cadde", t => t.Cadde),
            new("Sokak", "Sokak", t => t.Sokak),
            new("Adres", "Adres", t => t.Adres),
            new("PaftaNo", "Pafta No", t => t.PaftaNo),
            new("AdaNo", "Ada No", t => t.AdaNo),
            new("ParselNo", "Parsel No", t => t.ParselNo),
            new("Yuzolcumu", "Yüzölçümü (Parsel Alanı)", t => t.Yuzolcumu),
            new("Metrekare", "Metrekare", t => D(t.Metrekare)),
            new("ToplamMetrekare", "Toplam Metrekare", t => D(t.ToplamMetrekare)),
            new("BBBrutAlan", "Bağımsız Bölüm Brüt Alan", t => D(t.BBBrutAlan)),
            new("BBNetAlan", "Bağımsız Bölüm Net Alan", t => D(t.BBNetAlan)),
            new("Blok", "Blok", t => t.Blok),
            new("Giris", "Giriş", t => t.Giris),
            new("BagimsizBolumNo", "Bağımsız Bölüm No", t => t.BagimsizBolumNo),
            new("BulunduguKat", "Bulunduğu Kat", t => t.BulunduguKat),
            new("ToplamKatSayisi", "Toplam Kat Sayısı", t => t.ToplamKatSayisi),
            new("ArsaPayi", "Arsa Payı", t => t.ArsaPayi),
            new("AnaTasinmazNitelik", "Ana Taşınmaz Niteliği", t => t.AnaTasinmazNitelik),
            new("BBNitelik", "Bağımsız Bölüm Niteliği", t => t.BBNitelik),
            new("Nitelik", "Nitelik", t => t.Nitelik),
            new("KullanimSekli", "Kullanım Şekli", t => t.KullanimSekli),
            new("KatMulkiyeti", "Kat Mülkiyeti", t => B(t.KatMulkiyeti)),
            new("KatIrtifaki", "Kat İrtifakı", t => B(t.KatIrtifaki)),
            new("TapuTarihi", "Tapu Tarihi", t => T(t.TapuTarihi)),
            new("YevmiyeNo", "Yevmiye No", t => t.YevmiyeNo),
            new("CiltNo", "Cilt No", t => t.CiltNo),
            new("SahifeNo", "Sahife No", t => t.SahifeNo),
            new("InsaYili", "İnşa Yılı", t => t.InsaYili),
            new("MulkiyetSekli", "Mülkiyet Şekli", t => t.MulkiyetSekli),
            new("KiraDurumu", "Kira Durumu", t => t.KiraDurumu),
            new("Bagisci", "Bağışçı", t => t.Bagisci),
            new("SorumluBolge", "Sorumlu Bölge", t => t.SorumluBolge),
        };

        private static readonly Dictionary<string, Alan> _index =
            Alanlar.ToDictionary(a => a.Anahtar, StringComparer.OrdinalIgnoreCase);

        public static bool Gecerli(string? anahtar) => !string.IsNullOrWhiteSpace(anahtar) && _index.ContainsKey(anahtar);

        public static string? Oku(Tasinmaz tasinmaz, string? anahtar)
            => anahtar is not null && _index.TryGetValue(anahtar, out var a) ? a.Oku(tasinmaz) : null;

        /// <summary>Formda kullanılan tüm taşınmaz alanlarının anlık kopyasını üretir (anahtar → değer).</summary>
        public static Dictionary<string, string?> Snapshot(Tasinmaz tasinmaz, KontrolFormTanimi tanim)
        {
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var alan in tanim.TumAlanlar())
            {
                if (alan.VeriKaynagi == KontrolVeriKaynagiTipi.Manuel || !Gecerli(alan.TasinmazAlani)) continue;
                dict[alan.TasinmazAlani!] = Oku(tasinmaz, alan.TasinmazAlani);
            }
            return dict;
        }
    }
}
