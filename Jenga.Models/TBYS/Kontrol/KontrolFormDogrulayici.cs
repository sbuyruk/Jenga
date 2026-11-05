using System.Globalization;

namespace Jenga.Models.TBYS.Kontrol
{
    /// <summary>Form tanımı ve kontrol cevapları için doğrulama kuralları (saf mantık, DB bağımsız).</summary>
    public static class KontrolFormDogrulayici
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>Tanımın yapısal doğruluğu: boş başlık, çakışan kod, geçersiz seçenek/veri kaynağı.</summary>
        public static List<string> TanimiDogrula(KontrolFormTanimi tanim)
        {
            var hatalar = new List<string>();
            var kodlar = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (tanim.Bolumler.Count == 0)
                hatalar.Add("Form en az bir bölüm içermelidir.");

            foreach (var b in tanim.Bolumler)
            {
                if (string.IsNullOrWhiteSpace(b.Kod)) hatalar.Add("Bölüm kodu boş olamaz.");
                else if (!kodlar.Add("B:" + b.Kod)) hatalar.Add($"Bölüm kodu tekrar ediyor: {b.Kod}");
                if (string.IsNullOrWhiteSpace(b.Baslik)) hatalar.Add($"Bölüm başlığı boş olamaz ({b.Kod}).");

                foreach (var a in b.Alanlar) AlaniDogrula(a, b.Baslik, kodlar, hatalar);

                foreach (var g in b.Gruplar)
                {
                    if (string.IsNullOrWhiteSpace(g.Kod)) hatalar.Add($"Grup kodu boş olamaz ({b.Baslik}).");
                    else if (!kodlar.Add("G:" + g.Kod)) hatalar.Add($"Grup kodu tekrar ediyor: {g.Kod}");
                    if (string.IsNullOrWhiteSpace(g.Baslik)) hatalar.Add($"Grup başlığı boş olamaz ({g.Kod}).");
                    if (g.Alanlar.Count == 0) hatalar.Add($"Grup en az bir alan içermelidir ({g.Baslik}).");
                    if (g.EnAzSatir is < 0) hatalar.Add($"En az satır sayısı negatif olamaz ({g.Baslik}).");
                    foreach (var a in g.Alanlar) AlaniDogrula(a, g.Baslik, kodlar, hatalar);
                }
            }

            return hatalar;
        }

        private static void AlaniDogrula(KontrolAlanTanimi a, string ust, HashSet<string> kodlar, List<string> hatalar)
        {
            if (string.IsNullOrWhiteSpace(a.Kod)) hatalar.Add($"Alan kodu boş olamaz ({ust}).");
            else if (!kodlar.Add("A:" + a.Kod)) hatalar.Add($"Alan kodu tekrar ediyor: {a.Kod}");
            if (string.IsNullOrWhiteSpace(a.Baslik)) hatalar.Add($"Alan başlığı boş olamaz ({a.Kod}).");

            if (a.SecenekliMi)
            {
                if (a.Secenekler.Count == 0) hatalar.Add($"Seçimli alanın seçeneği yok: {a.Baslik}");
                var sKod = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var s in a.Secenekler)
                {
                    if (string.IsNullOrWhiteSpace(s.Kod) || string.IsNullOrWhiteSpace(s.Etiket))
                        hatalar.Add($"Boş seçenek kodu/etiketi: {a.Baslik}");
                    else if (!sKod.Add(s.Kod))
                        hatalar.Add($"Seçenek kodu tekrar ediyor: {a.Baslik}/{s.Kod}");
                }
            }
            else if (a.Secenekler.Count > 0)
            {
                hatalar.Add($"Seçimli olmayan alanda seçenek tanımlanamaz: {a.Baslik}");
            }

