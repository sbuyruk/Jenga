using System.Reflection;
using System.Text.RegularExpressions;
using Jenga.BlazorUI.Components.Pages.TBYS;
using Jenga.BlazorUI.Components.Pages.TBYS.Kontrol;
using Jenga.DataAccess.Services.TBYS;
using Jenga.Utility.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public class TasinmazHaritaTests
{
    [Fact]
    public async Task EskiAdres_SorguyuKoruyarakTbysHaritasinaYonlendirir()
    {
        var eskiRota = typeof(TasinmazHaritaYonlendirme).GetCustomAttribute<RouteAttribute>()!.Template;
        Assert.Equal("/tbys/tasinmaz-kontrol/harita", eskiRota);
        var navigation = new HaritaNavigation();
        using var services = new ServiceCollection().AddLogging()
            .AddSingleton<NavigationManager>(navigation)
            .BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());

        await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<TasinmazHaritaYonlendirme>());

        var yeniRota = typeof(TasinmazHaritaSayfasi).GetCustomAttribute<RouteAttribute>()!.Template;
        Assert.Equal("/tbys/tasinmaz-harita", yeniRota);
        Assert.Equal($"{yeniRota}?il=Ankara#harita", navigation.Hedef);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Harita_BosVeHataDurumlariniAciklar(bool hata)
    {
        using var services = Services(hata ? Result<List<TasinmazHaritaNoktasi>>.Failure(Error.Unexpected("Test hatası")) : Result<List<TasinmazHaritaNoktasi>>.Success([]));
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var root = await renderer.RenderComponentAsync<TasinmazHarita>();
            var html = System.Net.WebUtility.HtmlDecode(root.ToHtmlString());
            Assert.Contains(hata ? "Test hatası" : "kayıtlı geçerli koordinat bulunamadı", html);
            if (hata) Assert.Contains("Yeniden Dene", html);
        });
    }

    [Fact]
    public async Task Harita_AramaCinsVeKiraDurumuFiltreleriBirlikteCalisir()
    {
        const string yenidenInsa = "Kirada Değil - Yeniden İnşa";
        using var services = Services(Result<List<TasinmazHaritaNoktasi>>.Success([
            new() { TasinmazId = 1, EmlakSicilNo = "S-1", Cinsi = "Mesken", Adres = "Çankaya Test Sokak", Ili = "Ankara", KiraDurumu = yenidenInsa, Enlem = 39, Boylam = 32 },
            new() { TasinmazId = 2, EmlakSicilNo = "S-2", Cinsi = "Arsa", Adres = "Çankaya Diğer Sokak", Ili = "Ankara", KiraDurumu = yenidenInsa, ParselGeometrisiGeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[32,39],[33,39],[33,40],[32,39]]]}" },
            new() { TasinmazId = 3, EmlakSicilNo = "S-3", Cinsi = "Mesken", Adres = "Çankaya Test Sokak", Ili = "Ankara", KiraDurumu = "Kiralandı", Enlem = 40, Boylam = 33 },
            new() { TasinmazId = 4, EmlakSicilNo = "S-4", Cinsi = "Arsa", Ili = "Ankara" }
        ]));
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            TasinmazHarita? form = null;
            var root = await renderer.RenderComponentAsync<Host>(ParameterView.FromDictionary(new Dictionary<string, object?>
            { [nameof(Host.Capture)] = (Action<TasinmazHarita>)(h => form = h) }));
            Assert.NotNull(form);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            void Filtre(string arama, string cinsi, string kiraDurumu = "")
            {
                typeof(TasinmazHarita).GetField("_arama", flags)!.SetValue(form, arama);
                typeof(TasinmazHarita).GetField("_cinsi", flags)!.SetValue(form, cinsi);
                typeof(TasinmazHarita).GetField("_kiraDurumu", flags)!.SetValue(form, kiraDurumu);
                typeof(TasinmazHarita).GetMethod("Filtrele", flags)!.Invoke(form, null);
                typeof(ComponentBase).GetMethod("StateHasChanged", flags)!.Invoke(form, null);
            }
            List<TasinmazHaritaNoktasi> Gosterilen() => (List<TasinmazHaritaNoktasi>)typeof(TasinmazHarita).GetField("_gosterilen", flags)!.GetValue(form)!;
            Assert.Equal(4, Gosterilen().Count);
            var html = System.Net.WebUtility.HtmlDecode(root.ToHtmlString());
            var kiraSecici = Regex.Match(html, """<select id="harita-kira-durumu"[^>]*>(.*?)</select>""", RegexOptions.Singleline);
            var secenekler = Regex.Matches(kiraSecici.Groups[1].Value, """<option\b([^>]*)>([^<]*)</option>""")
                .Select(eslesme => (
                    Deger: Regex.Match(eslesme.Groups[1].Value, "\\svalue=\"([^\"]*)\"").Groups[1].Value,
                    Etiket: eslesme.Groups[2].Value));
            Assert.Equal(new[]
            {
                ("", "Tümü"),
                ("Kiralandı", "Kiralandı"),
                ("Kirada Değil - Boş", "Kirada Değil - Boş"),
                ("Kirada Değil - Çıplak Mülkiyet", "Kirada Değil - Çıplak Mülkiyet"),
                ("Kirada Değil - Taahhütname Verilen", "Kirada Değil - Taahhütname Verilen"),
                ("Kirada Değil - Hisseli", "Kirada Değil - Hisseli"),
                ("Kirada Değil - Hukuki Sorun", "Kirada Değil - Hukuki Sorun"),
                ("Kirada Değil - Vakıf Kullanımında", "Kirada Değil - Vakıf Kullanımında"),
                ("Kirada Değil - Kiralanma Talebi Olmayan", "Kirada Değil - Kiralanma Talebi Olmayan"),
                (yenidenInsa, yenidenInsa)
            }, secenekler);
            Filtre("", "", "Kiralandı");
            Assert.Equal(3, Assert.Single(Gosterilen()).TasinmazId);
            Filtre("", "", yenidenInsa);
            Assert.Equal(new[] { 1, 2 }, Gosterilen().Select(n => n.TasinmazId));
            Filtre("cankaya ANKARA", "Mesken", yenidenInsa);
            Assert.Equal(1, Assert.Single(Gosterilen()).TasinmazId);
            Filtre("S-2", "");
            Assert.Equal(2, Assert.Single(Gosterilen()).TasinmazId);
            Filtre("bulunamayan", "", yenidenInsa);
            Assert.Empty(Gosterilen());
            Assert.Contains("eşleşen taşınmaz bulunamadı", System.Net.WebUtility.HtmlDecode(root.ToHtmlString()));
            typeof(TasinmazHarita).GetMethod("TumunuGoster", flags)!.Invoke(form, null);
            Assert.Equal(4, Gosterilen().Count);
        });
    }

    private static ServiceProvider Services(Result<List<TasinmazHaritaNoktasi>> sonuc) => new ServiceCollection().AddLogging()
        .AddSingleton(KontrolListeTests.Fake<ITasinmazKontrolService>.Olustur((ad, _) => ad == "GetHaritaNoktalariAsync" ? Task.FromResult(sonuc) : throw new InvalidOperationException(ad)))
        .AddSingleton<IJSRuntime, NullJS>().BuildServiceProvider();

    private sealed class HaritaNavigation : NavigationManager
    {
        public HaritaNavigation() => Initialize("http://localhost/", "http://localhost/tbys/tasinmaz-kontrol/harita?il=Ankara#harita");

        public string? Hedef { get; private set; }

        protected override void NavigateToCore(string uri, NavigationOptions options) => Hedef = uri;
    }

    public sealed class Host : ComponentBase
    {
        [Parameter] public Action<TasinmazHarita>? Capture { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<TasinmazHarita>(0);
            builder.AddComponentReferenceCapture(1, h => Capture?.Invoke((TasinmazHarita)h));
            builder.CloseComponent();
        }
    }
    private sealed class NullJS : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult(default(TValue)!);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }
}
