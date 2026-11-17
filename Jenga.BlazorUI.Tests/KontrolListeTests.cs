using System.Net;
using System.Reflection;
using Jenga.BlazorUI.Components.Pages.TBYS.Kontrol;
using Jenga.BlazorUI.Services.Common;
using Jenga.BlazorUI.Services.Common.Toast;
using Jenga.DataAccess.Services.TBYS;
using Jenga.Models.TBYS.Kontrol;
using Jenga.Utility.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public class KontrolListeTests
{
    [Fact]
    public async Task Gecmis_GecikenSorguBitinceYuklemeKapanir_FiltreKaldirmaVeSekmelerYenilenir()
    {
        await using var ekran = new Ekran();
        await ekran.AcAsync();
        var tiklama = ekran.TiklaAsync("Geçmiş");
        await ekran.SorguBasladi.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Contains("Veriler hazırlanıyor", await ekran.HtmlAsync());
        Assert.False(tiklama.IsCompleted);
        Assert.Equal(42, ekran.SonFiltre);
        ekran.Sonuc.SetResult(Result<List<TasinmazKontrolOzet>>.Success([new() { Id = 7, TasinmazId = 42, RaporNo = "TEST-RAPOR", KontrolTarihi = DateTime.Today }]));
        await tiklama;
        Assert.DoesNotContain("Veriler hazırlanıyor", await ekran.HtmlAsync());
        Assert.Contains("TEST-RAPOR", await ekran.HtmlAsync());
        await ekran.TiklaAsync("Taşınmaz filtresini kaldır");
        Assert.Null(ekran.SonFiltre);
        Assert.Equal(2, ekran.GecmisSorgulari);
        await ekran.TiklaAsync("Planlama");
        Assert.Equal(2, ekran.PlanSorgulari);
        await ekran.TiklaAsync("Kontrol Geçmişi");
        Assert.Equal(3, ekran.GecmisSorgulari);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Gecmis_HatadaYuklemeKapanir_YenidenDenemekMumkundur(bool beklenmeyenHata)
    {
        await using var ekran = new Ekran();
        await ekran.AcAsync();
        var tiklama = ekran.TiklaAsync("Geçmiş");
        await ekran.SorguBasladi.Task.WaitAsync(TimeSpan.FromSeconds(5));
        if (beklenmeyenHata) ekran.Sonuc.SetException(new InvalidOperationException("Test hatası"));
        else ekran.Sonuc.SetResult(Result<List<TasinmazKontrolOzet>>.Failure(Error.Unexpected("Liste alınamadı.")));
        await tiklama;
        Assert.DoesNotContain("Veriler hazırlanıyor", await ekran.HtmlAsync());
        Assert.Single(ekran.Hatalar);
        await ekran.TiklaAsync("Planlama");
        Assert.Equal(2, ekran.PlanSorgulari);
    }

    private sealed class Ekran : IAsyncDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly TestRenderer _renderer;
        private HtmlRootComponent _root;
        public readonly TaskCompletionSource SorguBasladi = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public readonly TaskCompletionSource<Result<List<TasinmazKontrolOzet>>> Sonuc = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public readonly List<string> Hatalar = [];
        public int? SonFiltre;
        public int PlanSorgulari, GecmisSorgulari;

        public Ekran()
        {
            var services = new ServiceCollection().AddLogging();
            services.AddSingleton(Fake<ITasinmazKontrolService>.Olustur((ad, args) =>
            {
                if (ad == "GetPlanListesiAsync")
                {
                    PlanSorgulari++;
                    return Task.FromResult(Result<List<TasinmazPlanOzet>>.Success([new() { TasinmazId = 42, EmlakSicilNo = "TEST", Adres = "Test adresi" }]));
                }
                if (ad == "GetListeAsync")
                {
                    GecmisSorgulari++;
                    SonFiltre = (int?)args![0];
                    SorguBasladi.TrySetResult();
                    return Sonuc.Task;
                }
                throw new InvalidOperationException(ad);
            }));
            services.AddSingleton(Fake<ICurrentUserService>.Olustur((_, _) => Task.FromResult(true)));
            services.AddSingleton(Fake<IToastService>.Olustur((ad, args) => { if (ad == "ShowError") Hatalar.Add((string)args![0]!); return null; }));
            services.AddSingleton<NavigationManager, TestNavigation>();
            services.AddSingleton<IJSRuntime, TestJS>();
            _provider = services.BuildServiceProvider();
            _renderer = new(_provider, _provider.GetRequiredService<ILoggerFactory>());
        }

        public Task AcAsync() => _renderer.Dispatcher.InvokeAsync(async () =>
        {
            _root = _renderer.BeginRenderingComponent(typeof(TasinmazKontrolListe), ParameterView.Empty);
            await _root.QuiescenceTask;
        });
        public Task TiklaAsync(string etiket) => _renderer.Dispatcher.InvokeAsync(() =>
            _renderer.DispatchEventAsync(_renderer.DugmeBul(0, etiket) ?? throw new InvalidOperationException(etiket), null, new MouseEventArgs()));
        public Task<string> HtmlAsync() => _renderer.Dispatcher.InvokeAsync(() => WebUtility.HtmlDecode(_root.ToHtmlString()));
        public async ValueTask DisposeAsync() { await _renderer.DisposeAsync(); await _provider.DisposeAsync(); }
    }

#pragma warning disable BL0006
    private sealed class TestRenderer(IServiceProvider services, ILoggerFactory loggerFactory) : StaticHtmlRenderer(services, loggerFactory)
    {
        protected override IComponent ResolveComponentForRenderMode(Type componentType, int? parentComponentId,
            IComponentActivator componentActivator, IComponentRenderMode renderMode) => componentActivator.CreateInstance(componentType);

        public ulong? DugmeBul(int componentId, string etiket)
        {
            var frames = GetCurrentRenderTreeFrames(componentId);
            for (var i = 0; i < frames.Count; i++)
            {
                var frame = frames.Array[i];
                if (frame.FrameType == RenderTreeFrameType.Component && DugmeBul(frame.ComponentId, etiket) is ulong childId) return childId;
                if (frame.FrameType != RenderTreeFrameType.Element || frame.ElementName != "button") continue;
                ulong? id = null;
                var eslesti = false;
                for (var j = i + 1; j < i + frame.ElementSubtreeLength; j++)
                {
                    var item = frames.Array[j];
                    if (item.FrameType == RenderTreeFrameType.Attribute)
                    {
                        if (item.AttributeName == "onclick") id = item.AttributeEventHandlerId;
                        if (item.AttributeName == "title" && Equals(item.AttributeValue, etiket)) eslesti = true;
                    }
                    if (item.FrameType == RenderTreeFrameType.Text && item.TextContent.Trim() == etiket) eslesti = true;
                    if (item.FrameType == RenderTreeFrameType.Markup && item.MarkupContent.Trim() == etiket) eslesti = true;
                }
                if (eslesti && id.HasValue) return id;
            }
            return null;
        }
    }
#pragma warning restore BL0006

    public class Fake<T> : DispatchProxy where T : class
    {
        private Func<string, object?[]?, object?> _cagir = null!;
        public static T Olustur(Func<string, object?[]?, object?> cagir)
        {
            var proxy = Create<T, Fake<T>>();
            ((Fake<T>)(object)proxy)._cagir = cagir;
            return proxy;
        }
        protected override object? Invoke(MethodInfo? method, object?[]? args) => _cagir(method!.Name, args);
    }
    private sealed class TestNavigation : NavigationManager
    {
        public TestNavigation() => Initialize("http://localhost/", "http://localhost/tbys/tasinmaz-kontrol");
    }
    private sealed class TestJS : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult(default(TValue)!);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }
}
