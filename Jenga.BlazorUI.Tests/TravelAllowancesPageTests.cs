using System.Net;
using System.Reflection;
using Jenga.BlazorUI.Components.Pages.IKYS;
using Jenga.BlazorUI.Services.Common;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.IKYS;
using Jenga.Utility.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class TravelAllowancesPageTests
{
    [Fact]
    public async Task PageRendersEditableAmountDescriptionAndExportsSavedValuesInHeaderOrder()
    {
        await using var screen = new Screen(); await screen.RenderAsync();
        var html = await screen.HtmlAsync();
        Assert.Contains("Ülke Filtresi", html); Assert.Contains("Tarih Aralığı", html);
        Assert.Contains("Güncelle", html); Assert.Contains("Yeni Tarih Aralığı Ekle", html);
        Assert.DoesNotContain(">Sil</button>", html);
        Assert.DoesNotContain("<script>zararli</script>", await screen.RawHtmlAsync());
        var row = Assert.Single(screen.Rows);
        row.Amount = 999; row.Description = "Kaydedilmedi";
        var values = (IReadOnlyList<object?>)typeof(TravelAllowancesPage).GetMethod("ExportRow", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [row])!;
        Assert.Equal(new object?[] { 1, "Kadro 1", "Türkiye", "TL", 100m, "01.01.2026", null, "<script>zararli</script>" }, values);
        var preview = Environment.GetEnvironmentVariable("JENGA_ALLOWANCES_PREVIEW_PATH");
        if (!string.IsNullOrEmpty(preview)) await File.WriteAllTextAsync(preview, await screen.RawHtmlAsync());
    }

    [Fact]
    public async Task CreateRequiresConfirmationAndCancellationDoesNotCreateSeries()
    {
        await using var screen = new Screen(); await screen.RenderAsync();
        await screen.CallAsync("RequestCreate");
        Assert.Equal(0, screen.Creates);
        Assert.Contains("Evet, Oluştur", await screen.HtmlAsync());
        await screen.CallAsync("CancelConfirmation"); await screen.CallAsync("ConfirmAsync");
        Assert.Equal(0, screen.Creates);
        await screen.CallAsync("RequestCreate"); await screen.CallAsync("ConfirmAsync");
        Assert.Equal(1, screen.Creates);
        Assert.Equal(3, screen.Get<int?>("_seriesId"));
        Assert.Contains("1 kayıt eklendi", await screen.HtmlAsync());
        await screen.CallAsync("ConfirmAsync"); Assert.Equal(1, screen.Creates);
    }

    [Fact]
    public async Task UndoRequiresConfirmationAndCancellationKeepsPeriod()
    {
        await using var screen = new Screen(); await screen.RenderAsync();
        screen.Set("_data", Data(canUndo: true));
        await screen.CallAsync("RequestUndo");
        Assert.Contains("Evet, Geri Al", await screen.HtmlAsync());
        Assert.Equal(0, screen.Undos);
        await screen.CallAsync("CancelConfirmation"); await screen.CallAsync("ConfirmAsync");
        Assert.Equal(0, screen.Undos);
        await screen.CallAsync("RequestUndo"); await screen.CallAsync("ConfirmAsync");
        Assert.Equal(1, screen.Undos);
        Assert.Equal(new HarcirahUndoSeriesRequest("Türkiye", 2, "version-1"), screen.Undo);
        Assert.Equal(1, screen.Get<int?>("_seriesId"));
        Assert.Contains("Önceki dönem yeniden geçerli", await screen.HtmlAsync());
        await screen.CallAsync("ConfirmAsync"); Assert.Equal(1, screen.Undos);
    }

    [Fact]
    public async Task UnavailablePeriodCannotOpenUndoConfirmation()
    {
        await using var screen = new Screen(); await screen.RenderAsync();
        await screen.CallAsync("RequestUndo"); await screen.CallAsync("ConfirmAsync");
        Assert.False(screen.Get<bool>("_confirmUndo"));
        Assert.Equal(0, screen.Undos);
        Assert.Contains("Başlamış dönem geri alınamaz", await screen.HtmlAsync());
    }

    [Fact]
    public async Task UpdateSendsOriginalValuesAndCountryChangeClearsOldSeries()
    {
        await using var screen = new Screen(); await screen.RenderAsync();
        var row = screen.Rows[0]; row.Amount = 200; row.Description = "Güncel";
        await screen.CallAsync("UpdateRowAsync", row);
        Assert.Equal(new HarcirahUpdateRequest(1, 200, "Güncel", 100, "<script>zararli</script>"), screen.Update);
        screen.Set("_country", "Avrupa");
        await screen.CallAsync("CountryChangedAsync");
        Assert.Equal(("Avrupa", (int?)null), screen.LastFilter);
        Assert.Equal(5, screen.Get<int?>("_seriesId"));
        Assert.Equal("Avrupa", screen.Rows[0].Original.Ulke);
    }

    [Fact]
    public async Task InvalidStartDateDoesNotOpenConfirmation()
    {
        await using var screen = new Screen(); await screen.RenderAsync();
        screen.Set("_newStart", DateTime.MinValue);
        await screen.CallAsync("RequestCreate");
        Assert.False(screen.Get<bool>("_confirmCreate")); Assert.Equal(0, screen.Creates);
        Assert.Contains("Yeni başlangıç tarihi", await screen.HtmlAsync());
    }

    private static HarcirahPageData Data(string country = "Türkiye", int series = 2, decimal amount = 100, string? description = "<script>zararli</script>", bool empty = false, bool canUndo = false) => new()
    {
        Country = country, Countries = empty ? [] : ["Türkiye", "Avrupa"], SelectedSeriesId = empty ? null : series,
        Series = empty ? [] : [new(series, new(2026, 1, 1), null)],
        Undo = new(canUndo, canUndo ? "Dönem geri alınabilir." : "Başlamış dönem geri alınamaz.", canUndo ? 1 : null, canUndo ? "version-1" : ""),
        Rows = empty ? [] : [new() { Id = 1, Ulke = country, SeriId = series, KadroGrupId = 1, KadroGrubu = "Kadro 1", Sira = 1, ParaBirimi = country == "Türkiye" ? "TL" : "Euro", Miktar = amount, Aciklama = description, BaslangicTarihi = new(2026, 1, 1) }]
    };

    private sealed class Screen : IAsyncDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly Renderer _renderer;
        private readonly Capture _capture = new();
        private Microsoft.AspNetCore.Components.Web.HtmlRendering.HtmlRootComponent _root;
        private int _series = 2;
        private decimal _amount = 100;
        private string? _description = "<script>zararli</script>";
        public int Creates, Undos;
        public HarcirahUndoSeriesRequest? Undo;
        public HarcirahUpdateRequest? Update;
        public (string?, int?) LastFilter;
        public List<TravelAllowancesPage.EditRow> Rows => Get<List<TravelAllowancesPage.EditRow>>("_rows");
        public Screen()
        {
            var services = new ServiceCollection().AddLogging();
            services.AddSingleton<IComponentActivator>(_capture);
            services.AddSingleton<NavigationManager>(new Navigation());
            services.AddSingleton(Fake<IJSRuntime>((method, _) => throw new InvalidOperationException(method)));
            services.AddSingleton(Fake<ICurrentUserService>((method, _) => method == "HasPermissionAsync" ? Task.FromResult(true) : throw new InvalidOperationException(method)));
            services.AddSingleton(Fake<IHarcirahService>((method, args) =>
            {
                if (method == "GetAsync")
                {
                    var country = (string?)args![0] ?? "Türkiye";
                    LastFilter = (country, (int?)args[1]);
                    return Task.FromResult(Result.Success(Data(country, country == "Avrupa" ? 5 : _series, _amount, _description)));
                }
                if (method == "CreateSeriesAsync") { Creates++; _series = 3; _amount = 0; return Task.FromResult(Result.Success(new HarcirahCreateSeriesResult(3, 1))); }
                if (method == "UndoSeriesAsync") { Undos++; Undo = (HarcirahUndoSeriesRequest)args![0]!; _series = 1; return Task.FromResult(Result.Success(new HarcirahUndoSeriesResult(1, 1))); }
                if (method == "UpdateAsync") { Update = (HarcirahUpdateRequest)args![0]!; _amount = Update.Amount; _description = Update.Description; return Task.FromResult(Result.Success()); }
                throw new InvalidOperationException(method);
            }));
            _provider = services.BuildServiceProvider();
            _renderer = new Renderer(_provider, _provider.GetRequiredService<ILoggerFactory>());
        }
        public async Task RenderAsync() => await _renderer.Dispatcher.InvokeAsync(async () =>
        {
            _root = _renderer.BeginRenderingComponent(typeof(SeededPage), ParameterView.Empty); await _root.QuiescenceTask;
        });
        public async Task CallAsync(string name, params object?[] args) => await _renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = typeof(TravelAllowancesPage).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(_capture.Page, args);
            if (result is Task task) await task;
            typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(_capture.Page, null);
        });
        public Task<string> RawHtmlAsync() => _renderer.Dispatcher.InvokeAsync(() => _root.ToHtmlString());
        public async Task<string> HtmlAsync() => WebUtility.HtmlDecode(await RawHtmlAsync());
        public T Get<T>(string name) => (T)typeof(TravelAllowancesPage).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(_capture.Page)!;
        public void Set(string name, object? value) => typeof(TravelAllowancesPage).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(_capture.Page, value);
        public async ValueTask DisposeAsync() { await _renderer.DisposeAsync(); await _provider.DisposeAsync(); }
    }
    public sealed class SeededPage : TravelAllowancesPage
    {
        protected override void OnInitialized()
        {
            var data = Data();
            var fields = new Dictionary<string, object?>
            {
                ["_data"] = data, ["_country"] = data.Country, ["_seriesId"] = data.SelectedSeriesId, ["_isLoading"] = false,
                ["_rows"] = data.Rows.Select(x => new EditRow(x)).ToList(), ["_newStart"] = new DateTime(2027, 1, 1)
            };
            foreach (var (name, value) in fields) typeof(TravelAllowancesPage).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(this, value);
        }
    }
    private sealed class Navigation : NavigationManager { public Navigation() => Initialize("http://localhost/", "http://localhost/ikys/travel-allowances"); }
    private sealed class Capture : IComponentActivator
    {
        public TravelAllowancesPage? Page;
        public IComponent CreateInstance(Type type)
        {
            var component = (IComponent)Activator.CreateInstance(type)!;
            if (component is TravelAllowancesPage page) Page = page;
            return component;
        }
    }
#pragma warning disable BL0006
    private sealed class Renderer(IServiceProvider services, ILoggerFactory logger) : StaticHtmlRenderer(services, logger)
    {
        protected override IComponent ResolveComponentForRenderMode(Type type, int? parentId, IComponentActivator activator, IComponentRenderMode mode) => activator.CreateInstance(type);
    }
#pragma warning restore BL0006
    private static T Fake<T>(Func<string, object?[]?, object?> call) where T : class
    {
        var proxy = DispatchProxy.Create<T, Stub>(); ((Stub)(object)proxy).Call = call; return proxy;
    }
    public class Stub : DispatchProxy
    {
        public Func<string, object?[]?, object?> Call { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Call(method!.Name, args);
    }
}
