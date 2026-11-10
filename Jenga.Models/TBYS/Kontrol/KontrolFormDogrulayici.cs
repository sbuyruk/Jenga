using System.Globalization;

namespace Jenga.Models.TBYS.Kontrol
{
    /// <summary>Kontrol cevaplarının tip ve zorunluluk kuralları (DB bağımsız).</summary>
    public static class KontrolFormDogrulayici
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

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
