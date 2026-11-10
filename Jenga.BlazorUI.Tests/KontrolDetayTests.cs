using System.Reflection;
using System.Text.Json;
using Jenga.BlazorUI.Components.Pages.TBYS.Kontrol;
using Jenga.BlazorUI.Services.Common;
using Jenga.BlazorUI.Services.Common.Toast;
using Jenga.DataAccess.Services.IKYS;
using Jenga.DataAccess.Services.TBYS;
using Jenga.Models.IKYS;
using Jenga.Models.TBYS.Kontrol;
using Jenga.Utility.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public class KontrolDetayTests
{
    [Fact]
    public async Task IleriGeri_CevaplariKorur_KayitMevcutAdimiSifirlamaz()
    {
        await using var test = await Ekran.AcAsync();
        await test.GitAsync(2);
        test.Cevaplar.Koy("bina_insa_yili", "1998");
        await test.GitAsync(3);
        await test.GitAsync(2);
        Assert.Equal("1998", test.Cevaplar.Al("bina_insa_yili"));
        await test.CagirAsync("TaslakKaydetAsync");
        Assert.Equal(2, test.AktifAdim);
        Assert.Equal("1998", KontrolCevaplari.FromJson(test.Servis.Kayit.CevaplarJson).Al("bina_insa_yili"));
        Assert.False(test.Kaydedilmemis);
        await test.CagirAsync("DevamEtAsync");
        Assert.Equal(3, test.AktifAdim);
        Assert.Equal(new[] { 0, 1 }, test.Servis.KullanilanSurumler);
    }

    [Fact]
    public async Task KayitHatasi_AdimiVeGirdileriKorur_SonrakiKayittaAyniSurumuKullanir()
    {
        await using var test = await Ekran.AcAsync();
        await test.GitAsync(2);
        test.Cevaplar.Koy("bina_insa_yili", "2001");
        test.Servis.KayitHatasi = true;
        await test.CagirAsync("DevamEtAsync");
        Assert.Equal(2, test.AktifAdim);
        Assert.True(test.Kaydedilmemis);
        Assert.Equal("2001", test.Cevaplar.Al("bina_insa_yili"));
        Assert.Contains("Kayıt çakışması", test.Al<string>("_hata"));
        test.Servis.KayitHatasi = false;
        await test.CagirAsync("DevamEtAsync");
        Assert.Equal(3, test.AktifAdim);
        Assert.Equal(new[] { 0, 0 }, test.Servis.KullanilanSurumler);
        Assert.False(test.Kaydedilmemis);
    }

    [Fact]
    public async Task Tamamlama_EksikBolumeDoner_SonraSaltOkunurRaporGosterir()
    {
        await using var test = await Ekran.AcAsync();
        await test.GitAsync(7);
        await test.CagirAsync("TamamlaAsync");
        Assert.Equal(0, test.AktifAdim);
        Assert.Equal(0, test.Servis.TamamlamaSayisi);
        test.Yaz("_imzalayanMakam", "Selin Kaya");
        await test.CagirAsync("TamamlaAsync");
        Assert.Equal(6, test.AktifAdim);
        test.Cevaplar.Koy("kanaat_ozet", "Taşınmaz iyi durumdadır.");
        await test.CagirAsync("TamamlaAsync");
        Assert.Equal(1, test.Servis.TamamlamaSayisi);
        Assert.Equal(KontrolDurumu.Tamamlandi, test.Servis.Kayit.Durum);
        Assert.Equal(7, test.AktifAdim);
        Assert.False(test.Kaydedilmemis);
        await test.GitAsync(6);
        var html = await test.HtmlAsync();
        Assert.Contains("disabled", html);
        Assert.Contains("Revize et", html);
        Assert.DoesNotContain("Taslağı kaydet", System.Net.WebUtility.HtmlDecode(html));
    }

    [Fact]
    public async Task GuncellenenTaslak_YenidenAcildigindaTumAlanlarKorunur()
    {
        var servis = new KontrolTestServisi();
        await using (var ilk = await Ekran.AcAsync(servis))
        {
            ilk.Cevaplar.Koy("kanaat_ozet", "Kayıtlı açıklama");
            ilk.Cevaplar.GrupSatirlari("emsaller").Add(new() { ["emsal_bedel"] = "2500000" });
            ilk.Yaz("_imzalayanMakam", "Selin Kaya");
            await ilk.CagirAsync("TaslakKaydetAsync");
        }
        await using var ikinci = await Ekran.AcAsync(servis);
        Assert.Equal("Kayıtlı açıklama", ikinci.Cevaplar.Al("kanaat_ozet"));
        Assert.Equal("2500000", ikinci.Cevaplar.Gruplar["emsaller"][0]["emsal_bedel"]);
        Assert.Equal("Selin Kaya", ikinci.Al<string>("_imzalayanMakam"));
        Assert.False(ikinci.Kaydedilmemis);
    }

    [Theory]
    [InlineData(11, 100, "en fazla 10 dosya")]
    [InlineData(1, 21 * 1024 * 1024, "20 MB")]
    public async Task GecersizDosyaSecimi_IslemDurumundaKalmaz(int adet, long boyut, string hata)
    {
        await using var test = await Ekran.AcAsync();
        var dosyalar = Enumerable.Range(0, adet).Select(_ => (IBrowserFile)new TestDosya(boyut)).ToArray();
        await test.CagirAsync("EkYukleAsync", new InputFileChangeEventArgs(dosyalar), ("foto_dis", (string?)null, (int?)null));
        Assert.False(test.Al<bool>("_busy"));
        Assert.Contains(hata, test.Al<string>("_hata"));
    }

    private sealed class TestDosya(long boyut) : IBrowserFile
    {
        public string Name => "test.jpg";
        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public long Size => boyut;
        public string ContentType => "image/jpeg";
        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Geçersiz dosya okunmamalı.");
    }

    private sealed class Ekran : IAsyncDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly TestRenderer _renderer;
        private readonly Aktivator _aktivator;
        private HtmlRootComponent _root;
        public KontrolTestServisi Servis { get; }
        private TasinmazKontrolDetay Sayfa => _aktivator.Sayfa!;
        public KontrolCevaplari Cevaplar => Al<KontrolCevaplari>("_cevaplar");
        public int AktifAdim => Al<int>("_aktifAdim");
        public bool Kaydedilmemis => (bool)typeof(TasinmazKontrolDetay).GetProperty("KaydedilmemisDegisiklikVar", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(Sayfa)!;

        private Ekran(KontrolTestServisi servis)
        {
            Servis = servis;
            _aktivator = new();
            var services = new ServiceCollection().AddLogging();
            services.AddSingleton<IComponentActivator>(_aktivator);
            services.AddSingleton(Fake<ITasinmazKontrolService>.Olustur(servis.Cagir));
            services.AddSingleton(Fake<ICurrentUserService>.Olustur((ad, _) => ad == "HasPermissionAsync" ? Task.FromResult(true) : throw new InvalidOperationException(ad)));
            services.AddSingleton(Fake<IPersonelService>.Olustur((ad, _) => ad == "GetCalisanPersonelAsync" ? Task.FromResult(Result<List<Personel>>.Success(new())) : throw new InvalidOperationException(ad)));
            services.AddSingleton<IToastService, ToastService>();
            services.AddSingleton<IModalService, ModalService>();
            services.AddSingleton<NavigationManager, TestNavigation>();
            services.AddSingleton<IJSRuntime, TestJS>();
            _provider = services.BuildServiceProvider();
            _renderer = new TestRenderer(_provider, _provider.GetRequiredService<ILoggerFactory>());
        }

        public static async Task<Ekran> AcAsync(KontrolTestServisi? servis = null)
        {
            var ekran = new Ekran(servis ?? new());
            ekran._root = await ekran._renderer.Dispatcher.InvokeAsync(async () =>
            {
                var root = ekran._renderer.BeginRenderingComponent(typeof(TasinmazKontrolDetay), ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(TasinmazKontrolDetay.Id)] = 1 }));
                await root.QuiescenceTask;
                return root;
            });
            return ekran;
        }

        public T Al<T>(string ad) => (T)typeof(TasinmazKontrolDetay).GetField(ad, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(Sayfa)!;
        public void Yaz(string ad, object? deger) => typeof(TasinmazKontrolDetay).GetField(ad, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(Sayfa, deger);
        public Task GitAsync(int adim) => CagirAsync("AdimaGitAsync", adim);
        public Task CagirAsync(string ad, params object[] args) => _renderer.Dispatcher.InvokeAsync(async () =>
        {
            var task = (Task)typeof(TasinmazKontrolDetay).GetMethod(ad, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(Sayfa, args)!;
            await task;
            typeof(TasinmazKontrolDetay).GetMethod("FormDegisti", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(Sayfa, null);
        });
        public Task<string> HtmlAsync() => _renderer.Dispatcher.InvokeAsync(() => System.Net.WebUtility.HtmlDecode(_root.ToHtmlString()));
        public async ValueTask DisposeAsync() { await _renderer.DisposeAsync(); await _provider.DisposeAsync(); }
    }

    // Etkileşimli sayfayı HTTP sunucusu açmadan render eder; JS yaşam döngüsünü çalıştırmaz.
#pragma warning disable BL0006 // Yalnızca testlerde render-mode çözümlemesini değiştirmek için kullanılır.
    private sealed class TestRenderer(IServiceProvider services, ILoggerFactory loggerFactory) : StaticHtmlRenderer(services, loggerFactory)
    {
        protected override IComponent ResolveComponentForRenderMode(Type componentType, int? parentComponentId,
            IComponentActivator componentActivator, IComponentRenderMode renderMode) => componentActivator.CreateInstance(componentType);
    }
#pragma warning restore BL0006

    private sealed class Aktivator : IComponentActivator
    {
        public TasinmazKontrolDetay? Sayfa;
        public IComponent CreateInstance(Type componentType)
        {
            var instance = (IComponent)Activator.CreateInstance(componentType)!;
            if (instance is TasinmazKontrolDetay sayfa) Sayfa = sayfa;
            return instance;
        }
    }

    private sealed class TestNavigation : NavigationManager
    {
        public TestNavigation() => Initialize("http://localhost/", "http://localhost/tbys/tasinmaz-kontrol/1");
        protected override void SetNavigationLockState(bool value) { }
    }
    private sealed class TestJS : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult(default(TValue)!);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }
    public class Fake<T> : DispatchProxy where T : class
    {
        private Func<string, object?[]?, object?> _cagir = null!;
        public static T Olustur(Func<string, object?[]?, object?> cagir)
        {
            var proxy = Create<T, Fake<T>>();
            ((Fake<T>)(object)proxy)._cagir = cagir;
            return proxy;
        }
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => _cagir(targetMethod!.Name, args);
    }
    private sealed class KontrolTestServisi
    {
        public readonly TasinmazKontrol Kayit = new()
        {
            Id = 1, TasinmazId = 1, KontrolTarihi = DateTime.Today, BolgeAdi = "Ankara"
        };
        public bool KayitHatasi;
        public int TamamlamaSayisi;
        public List<int> KullanilanSurumler = new();
        public object Cagir(string ad, object?[]? args)
        {
            switch (ad)
            {
                case "GetByIdAsync": return Task.FromResult(Result<TasinmazKontrol>.Success(JsonSerializer.Deserialize<TasinmazKontrol>(JsonSerializer.Serialize(Kayit))!));
                case "GetEklerAsync": return Task.FromResult(Result<List<TasinmazKontrolEk>>.Success(new()));
                case "GetBolgePersoneliAsync": return Task.FromResult(Result<List<BolgePersoneli>>.Success(new()));
                case "GetPlanAsync": return Task.FromResult(Result<TasinmazKontrolPlani?>.Success(null));
                case "TaslakKaydetAsync":
                    var surum = (int)args![1]!;
                    KullanilanSurumler.Add(surum);
                    if (KayitHatasi || surum != Kayit.KayitSurumu) return Task.FromResult(Result.Failure(Error.Conflict("Kayıt çakışması")));
                    Kayit.CevaplarJson = ((KontrolCevaplari)args[2]!).ToJson();
                    Kayit.KontrolEdenler = (string?)args[3]; Kayit.KontrolTarihi = (DateTime)args[4]!; Kayit.ImzalayanMakam = (string?)args[5]; Kayit.KayitSurumu++;
                    return Task.FromResult(Result.Success());
                case "TamamlaAsync":
                    var istek = (KontrolTamamlaIstegi)args![0]!;
                    Assert.Equal(Kayit.KayitSurumu, istek.KayitSurumu);
                    TamamlamaSayisi++;
                    Kayit.Durum = KontrolDurumu.Tamamlandi; Kayit.CevaplarJson = istek.Cevaplar.ToJson(); Kayit.ImzalayanMakam = istek.ImzalayanMakam;
                    return Task.FromResult(Result.Success());
                default: throw new InvalidOperationException(ad);
            }
        }
    }
}