            if (a.VeriKaynagi != KontrolVeriKaynagiTipi.Manuel)
            {
                if (!TasinmazVeriKaynagi.Gecerli(a.TasinmazAlani))
                    hatalar.Add($"Geçersiz taşınmaz veri kaynağı: {a.Baslik} → '{a.TasinmazAlani}'");
                if (a.Tip == KontrolAlanTipi.Ek)
                    hatalar.Add($"Ek alanı taşınmazdan beslenemez: {a.Baslik}");
            }
        }

        /// <summary>
        /// Cevapların tip ve zorunluluk doğrulaması. <paramref name="ekSayisi"/>: (alanKodu, satırNo) → yüklü ek sayısı.
        /// Tamamlama sırasında çağrılır; taslak kaydında yalnızca tip hataları (<paramref name="zorunluKontrol"/>=false) denetlenir.
        /// </summary>
        public static List<string> CevaplariDogrula(
            KontrolFormTanimi tanim,
            KontrolCevaplari cevaplar,
            Func<string, int?, int> ekSayisi,
            bool zorunluKontrol)
        {
            var hatalar = new List<string>();

            foreach (var b in tanim.Bolumler)
            {
                foreach (var a in b.Alanlar)
                    CevapDogrula(a, cevaplar.Al(a.Kod), ekSayisi(a.Kod, null), zorunluKontrol, hatalar, null);

                foreach (var g in b.Gruplar)
                {
                    var satirlar = cevaplar.Gruplar.TryGetValue(g.Kod, out var s) ? s : new List<Dictionary<string, string?>>();
                    var enAz = g.Tekrarlanabilir ? (g.EnAzSatir ?? 0) : 1;
                    if (zorunluKontrol && satirlar.Count < enAz && g.Alanlar.Any(x => x.Zorunlu))
                        hatalar.Add($"'{g.Baslik}' için en az {enAz} satır girilmelidir.");

                    for (int i = 0; i < satirlar.Count; i++)
                    {
                        var satir = satirlar[i];
                        foreach (var a in g.Alanlar)
                        {
                            satir.TryGetValue(a.Kod, out var v);
                            CevapDogrula(a, v, ekSayisi(a.Kod, i), zorunluKontrol, hatalar, $"{g.Baslik} #{i + 1}");
                        }
                    }
                }
            }

            return hatalar;
        }

        private static void CevapDogrula(KontrolAlanTanimi a, string? deger, int ekSayisi, bool zorunluKontrol, List<string> hatalar, string? baglam)
        {
            var ad = baglam is null ? a.Baslik : $"{baglam} / {a.Baslik}";

            if (a.Tip == KontrolAlanTipi.Ek)
            {
                if (zorunluKontrol && a.Zorunlu && ekSayisi == 0) hatalar.Add($"'{ad}' için en az bir ek yüklenmelidir.");
                return;
            }

            if (string.IsNullOrWhiteSpace(deger))
            {
                if (zorunluKontrol && a.Zorunlu) hatalar.Add($"'{ad}' zorunludur.");
                return;
            }

            switch (a.Tip)
            {
                case KontrolAlanTipi.TamSayi:
                    if (!long.TryParse(deger, NumberStyles.Integer, Inv, out _)) hatalar.Add($"'{ad}' tam sayı olmalıdır.");
                    break;
                case KontrolAlanTipi.OndalikSayi:
                    if (!decimal.TryParse(deger, NumberStyles.Number, Inv, out _)) hatalar.Add($"'{ad}' sayısal olmalıdır.");
                    break;
                case KontrolAlanTipi.Tarih:
                    if (!DateTime.TryParseExact(deger, "yyyy-MM-dd", Inv, DateTimeStyles.None, out _)) hatalar.Add($"'{ad}' geçerli bir tarih olmalıdır.");
                    break;
                case KontrolAlanTipi.EvetHayir:
                    if (!Enum.TryParse<EvetHayirCevabi>(deger, true, out _)) hatalar.Add($"'{ad}' Evet/Hayır/Uygulanamaz olmalıdır.");
                    break;
                case KontrolAlanTipi.TekliSecim:
                    if (!a.Secenekler.Any(s => s.Kod.Equals(deger, StringComparison.OrdinalIgnoreCase))) hatalar.Add($"'{ad}' için geçersiz seçenek.");
                    break;
                case KontrolAlanTipi.CokluSecim:
                    foreach (var p in deger.Split(KontrolCevaplari.CokluSecimAyirici, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        if (!a.Secenekler.Any(s => s.Kod.Equals(p, StringComparison.OrdinalIgnoreCase))) hatalar.Add($"'{ad}' için geçersiz seçenek: {p}");
                    break;
            }
        }
    }
}
