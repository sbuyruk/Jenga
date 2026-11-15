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
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class TaskApprovalEntryPageTests
{
    [Fact]
    public async Task SaveRequiresConfirmationAndMailWarningPreventsDuplicateCreation()
    {
        await using var screen = new Screen(warning: true);
        await screen.RenderAsync();
        await screen.CallAsync("SaveAsync");
        Assert.Equal(0, screen.Saves);
        Assert.Contains("Görev Kaydedilecek", await screen.HtmlAsync());
        await screen.CallAsync("ConfirmAsync");
        Assert.Equal(1, screen.Saves);
        Assert.Equal(91, screen.Model.Id);
        Assert.Contains("bildirim gönderilemedi", await screen.HtmlAsync());
        Assert.False(screen.Get<bool>("_showSave"));
        await screen.CallAsync("SaveAsync");
        await screen.CallAsync("ConfirmAsync");
        Assert.Equal(1, screen.Saves);
    }

    [Fact]
    public async Task DeleteRequiresConfirmationAndCancellationDoesNotCallService()
    {
        await using var screen = new Screen();
        await screen.RenderAsync();
        screen.Model.Id = 91;
        screen.Set("_showSave", false); screen.Set("_showUpdate", true); screen.Set("_showDelete", true);
        await screen.CallAsync("DeleteAsync");
        Assert.Equal(0, screen.Deletes);
        Assert.Contains("Görev Onayı Silinecek", await screen.HtmlAsync());
        screen.Set("_confirmationAction", null);
        await screen.CallAsync("ConfirmAsync");
        Assert.Equal(0, screen.Deletes);
        await screen.CallAsync("DeleteAsync");
        await screen.CallAsync("ConfirmAsync");
        Assert.Equal(1, screen.Deletes);
        Assert.EndsWith("/ikys/task-approvals/personal", screen.Navigation.Uri);
    }

    [Fact]
    public async Task MissingRateBlocksSaveBeforeConfirmation()
    {
        await using var screen = new Screen(validCalculation: false);
        await screen.RenderAsync();
        await screen.CallAsync("SaveAsync");
        Assert.Equal(0, screen.Saves);
        Assert.Null(screen.Get<string?>("_confirmationAction"));
        Assert.Contains("Harcırah tanımı bulunamadı", await screen.HtmlAsync());
    }

    private sealed class Screen : IAsyncDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly TestRenderer _renderer;
        private Microsoft.AspNetCore.Components.Web.HtmlRendering.HtmlRootComponent _root;
        private readonly Capture _capture = new();
        public Navigation Navigation { get; } = new();
        public int Saves, Deletes;
        private TaskApprovalEntryPage Page => _capture.Page!;
        public GorevOnay Model => Get<GorevOnay>("_model");

        public Screen(bool warning = false, bool validCalculation = true)
        {
            var services = new ServiceCollection().AddLogging();
            services.AddSingleton<NavigationManager>(Navigation);
            services.AddSingleton<IComponentActivator>(_capture);
            services.AddSingleton(Fake<ICurrentUserService>((method, _) => method == "HasPermissionAsync" ? Task.FromResult(true) : throw new InvalidOperationException(method)));
            services.AddSingleton(Fake<IGorevOnayService>((method, _) => throw new InvalidOperationException(method)));
            services.AddSingleton(Fake<ITaskApprovalEntryService>((method, args) =>
            {
                if (method == "CalculateAsync") return Task.FromResult(new TaskApprovalCalculationResult
                {
                    IsValid = validCalculation, ErrorMessage = validCalculation ? null : "Harcırah tanımı bulunamadı",
                    StartDateTime = DateTime.Today.AddHours(8), EndDateTime = DateTime.Today.AddHours(17),
                    EarnedDayText = "0,5 gün", TotalAllowanceText = "150,00", Currency = "TL", DailyAllowance = 300
                });
                if (method == "SaveAsync")
                {
                    Saves++;
                    return Task.FromResult(Result.Success(new TaskApprovalEntrySaveResult { Id = 91, Warning = warning ? "Görev kaydedildi ancak bildirim gönderilemedi." : null }));
                }
                if (method == "DeleteAsync") { Deletes++; return Task.FromResult(Result.Success()); }
                throw new InvalidOperationException(method);
            }));
            _provider = services.BuildServiceProvider();
            _renderer = new TestRenderer(_provider, _provider.GetRequiredService<ILoggerFactory>());
        }
        public async Task RenderAsync() => await _renderer.Dispatcher.InvokeAsync(async () =>
        {
            _root = _renderer.BeginRenderingComponent(typeof(SeededPage), ParameterView.Empty);
            await _root.QuiescenceTask;
        });
        public async Task CallAsync(string name) => await _renderer.Dispatcher.InvokeAsync(async () =>
        {
            await (Task)typeof(TaskApprovalEntryPage).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(Page, null)!;
            typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(Page, null);
        });
        public Task<string> HtmlAsync() => _renderer.Dispatcher.InvokeAsync(() => WebUtility.HtmlDecode(_root.ToHtmlString()));
        public T Get<T>(string name) => (T)typeof(TaskApprovalEntryPage).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(Page)!;
        public void Set(string name, object? value) => typeof(TaskApprovalEntryPage).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(Page, value);
        public async ValueTask DisposeAsync() { await _renderer.DisposeAsync(); await _provider.DisposeAsync(); }
    }
    public sealed class SeededPage : TaskApprovalEntryPage
    {
        protected override void OnInitialized()
        {
            var fields = new Dictionary<string, object?>
            {
                ["_isLoading"] = false, ["_showSave"] = true, ["_canCreate"] = true,
                ["_canEdit"] = true, ["_canDelete"] = true, ["_calculationValid"] = true,
                ["_currentPersonel"] = new Personel { Id = 1 },
                ["_model"] = new GorevOnay { PersonelId = 1, GorevinSebebi = "Görev", GorevinYeri = "Ankara", BaslangicTarihi = DateTime.Today.AddHours(8), BitisTarihi = DateTime.Today.AddHours(17) }
            };
            foreach (var (name, value) in fields) typeof(TaskApprovalEntryPage).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(this, value);
        }
    }
    private sealed class Capture : IComponentActivator
    {
        public TaskApprovalEntryPage? Page;
        public IComponent CreateInstance(Type type)
        {
            var instance = (IComponent)Activator.CreateInstance(type)!;
            if (instance is TaskApprovalEntryPage page) Page = page;
            return instance;
        }
    }
    private sealed class Navigation : NavigationManager
    {
        public Navigation() => Initialize("http://localhost/", "http://localhost/ikys/task-approval-entry");
        protected override void NavigateToCore(string uri, bool forceLoad) { Uri = ToAbsoluteUri(uri).AbsoluteUri; NotifyLocationChanged(false); }
    }
#pragma warning disable BL0006
    private sealed class TestRenderer(IServiceProvider services, ILoggerFactory loggerFactory) : StaticHtmlRenderer(services, loggerFactory)
    {
        protected override IComponent ResolveComponentForRenderMode(Type componentType, int? parentComponentId, IComponentActivator activator, IComponentRenderMode mode) => activator.CreateInstance(componentType);
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
