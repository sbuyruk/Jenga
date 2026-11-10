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

namespace Jenga.BlazorUI.Tests;

public class KontrolFormTests
{
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

    private static async Task<string> RenderAsync(KontrolFormTanimi tanim, KontrolCevaplari cevaplar,
        IReadOnlyList<KontrolBolumTanimi>? bolumler, bool readOnly = false, IReadOnlyList<TasinmazKontrolEk>? ekler = null)
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
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
