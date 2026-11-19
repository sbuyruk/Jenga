using System.Reflection;
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
    public async Task Harita_TurkceAramaVeCinsFiltresiBirlikteCalisir()
    {
        using var services = Services(Result<List<TasinmazHaritaNoktasi>>.Success([
            new() { TasinmazId = 1, EmlakSicilNo = "S-1", Cinsi = "Mesken", Adres = "Çankaya Test Sokak", Ili = "Ankara" },
            new() { TasinmazId = 2, EmlakSicilNo = "S-2", Cinsi = "Arsa", Adres = "Çankaya Diğer Sokak", Ili = "Ankara" }
        ]));
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            TasinmazHarita? form = null;
            var root = await renderer.RenderComponentAsync<Host>(ParameterView.FromDictionary(new Dictionary<string, object?>
            { [nameof(Host.Capture)] = (Action<TasinmazHarita>)(h => form = h) }));
            Assert.NotNull(form);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            void Filtre(string arama, string cinsi)
            {
                typeof(TasinmazHarita).GetField("_arama", flags)!.SetValue(form, arama);
                typeof(TasinmazHarita).GetField("_cinsi", flags)!.SetValue(form, cinsi);
                typeof(TasinmazHarita).GetMethod("Filtrele", flags)!.Invoke(form, null);
                typeof(ComponentBase).GetMethod("StateHasChanged", flags)!.Invoke(form, null);
            }
            List<TasinmazHaritaNoktasi> Gosterilen() => (List<TasinmazHaritaNoktasi>)typeof(TasinmazHarita).GetField("_gosterilen", flags)!.GetValue(form)!;
            Filtre("cankaya ANKARA", "Mesken");
            Assert.Equal(1, Assert.Single(Gosterilen()).TasinmazId);
            Filtre("S-2", "");
            Assert.Equal(2, Assert.Single(Gosterilen()).TasinmazId);
            Filtre("bulunamayan", "");
            Assert.Empty(Gosterilen());
            Assert.Contains("eşleşen taşınmaz bulunamadı", System.Net.WebUtility.HtmlDecode(root.ToHtmlString()));
            typeof(TasinmazHarita).GetMethod("TumunuGoster", flags)!.Invoke(form, null);
            Assert.Equal(2, Gosterilen().Count);
        });
    }

    private static ServiceProvider Services(Result<List<TasinmazHaritaNoktasi>> sonuc) => new ServiceCollection().AddLogging()
        .AddSingleton(KontrolListeTests.Fake<ITasinmazKontrolService>.Olustur((ad, _) => ad == "GetHaritaNoktalariAsync" ? Task.FromResult(sonuc) : throw new InvalidOperationException(ad)))
        .AddSingleton<IJSRuntime, NullJS>().BuildServiceProvider();

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
