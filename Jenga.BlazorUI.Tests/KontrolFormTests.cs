using Jenga.BlazorUI.Components.Pages.TBYS.Kontrol;
using Jenga.Models.TBYS.Kontrol;
using Jenga.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.JSInterop;

namespace Jenga.BlazorUI.Tests;

public class KontrolFormTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task KiraAlanlari_YeniVeEskiRaporlardaIlkAlanKiraciVeSaltOkunur(bool eskiRapor, bool kiraciEksik)
    {
        var tanim = KontrolRaporu.Olustur();
        if (eskiRapor)
        {
            var eskiAlanlar = tanim.Bolumler.Single(b => b.Kod == "irtibat").Alanlar;
            eskiAlanlar.RemoveAll(a => a.Kod == "kira_sozlesme_baslangic" || (kiraciEksik && a.Kod == "kira_kiraci_adi"));
            if (!kiraciEksik)
            {
                tanim.AlanBul("kira_bedel")!.Sira = 1;
                tanim.AlanBul("kira_kiraci_adi")!.Sira = 2;
            }
            tanim.AlanBul("kira_baslangic")!.Baslik = "Kira Başlangıç Tarihi";
            tanim.AlanBul("kira_bitis")!.Baslik = "Kira Bitiş Tarihi";
            tanim.AlanBul("kira_odeme_duzeni")!.Baslik = "Ödeme Düzeni";
            var snapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = tanim.ToJson() });
            tanim = KontrolRaporu.TanimAl(snapshot);
        }
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy("kira_kiraci_adi", "Ayşe Yılmaz");
        cevaplar.Koy("kira_baslangic", "2020-01-01");
        cevaplar.Koy("kira_bitis", "2027-01-01");
        cevaplar.Koy("kira_odeme_duzeni", "Aylık");
        cevaplar.GrupSatirlari("irtibat_kisiler").Add(new() { ["irtibat_ad"] = "Ali Demir", ["irtibat_tel"] = "03121234567" });
        var bolum = Assert.Single(tanim.Bolumler, b => b.Kod == "irtibat");
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [bolum]));
        foreach (var baslik in new[] { "Kiracı Adı", "İlk Sözleşme Tarihi", "Sözleşme Başlangıcı", "Sözleşme Bitişi", "Ödeme Şekli" }) Assert.Contains(baslik, html);
        Assert.Equal("kira_kiraci_adi", bolum.Alanlar.OrderBy(a => a.Sira).First().Kod);
        Assert.True(html.IndexOf("Kiracı Adı", StringComparison.Ordinal) < html.IndexOf("Aylık Kira Bedeli", StringComparison.Ordinal));
        foreach (var alan in bolum.Alanlar)
            Assert.Matches($"<input[^>]*id=\"f_{alan.Kod}_x\"[^>]*disabled", html);
        foreach (var alan in Assert.Single(bolum.Gruplar).Alanlar)
            Assert.Matches($"<input[^>]*id=\"f_{alan.Kod}_0\"[^>]*disabled", html);
        Assert.DoesNotContain("Satır Ekle", html);
        Assert.DoesNotContain("Satırı sil", html);
        Assert.Contains("Ayşe Yılmaz", html);
        Assert.Contains("Ali Demir", html);
        foreach (var eski in new[] { "Kira Başlangıç Tarihi", "Kira Bitiş Tarihi", "Ödeme Düzeni" }) Assert.DoesNotContain(eski, html);
        Assert.Contains("2020-01-01", html);
        Assert.Contains("2027-01-01", html);
        Assert.Contains("Aylık", html);
        Assert.Equal(KontrolAlanTipi.Tarih, tanim.AlanBul("kira_sozlesme_baslangic")!.Tip);
        Assert.Contains(KontrolFormAdimlari.AlanAra(KontrolFormAdimlari.Olustur(tanim), "Kiracı Adı"), a => a.AlanKodu == "kira_kiraci_adi");
        var tekrar = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = tanim.ToJson() });
        Assert.Single(KontrolRaporu.TanimAl(tekrar).TumAlanlar(), a => a.Kod == "kira_sozlesme_baslangic");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DepremBolgesi_YeniVeEskiRaporlardaSecilebilir(bool eskiRapor)
    {
        var tanim = KontrolRaporu.Olustur();
        if (eskiRapor)
        {
            tanim.Bolumler.Single(b => b.Kod == "konum").Alanlar.RemoveAll(a => a.Kod == "konum_deprem_bolgesi");
            var snapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = tanim.ToJson() });
            tanim = KontrolRaporu.TanimAl(snapshot);
        }
        var bolum = tanim.Bolumler.Single(b => b.Kod == "konum");
        var alan = Assert.Single(bolum.Alanlar, a => a.Kod == "konum_deprem_bolgesi");
        Assert.Equal(KontrolAlanTipi.TekliSecim, alan.Tip);
        Assert.Equal(new[] { "1", "2", "3", "4" }, alan.Secenekler.Select(s => s.Kod));
        Assert.Equal(tanim.AlanBul("konum_koordinat")!.Sira + 1, alan.Sira);
        var cevaplar = new KontrolCevaplari();
        var sadeceKonum = new KontrolFormTanimi { Bolumler = [bolum] };
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(sadeceKonum, cevaplar, (_, _) => 0, true));
        foreach (var secim in new[] { "1", "2", "3", "4" })
        {
            cevaplar.Koy(alan.Kod, secim);
            Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(sadeceKonum, cevaplar, (_, _) => 0, true));
            Assert.Equal(secim, KontrolCevaplari.FromJson(cevaplar.ToJson()).Al(alan.Kod));
        }
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [bolum]));
        Assert.Contains("Deprem Bölgesi", html);
        Assert.Matches("<select[^>]*id=\"f_konum_deprem_bolgesi_x\"", html);
        Assert.Contains(KontrolFormAdimlari.AlanAra(KontrolFormAdimlari.Olustur(tanim), "Deprem Bölgesi"), a => a.AlanKodu == alan.Kod);
        cevaplar.Koy(alan.Kod, "5");
        Assert.NotEmpty(KontrolFormDogrulayici.CevaplariDogrula(sadeceKonum, cevaplar, (_, _) => 0, true));
        var tekrarSnapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = tanim.ToJson() });
        Assert.Single(KontrolRaporu.TanimAl(tekrarSnapshot).TumAlanlar(), a => a.Kod == alan.Kod);
    }

    [Theory]
    [InlineData("39.925533, 32.866287", "39.925533%2C32.866287")]
    [InlineData("-33.8688; 151.2093", "-33.8688%2C151.2093")]
    [InlineData(" 0 0 ", "0%2C0")]
    [InlineData("90, -180", "90%2C-180")]
    [InlineData("91, 32", null)]
    [InlineData("39, 181", null)]
    [InlineData("", null)]
    [InlineData("Ankara", null)]
    [InlineData("javascript:alert(1)", null)]
    public async Task HaritadaGoster_GecerliKoordinatiGuvenliBaglantiylaAcar(string koordinat, string? beklenen)
    {
        var tanim = KontrolRaporu.Olustur();
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy("konum_koordinat", koordinat);
        var once = cevaplar.ToJson();
        foreach (var saltOkunur in new[] { false, true })
        {
            var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, tanim.Bolumler.Where(b => b.Kod == "konum").ToList(), saltOkunur));
            Assert.Contains("Haritada Göster", html);
            if (beklenen is not null)
            {
                Assert.Contains($"href=\"https://www.google.com/maps/search/?api=1&query={beklenen}\"", html);
                Assert.Contains("target=\"_blank\" rel=\"noopener noreferrer\"", html);
            }
            else Assert.DoesNotContain("https://www.google.com/maps/search/", html);
        }
        Assert.Equal(once, cevaplar.ToJson());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task KonumAl_BasariliKonumuYazar_HatadaMevcutDegeriKorur(bool hata)
    {
        var js = new KonumJS { Sonuc = hata
            ? new() { Hata = "Konum izni verilmedi." }
            : new() { Koordinat = "39.925533, 32.866287" } };
        var degisiklik = 0;
        var form = new KontrolFormRenderer
        {
            Tanim = KontrolRaporu.Olustur(), Cevaplar = new(),
            OnChanged = EventCallback.Factory.Create(new object(), () => degisiklik++)
        };
        form.Cevaplar.Koy("konum_koordinat", "Eski koordinat");
        typeof(KontrolFormRenderer).GetProperty("JS", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(form, js);
        async Task KonumAl() => await (Task)typeof(KontrolFormRenderer).GetMethod("KonumAlAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(form, null)!;
        await KonumAl();
        Assert.Equal(hata ? "Eski koordinat" : "39.925533, 32.866287", form.Cevaplar.Al("konum_koordinat"));
        Assert.Equal(hata ? 0 : 1, degisiklik);
        form.ReadOnly = true;
        await KonumAl();
        Assert.Equal(1, js.Cagri);
        var bolumler = form.Tanim.Bolumler.Where(b => b.Kod == "konum").ToList();
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(form.Tanim, form.Cevaplar, bolumler));
        Assert.Contains("Bulunduğum Konumu Al", html);
        Assert.DoesNotContain("Bulunduğum Konumu Al", await RenderAsync(form.Tanim, form.Cevaplar, bolumler, true));
    }

    private sealed class KonumJS : IJSRuntime
    {
        public KontrolFormRenderer.KonumSonucu Sonuc { get; set; } = new();
        public int Cagri { get; private set; }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Assert.Equal("kontrolKonum.al", identifier);
            Cagri++;
            return ValueTask.FromResult((TValue)(object)Sonuc);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TasinmazBelgeleri_AdlariVeDosyalariSatirBazindaKorunur(bool eskiRapor)
    {
        var tanim = KontrolRaporu.Olustur();
        if (eskiRapor)
        {
            tanim.Bolumler.RemoveAll(b => b.Kod == "tasinmaz_belgeleri");
            var snapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = tanim.ToJson() });
            tanim = KontrolRaporu.TanimAl(snapshot);
        }
        var bolum = Assert.Single(tanim.Bolumler, b => b.Kod == "tasinmaz_belgeleri");
        var grup = Assert.Single(bolum.Gruplar);
        Assert.True(grup.Tekrarlanabilir);
        var adimlar = KontrolFormAdimlari.Olustur(tanim);
        Assert.Contains(bolum, adimlar.Single(a => a.Baslik == "Belge, Fotoğraf ve Ekler").Bolumler);
        Assert.Contains(KontrolFormAdimlari.AlanAra(adimlar, "Belge Adı"), a => a.GrupKodu == grup.Kod);

        var cevaplar = new KontrolCevaplari();
        var sadeceBelgeler = new KontrolFormTanimi { Bolumler = [bolum] };
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(sadeceBelgeler, cevaplar, (_, _) => 0, true));
        cevaplar.GrupSatirlari(grup.Kod).Add(new() { ["tasinmaz_belge_ad"] = "Tapu Senedi" });
        cevaplar.GrupSatirlari(grup.Kod).Add(new() { ["tasinmaz_belge_ad"] = "Yapı Ruhsatı" });
        Assert.NotEmpty(KontrolFormDogrulayici.CevaplariDogrula(sadeceBelgeler, cevaplar, (_, _) => 0, true));
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(sadeceBelgeler, cevaplar, (_, _) => 1, true));
        cevaplar.Gruplar[grup.Kod][0]["tasinmaz_belge_ad"] = "";
        Assert.Contains(KontrolFormDogrulayici.CevaplariDogrula(sadeceBelgeler, cevaplar, (_, _) => 1, true), h => h.Contains("Belge Adı"));
        cevaplar.Gruplar[grup.Kod][0]["tasinmaz_belge_ad"] = "Tapu Senedi";
        cevaplar = KontrolCevaplari.FromJson(cevaplar.ToJson());
        TasinmazKontrolEk[] ekler =
        [
            new() { Id = 701, AlanKodu = "tasinmaz_belge_ek", GrupKodu = grup.Kod, SatirNo = 0, DosyaAdi = "tapu.pdf" },
            new() { Id = 702, AlanKodu = "tasinmaz_belge_ek", GrupKodu = grup.Kod, SatirNo = 1, DosyaAdi = "ruhsat.pdf" }
        ];
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [bolum], false, ekler));
        Assert.Contains("Tapu Senedi", html);
        Assert.Contains("Yapı Ruhsatı", html);
        Assert.Contains("/api/tbys/kontrol-ek/701", html);
        Assert.Contains("/api/tbys/kontrol-ek/702", html);
        Assert.True(html.IndexOf("Tapu Senedi", StringComparison.Ordinal) < html.IndexOf("tapu.pdf", StringComparison.Ordinal));
        Assert.True(html.IndexOf("tapu.pdf", StringComparison.Ordinal) < html.IndexOf("Yapı Ruhsatı", StringComparison.Ordinal));
        Assert.Contains("dosyalarını kaldırdıktan sonra", html);
        var readOnly = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [bolum], true, ekler));
        Assert.DoesNotContain("Satır Ekle", readOnly);
        Assert.DoesNotContain("bi-x-circle", readOnly);
        Assert.Contains("tapu.pdf", readOnly);
        var tekrar = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = tanim.ToJson() });
        Assert.Single(KontrolRaporu.TanimAl(tekrar).Bolumler, b => b.Kod == bolum.Kod);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Olculer_ToplamiSatirlarDegistikceGuncellenir(bool readOnly)
    {
        var tanim = KontrolRaporu.Olustur();
        var bolum = tanim.Bolumler.Single(b => b.Kod == "bb_ozellik");
        var cevaplar = new KontrolCevaplari();
        async Task ToplamiDogrula(string beklenen)
        {
            var once = cevaplar.ToJson();
            var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [bolum], readOnly));
            Assert.Contains("Toplam Alan (m²)", html);
            Assert.Matches($"<output[^>]*>{System.Text.RegularExpressions.Regex.Escape(beklenen)}</output>", html);
            Assert.Equal(once, cevaplar.ToJson());
        }

        await ToplamiDogrula("0,00");
        var satirlar = cevaplar.GrupSatirlari("bb_olculer");
        satirlar.Add(new() { ["bbo_olcu_ad"] = "Salon", ["bbo_olcu_alan"] = "45" });
        satirlar.Add(new() { ["bbo_olcu_ad"] = "Mutfak", ["bbo_olcu_alan"] = "12.5" });
        await ToplamiDogrula("57,50");
        satirlar[0]["bbo_olcu_alan"] = "46.25";
        await ToplamiDogrula("58,75");
        satirlar.RemoveAt(1);
        await ToplamiDogrula("46,25");
        satirlar.Add(new() { ["bbo_olcu_alan"] = "3,5" });
        satirlar.Add(new() { ["bbo_olcu_alan"] = "" });
        satirlar.Add(new() { ["bbo_olcu_alan"] = "abc" });
        await ToplamiDogrula("49,75");
        satirlar.Clear();
        await ToplamiDogrula("0,00");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Olculer_BagimsizBolumdeTekrarlanabilirSatirlarlaGosterilir(bool eskiRapor)
    {
        var tanim = KontrolRaporu.Olustur();
        if (eskiRapor)
        {
            tanim.Bolumler.Single(b => b.Kod == "bb_ozellik").Gruplar.RemoveAll(g => g.Kod == "bb_olculer");
            var snapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = tanim.ToJson() });
            tanim = KontrolRaporu.TanimAl(snapshot);
        }
        var bolum = tanim.Bolumler.Single(b => b.Kod == "bb_ozellik");
        var grup = Assert.Single(bolum.Gruplar, g => g.Kod == "bb_olculer");
        Assert.True(grup.Tekrarlanabilir);
        Assert.Equal("Ölçüler (Metrekare)", grup.Baslik);
        var cevaplar = new KontrolCevaplari();
        var bolumTanimi = new KontrolFormTanimi { Bolumler = [bolum] };
        // Ölçü girmek isteğe bağlıdır; eklenen satırın iki alanı da tamamlanmalıdır.
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(bolumTanimi, cevaplar, (_, _) => 0, true));
        cevaplar.GrupSatirlari("bb_olculer").Add(new() { ["bbo_olcu_ad"] = "Salon" });
        Assert.Contains(KontrolFormDogrulayici.CevaplariDogrula(bolumTanimi, cevaplar, (_, _) => 0, true), h => h.Contains("zorunludur"));
        cevaplar.Gruplar["bb_olculer"][0]["bbo_olcu_alan"] = "45";
        cevaplar.Gruplar["bb_olculer"].Add(new() { ["bbo_olcu_ad"] = "Mutfak", ["bbo_olcu_alan"] = "12.5" });
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(bolumTanimi, cevaplar, (_, _) => 0, true));
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [bolum]));
        Assert.Contains("Satır Ekle", html);
        Assert.Contains("Salon", html);
        Assert.Contains("45", html);
        Assert.Contains("m²", html);
        Assert.DoesNotContain("#1", html);
        Assert.DoesNotContain("#2", html);
        Assert.Contains("Ölçü satırını sil", html);
        var readOnly = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [bolum], true));
        Assert.DoesNotContain("Satır Ekle", readOnly);
        Assert.DoesNotContain("#1", readOnly);
        Assert.DoesNotContain("Ölçü satırını sil", readOnly);
        Assert.Contains("Salon", readOnly);
        Assert.Contains(KontrolFormAdimlari.AlanAra(KontrolFormAdimlari.Olustur(tanim), "metrekare"), a => a.GrupKodu == "bb_olculer");
        var tekrarSnapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = tanim.ToJson() });
        Assert.Single(KontrolRaporu.TanimAl(tekrarSnapshot).Bolumler.Single(b => b.Kod == "bb_ozellik").Gruplar, g => g.Kod == "bb_olculer");
    }

    [Theory]
    [InlineData("Zemin Tipi", "t_zemin_tipi", 0)]
    [InlineData("zemin tipi", "t_zemin_tipi", 0)]
    [InlineData("ISITMA", "bina_isitma", 2)]
    [InlineData("enerji kimlik", "bina_enerji_kimlik", 2)]
    [InlineData("emsal bedel", "emsal_bedel", 3)]
    [InlineData("imzalayan makam", "rapor_imzalayan", 0)]
    public void AlanAramasi_TumAdimlariTurkceKarakterFarkiOlmadanTarar(string sorgu, string kod, int adim)
    {
        var adimlar = KontrolFormAdimlari.Olustur(KontrolRaporu.Olustur());
        var sonuclar = KontrolFormAdimlari.AlanAra(adimlar, sorgu);
        Assert.Contains(sonuclar, s => s.AlanKodu == kod && s.Adim == adim);
        Assert.All(sonuclar, s => Assert.False(string.IsNullOrWhiteSpace(s.Yol)));
    }

    [Fact]
    public void AlanAramasi_AyniAdliVeTekrarlananAlanlarinKonumunuAyirtEder()
    {
        var adimlar = KontrolFormAdimlari.Olustur(KontrolRaporu.Olustur());
        var cepheler = KontrolFormAdimlari.AlanAra(adimlar, "Dış Cephe");
        Assert.Contains(cepheler, s => s.AlanKodu == "bina_dis_cephe" && s.Yol.Contains("Bina Özellikleri"));
        Assert.Contains(cepheler, s => s.AlanKodu == "bbo_cephe" && s.Yol.Contains("Bağımsız Bölüm Özellikleri"));
        var bedel = Assert.Single(KontrolFormAdimlari.AlanAra(adimlar, "emsal bedel"), s => s.AlanKodu == "emsal_bedel");
        Assert.Equal("emsaller", bedel.GrupKodu);
        Assert.Empty(KontrolFormAdimlari.AlanAra(adimlar, "   "));
        Assert.Empty(KontrolFormAdimlari.AlanAra(adimlar, "xyz-bulunamayan"));
    }

    [Fact]
    public async Task DisCepheSistemleri_BaslikVeSeceneklerleGosterilir()
    {
        var tanim = KontrolRaporu.TanimAl(null);
        var bina = tanim.Bolumler.Single(b => b.Kod == "bina");
        var alan = bina.Alanlar.Single(a => a.Kod == "bina_dis_cephe");
        Assert.Equal("Dış Cephe Sistemleri", alan.Baslik);
        Assert.Equal(KontrolAlanTipi.TekliSecim, alan.Tip);
        Assert.Equal(new[]
        {
            "Mantolama (Isı Yalıtım) Sistemleri",
            "Giydirme Cephe Sistemleri",
            "Mekanik Cephe Sistemleri",
            "Sıva ve Boya Sistemleri"
        }, alan.Secenekler.OrderBy(s => s.Sira).Select(s => s.Etiket));
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy(alan.Kod, "Giydirme");
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (_, _) => 0, false));
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [bina]));
        Assert.Matches("<select[^>]*id=\"f_bina_dis_cephe_x\"", html);
        Assert.Contains("Dış Cephe Sistemleri", html);
        Assert.Contains("Giydirme Cephe Sistemleri</option>", html);
    }

    [Theory]
    [InlineData("Siva", "Sıva ve Boya Sistemleri")]
    [InlineData("Mantolama", "Mantolama (Isı Yalıtım) Sistemleri")]
    [InlineData("Kaplama", "Kaplama (Kayıtlı değer)")]
    public void DisCepheSistemleri_EskiKayitliDegeriKorur(string kayitliDeger, string etiket)
    {
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy("bina_dis_cephe", kayitliDeger);
        var tanim = KontrolRaporu.TanimAl(null, cevaplar.ToJson());
        var alan = tanim.AlanBul("bina_dis_cephe")!;
        Assert.Contains(alan.Secenekler, s => s.Kod == kayitliDeger && s.Etiket == etiket);
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (_, _) => 0, false));
    }

    [Fact]
    public async Task DisCepheSayisi_BaslikVeBirdenBeseKadarSeceneklerleGosterilir()
    {
        var tanim = KontrolRaporu.TanimAl(null);
        var bolum = tanim.Bolumler.Single(b => b.Kod == "bb_ozellik");
        var alan = bolum.Alanlar.Single(a => a.Kod == "bbo_cephe");
        Assert.Equal("Dış Cephe Sayısı", alan.Baslik);
        Assert.Equal(KontrolAlanTipi.TekliSecim, alan.Tip);
        Assert.Equal(new[] { "1", "2", "3", "4", "5" }, alan.Secenekler.OrderBy(s => s.Sira).Select(s => s.Etiket));
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy(alan.Kod, "3");
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (_, _) => 0, false));
        Assert.Equal("3", KontrolCevaplari.FromJson(cevaplar.ToJson()).Al(alan.Kod));
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [bolum]));
        Assert.Matches("<select[^>]*id=\"f_bbo_cephe_x\"", html);
        Assert.Contains("Dış Cephe Sayısı", html);
    }

    [Fact]
    public void DisCephe_EskiSerbestMetinDegeriniKorur()
    {
        var eski = KontrolRaporu.Olustur();
        var alan = eski.AlanBul("bbo_cephe")!;
        alan.Baslik = "Cephe";
        alan.Tip = KontrolAlanTipi.KisaMetin;
        alan.Secenekler.Clear();
        var snapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = eski.ToJson() });
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy("bbo_cephe", "Kuzey");
        var tanim = KontrolRaporu.TanimAl(snapshot, cevaplar.ToJson());
        Assert.Equal("Dış Cephe Sayısı", tanim.AlanBul("bbo_cephe")!.Baslik);
        Assert.Equal(KontrolAlanTipi.TekliSecim, tanim.AlanBul("bbo_cephe")!.Tip);
        Assert.Contains(tanim.AlanBul("bbo_cephe")!.Secenekler, s => s.Kod == "Kuzey" && s.Etiket.Contains("Kayıtlı değer"));
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (_, _) => 0, false));
    }

    [Theory]
    [InlineData(false, "Var")]
    [InlineData(true, "Yok")]
    public async Task Esya_ZeminKaplamasindanSonraSeceneklerleGosterilirVeKaydedilir(bool eskiRapor, string secim)
    {
        var tanim = KontrolRaporu.Olustur();
        if (eskiRapor)
        {
            tanim.Bolumler.Single(b => b.Kod == "bb_ozellik").Alanlar.RemoveAll(a => a.Kod == "bbo_esya");
            var snapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = tanim.ToJson() });
            tanim = KontrolRaporu.TanimAl(snapshot);
        }
        var bolum = tanim.Bolumler.Single(b => b.Kod == "bb_ozellik");
        var alan = Assert.Single(tanim.TumAlanlar(), a => a.Kod == "bbo_esya");
        Assert.Equal(KontrolAlanTipi.TekliSecim, alan.Tip);
        Assert.Equal(new[] { "Var", "Yok" }, alan.Secenekler.Select(s => s.Etiket));
        Assert.Equal(bolum.Alanlar.FindIndex(a => a.Kod == "bbo_zemin") + 1, bolum.Alanlar.IndexOf(alan));
        Assert.Equal(tanim.AlanBul("bbo_zemin")!.Sira + 1, alan.Sira);
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy("bbo_esya", secim);
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (_, _) => 0, false));
        Assert.Equal(secim, KontrolCevaplari.FromJson(cevaplar.ToJson()).Al("bbo_esya"));
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [bolum]));
        Assert.Matches("<select[^>]*id=\"f_bbo_esya_x\"", html);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Mutfak_SalondanSonraSeceneklerleGosterilirVeKaydedilir(bool eskiRapor)
    {
        var tanim = KontrolRaporu.Olustur();
        if (eskiRapor)
        {
            tanim.Bolumler.Single(b => b.Kod == "bb_ozellik").Alanlar.RemoveAll(a => a.Kod == "bbo_mutfak");
            var snapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = tanim.ToJson() });
            tanim = KontrolRaporu.TanimAl(snapshot);
        }
        var bolum = tanim.Bolumler.Single(b => b.Kod == "bb_ozellik");
        var alan = Assert.Single(tanim.TumAlanlar(), a => a.Kod == "bbo_mutfak");
        Assert.Equal(KontrolAlanTipi.TekliSecim, alan.Tip);
        Assert.Equal(new[] { "Açık (Amerikan)", "Kapalı", "Diğer" }, alan.Secenekler.Select(s => s.Etiket));
        Assert.Equal(bolum.Alanlar.FindIndex(a => a.Kod == "bbo_salon") + 1, bolum.Alanlar.IndexOf(alan));
        Assert.Equal(tanim.AlanBul("bbo_salon")!.Sira + 1, alan.Sira);
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy("bbo_mutfak", "Kapalı");
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (_, _) => 0, false));
        Assert.Equal("Kapalı", KontrolCevaplari.FromJson(cevaplar.ToJson()).Al("bbo_mutfak"));
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [bolum]));
        Assert.Matches("<select[^>]*id=\"f_bbo_mutfak_x\"", html);
    }

    [Fact]
    public async Task ZeminTipi_YevmiyeSonrasindaTasinmazdanGosterilir()
    {
        var tanim = KontrolRaporu.Olustur();
        var ana = tanim.Bolumler.Single(b => b.Kod == "ana_tasinmaz");
        var alan = tanim.AlanBul("t_zemin_tipi")!;
        Assert.Equal(ana.Alanlar.FindIndex(a => a.Kod == "t_yevmiye") + 1, ana.Alanlar.IndexOf(alan));
        Assert.Equal(KontrolVeriKaynagiTipi.TasinmazSaltOkunur, alan.VeriKaynagi);
        var snapshot = TasinmazVeriKaynagi.Snapshot(new Jenga.Models.TBYS.Tasinmaz { ZeminTipi = "Ana Taşınmaz" }, tanim);
        Assert.Equal("Ana Taşınmaz", snapshot["ZeminTipi"]);
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy(alan.Kod, snapshot[alan.TasinmazAlani!]);
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [ana]));
        Assert.Contains("Zemin Tipi", html);
        Assert.Contains("Ana Taşınmaz", html);
    }

    [Fact]
    public void ZeminTipi_EskiRaporlaraTekKezEklenir()
    {
        var eski = KontrolRaporu.Olustur();
        eski.Bolumler.Single(b => b.Kod == "ana_tasinmaz").Alanlar.RemoveAll(a => a.Kod == "t_zemin_tipi");
        var snapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = eski.ToJson() });
        var tanim = KontrolRaporu.TanimAl(snapshot);
        var alan = Assert.Single(tanim.TumAlanlar(), a => a.Kod == "t_zemin_tipi");
        Assert.Equal(tanim.AlanBul("t_yevmiye")!.Sira + 1, alan.Sira);
        var tekrarSnapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = tanim.ToJson() });
        Assert.Single(KontrolRaporu.TanimAl(tekrarSnapshot).TumAlanlar(), a => a.Kod == "t_zemin_tipi");
    }

    [Fact]
    public async Task EnerjiKimlikBelgesi_IsitmaninYanindaIstenenSecenekleriGosterir()
    {
        var tanim = KontrolRaporu.Olustur();
        var bina = tanim.Bolumler.Single(b => b.Kod == "bina");
        var alan = tanim.AlanBul("bina_enerji_kimlik")!;
        Assert.Equal(KontrolAlanTipi.TekliSecim, alan.Tip);
        Assert.Equal(new[] { "YOK", "A", "B", "C", "D", "E", "F", "G" }, alan.Secenekler.Select(s => s.Kod));
        Assert.Equal(bina.Alanlar.FindIndex(a => a.Kod == "bina_isitma") + 1, bina.Alanlar.IndexOf(alan));
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy(alan.Kod, "B");
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (_, _) => 0, false));
        Assert.Equal("B", KontrolCevaplari.FromJson(cevaplar.ToJson()).Al(alan.Kod));
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, [bina]));
        Assert.Matches("<select[^>]*id=\"f_bina_enerji_kimlik_x\"", html);
    }

    [Fact]
    public void EnerjiKimlikBelgesi_EskiRaporaTekKezEklenir_DigerCevaplarKorunur()
    {
        var eski = KontrolRaporu.Olustur();
        eski.Bolumler.Single(b => b.Kod == "bina").Alanlar.RemoveAll(a => a.Kod == "bina_enerji_kimlik");
        var snapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = eski.ToJson() });
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy("bina_isitma", "Klima");
        var tanim = KontrolRaporu.TanimAl(snapshot, cevaplar.ToJson());
        var alan = Assert.Single(tanim.TumAlanlar(), a => a.Kod == "bina_enerji_kimlik");
        Assert.Equal(tanim.AlanBul("bina_isitma")!.Sira + 1, alan.Sira);
        Assert.Null(cevaplar.Al(alan.Kod));
        Assert.Equal("Klima", cevaplar.Al("bina_isitma"));
        var tekrarSnapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = tanim.ToJson() });
        Assert.Single(KontrolRaporu.TanimAl(tekrarSnapshot).TumAlanlar(), a => a.Kod == "bina_enerji_kimlik");
    }

    [Fact]
    public async Task IsitmaSistemi_IstenenSeceneklerleAcilirListeGosterir()
    {
        var tanim = KontrolRaporu.Olustur();
        var alan = tanim.AlanBul("bina_isitma")!;
        Assert.Equal(KontrolAlanTipi.TekliSecim, alan.Tip);
        Assert.Equal(new[]
        {
            "Yok", "Soba", "Doğalgaz Sobası", "Kat Kaloriferi", "Merkezi", "Merkezi (Pay Ölçer)",
            "Kombi Doğalgaz", "Kombi Elektrik", "Yerden Isıtma", "Klima", "Fancoil Ünitesi",
            "Güneş Enerjisi", "Elektrikli Radyatör", "Jeotermal", "Şömine", "VRV", "Isı Pompası"
        }, alan.Secenekler.OrderBy(s => s.Sira).Select(s => s.Etiket));
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, new(), tanim.Bolumler.Where(b => b.Kod == "bina").ToList()));
        Assert.Matches("<select[^>]*id=\"f_bina_isitma_x\"", html);
        Assert.Matches("<option[^>]*value=\"Isı Pompası\"[^>]*>Isı Pompası</option>", html);
    }

    [Fact]
    public async Task IsitmaSistemi_EskiSerbestMetinDegeriGorunurVeKaydedilebilirKalir()
    {
        var eski = KontrolRaporu.Olustur();
        var alan = eski.AlanBul("bina_isitma")!;
        alan.Tip = KontrolAlanTipi.KisaMetin;
        alan.Secenekler.Clear();
        var snapshot = JsonSerializer.Serialize(new Dictionary<string, string?> { [KontrolRaporu.EskiAlanlarAnahtari] = eski.ToJson() });
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy("bina_isitma", "Eski özel ısıtma");
        var tanim = KontrolRaporu.TanimAl(snapshot, cevaplar.ToJson());
        Assert.Equal(KontrolAlanTipi.TekliSecim, tanim.AlanBul("bina_isitma")!.Tip);
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (_, _) => 0, false));
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, tanim.Bolumler.Where(b => b.Kod == "bina").ToList(), true));
        Assert.Contains("Eski özel ısıtma (Kayıtlı değer)", html);
        Assert.Equal("Eski özel ısıtma", cevaplar.Al("bina_isitma"));
    }

    [Fact]
    public void YeniKontrol_SabitRaporVeStandartPeriyotKullanir()
    {
        Assert.Equal(KontrolRaporu.Olustur().ToJson(), KontrolRaporu.TanimAl("{}").ToJson());
        Assert.Equal((1, PeriyotBirimi.Yil), KontrolRaporu.PeriyotAl("{}"));
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(local);Database=KontrolModelTest;Integrated Security=True;TrustServerCertificate=True").Options);
        Assert.DoesNotContain(db.Model.GetEntityTypes(), t => t.GetTableName() == "TasinmazKontrolForm_Table");
        Assert.Null(db.Model.FindEntityType(typeof(TasinmazKontrol))!.FindProperty("FormId"));
    }

    [Fact]
    public void GecmisRapor_AlanlariPeriyoduVeRevizyonuSablonTablosuOlmadanKorunur()
    {
        var eski = KontrolRaporu.Olustur();
        eski.Bolumler.Add(new() { Kod = "eski_ozel", Baslik = "Eski özel bölüm", Alanlar = { new() { Kod = "eski_not", Baslik = "Özel not" } } });
        var snapshot = new Dictionary<string, string?>
        {
            [KontrolRaporu.EskiAlanlarAnahtari] = eski.ToJson(),
            ["_kontrolPeriyotSure"] = "3", ["_kontrolPeriyotBirim"] = "2", ["Adres"] = "Eski adres"
        };
        // SQL geçişi alan tanımını snapshot içinde JSON metni olarak taşır (4000 karakterden uzun olabilir).
        var json = JsonSerializer.Serialize(snapshot);
        Assert.True(eski.ToJson().Length > 4000);
        Assert.NotNull(KontrolRaporu.TanimAl(json).AlanBul("eski_not"));
        Assert.Equal((3, PeriyotBirimi.Ay), KontrolRaporu.PeriyotAl(json));

        var revizyonSnapshot = new Dictionary<string, string?> { ["Adres"] = "Güncel adres" };
        KontrolRaporu.GecmisBilgileriniKoru(revizyonSnapshot, json);
        var revizyonJson = JsonSerializer.Serialize(revizyonSnapshot);
        Assert.NotNull(KontrolRaporu.TanimAl(revizyonJson).AlanBul("eski_not"));
        Assert.Equal((3, PeriyotBirimi.Ay), KontrolRaporu.PeriyotAl(revizyonJson));
        Assert.Equal("Güncel adres", revizyonSnapshot["Adres"]);
    }

    [Fact]
    public void VarsayilanForm_ButunBolumleriBirKezGruplar_OrijinaliDegistirmez()
    {
        var tanim = KontrolRaporu.Olustur();
        var once = tanim.ToJson();
        var adimlar = KontrolFormAdimlari.Olustur(tanim);
        Assert.Equal(7, adimlar.Count); // Son kontrol ekranıyla birlikte sekiz adım.
        var bolumler = adimlar.SelectMany(a => a.Bolumler).ToList();
        Assert.Equal(tanim.Bolumler.Count, bolumler.Count);
        Assert.All(tanim.Bolumler, b => Assert.Single(bolumler, x => ReferenceEquals(x, b)));
        Assert.Equal(once, tanim.ToJson());
    }

    [Fact]
    public void OzelForm_BilinmeyenBolumlerVeBosFormIcinRaporAdimiKorunur()
    {
        var tanim = new KontrolFormTanimi();
        Assert.Single(KontrolFormAdimlari.Olustur(tanim));
        var ozel = new KontrolBolumTanimi { Kod = "ozel", Baslik = "Özel denetim", Sira = 1 };
        var bina = new KontrolBolumTanimi { Kod = "BINA", Baslik = "Bina", Sira = 2 };
        tanim.Bolumler.AddRange([ozel, bina]);
        var adimlar = KontrolFormAdimlari.Olustur(tanim);
        Assert.Empty(adimlar[0].Bolumler);
        Assert.Same(bina, Assert.Single(adimlar[1].Bolumler));
        Assert.Same(ozel, Assert.Single(adimlar[2].Bolumler));
    }

    [Fact]
    public async Task Renderer_SadeceSeciliBolumleriGosterir_CevaplarVeEkBaglantilariKorunur()
    {
        var tanim = KontrolRaporu.Olustur();
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy("bina_insa_yili", "1998");
        cevaplar.Koy("kanaat_ozet", "Önceki adımda girilen değerlendirme");
        var html = await RenderAsync(tanim, cevaplar, tanim.Bolumler.Where(b => b.Kod == "bina").ToList());
        Assert.Contains("1998", html);
        Assert.Contains("kontrol-alan-mini", html);
        Assert.DoesNotContain("kanaat_ozet", html);
        Assert.Equal("Önceki adımda girilen değerlendirme", cevaplar.Al("kanaat_ozet"));

        var ekHtml = await RenderAsync(tanim, cevaplar, tanim.Bolumler.Where(b => b.Kod == "foto").ToList(), true,
            [new() { Id = 42, AlanKodu = "foto_dis", DosyaAdi = "cephe.jpg" }]);
        Assert.Contains("/api/tbys/kontrol-ek/42", ekHtml);
        Assert.Contains("cephe.jpg", ekHtml);
        Assert.DoesNotContain("bi-x-circle", ekHtml);
    }

    [Fact]
    public async Task Renderer_TumFormOnizlemesi_VeSaltOkunurDegerlerCalisir()
    {
        var tanim = KontrolRaporu.Olustur();
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy("kanaat_ozet", "Saklanan değerlendirme");
        var html = await RenderAsync(tanim, cevaplar, null, true);
        Assert.Contains("f_bina_insa_yili_x", html);
        Assert.Contains("f_kanaat_ozet_x", html);
        Assert.Contains("Saklanan de", html);
        Assert.Contains("disabled", html);
        Assert.DoesNotContain("Satır Ekle", System.Net.WebUtility.HtmlDecode(html));
    }

    [Fact]
    public async Task Kanaat_ZorunluAlanlariVeDegerlendirmeRehberiniGosterir()
    {
        var tanim = KontrolRaporu.Olustur();
        var bolumler = tanim.Bolumler.Where(b => b.Kod == "kanaat").ToList();
        var cevaplar = new KontrolCevaplari();
        var html = System.Net.WebUtility.HtmlDecode(await RenderAsync(tanim, cevaplar, bolumler));
        Assert.Contains("alanları mutlaka doldurulmalıdır", html);
        foreach (var konu in new[] { "tadilata", "rayice", "DASK", "kiraya verilebilirliği", "çevresel faktörler", "Tarla ve arsaların" })
            Assert.Contains(konu, html);

        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (_, _) => 0, zorunluKontrol: false));
        var hatalar = KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (_, _) => 0, zorunluKontrol: true);
        Assert.Contains("'Genel Değerlendirme' zorunludur.", hatalar);
        Assert.Contains("'Öneriler' zorunludur.", hatalar);
        cevaplar.Koy("kanaat_ozet", "Taşınmaz iyi durumdadır.");
        Assert.Contains("'Öneriler' zorunludur.", KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (_, _) => 0, zorunluKontrol: true));
        cevaplar.Koy("kanaat_oneri", "Mevcut kullanım sürdürülebilir.");
        cevaplar.Koy("siklik_oneri", "1Yil");
        Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (_, _) => 0, zorunluKontrol: true));
    }

    private static async Task<string> RenderAsync(KontrolFormTanimi tanim, KontrolCevaplari cevaplar,
        IReadOnlyList<KontrolBolumTanimi>? bolumler, bool readOnly = false, IReadOnlyList<TasinmazKontrolEk>? ekler = null)
    {
        using var services = new ServiceCollection().AddLogging().AddSingleton<IJSRuntime, KonumJS>().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(KontrolFormRenderer.Tanim)] = tanim,
                [nameof(KontrolFormRenderer.Cevaplar)] = cevaplar,
                [nameof(KontrolFormRenderer.Bolumler)] = bolumler,
                [nameof(KontrolFormRenderer.ReadOnly)] = readOnly,
                [nameof(KontrolFormRenderer.Ekler)] = ekler ?? Array.Empty<TasinmazKontrolEk>()
            });
            var rendered = await renderer.RenderComponentAsync<KontrolFormRenderer>(parameters);
            return rendered.ToHtmlString();
        });
    }
}
