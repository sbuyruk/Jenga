using System.Net;
using System.Reflection;
using Jenga.BlazorUI.Components.Common;
using Jenga.BlazorUI.Components.Pages.IKYS;
using Jenga.BlazorUI.Services.Common;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class TaskApprovalListsTests
{
    [Fact]
    public async Task NavigationBetweenListsReloadsScopeOnTheSameComponentAndClearsSelections()
    {
        var navigation = new Navigation("/ikys/task-approvals");
        var scopes = new List<bool>();
        var capture = new Capture();
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<NavigationManager>(navigation);
        services.AddSingleton<IComponentActivator>(capture);
        services.AddSingleton(Fake<ICurrentUserService>((method, _) => method switch
        {
            "HasPermissionAsync" => Task.FromResult(true),
            "GetCurrentPersonelAsync" => Task.FromResult<Personel?>(new() { Id = 2 }),
            _ => throw new InvalidOperationException(method)
        }));
        services.AddSingleton(Fake<IGorevOnayService>((method, args) =>
        {
            if (method != "GetTaskApprovalListAsync") throw new InvalidOperationException(method);
            var general = (bool)args![1]!;
            scopes.Add(general);
            return Task.FromResult(Result.Success(new List<TaskApprovalListItem>
            {
                new() { TaskApprovalId = general ? 10 : 20, FullName = general ? "Başka Personel" : "Kendi Görevim", ManagerApprovalValue = 1 }
            }));
        }));
        services.AddSingleton(Fake<IJSRuntime>((method, _) => throw new InvalidOperationException(method)));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new TestRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var root = renderer.BeginRenderingComponent(typeof(TaskApprovalListPage), ParameterView.Empty);
            await root.QuiescenceTask;
            var page = capture.Page!;
            Set(page, "_initialized", true);
            await (Task)typeof(TaskApprovalListPage).GetMethod("LoadAsync", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(page, null)!;
            Assert.Contains("Başka Personel", WebUtility.HtmlDecode(root.ToHtmlString()));
            var selected = (HashSet<int>)typeof(TaskApprovalListPage).GetField("_selectedIds", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(page)!;
            selected.Add(10);
            foreach (var path in new[] { "/ikys/task-approvals/personal", "/ikys/task-approvals/personel" })
            {
                navigation.NavigateTo(path);
                var html = WebUtility.HtmlDecode(root.ToHtmlString());
                Assert.Contains("Kişisel Görev Onay Listesi", html);
                Assert.Contains("Kendi Görevim", html);
                Assert.DoesNotContain("Başka Personel", html);
                Assert.DoesNotContain("Ödendi Yap", html);
                Assert.Empty(selected);
            }
            navigation.NavigateTo("/ikys/task-approvals");
            Assert.Contains("Başka Personel", WebUtility.HtmlDecode(root.ToHtmlString()));
            Assert.Equal(new[] { true, false, false, true }, scopes);
            Assert.Same(page, capture.Page);
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PersonalAndGeneralRoutesRenderDifferentActions(bool personal)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<NavigationManager>(new Navigation(personal ? "/ikys/task-approvals/personal" : "/ikys/task-approvals"));
        services.AddSingleton(Fake<ICurrentUserService>((method, _) => method == "HasPermissionAsync" ? Task.FromResult(true) : throw new InvalidOperationException(method)));
        services.AddSingleton(Fake<IGorevOnayService>((method, _) => throw new InvalidOperationException(method)));
        services.AddSingleton(Fake<IJSRuntime>((method, _) => throw new InvalidOperationException(method)));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new TestRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var root = renderer.BeginRenderingComponent(typeof(SeededPage), ParameterView.Empty);
            await root.QuiescenceTask;
            var raw = root.ToHtmlString();
            Assert.DoesNotContain("<script>zararli</script>", raw);
            return WebUtility.HtmlDecode(raw);
        });
        Assert.Contains("Ödendi", html);
        Assert.Contains("Ret gerekçesi", html);
        if (personal)
        {
            Assert.Contains("Kişisel Görev Onay Listesi", html);
            Assert.DoesNotContain("type=\"checkbox\"", html);
            Assert.DoesNotContain("Ödendi Yap", html);
            Assert.DoesNotContain(">Rapor Al</button>", html);
            Assert.Equal(1, html.Split(">Düzenle</button>").Length - 1);
        }
        else
        {
            Assert.Contains("Seçilen Görevler İçin Rapor Al", html);
            Assert.Contains("Ödendi Yap", html);
            Assert.Equal(3, html.Split("type=\"checkbox\"").Length - 1);
            Assert.Equal(2, html.Split(">Rapor Al</button>").Length - 1);
            Assert.Equal(3, html.Split(">Düzenle</button>").Length - 1);
        }
    }

    [Fact]
    public void SelectAllUsesOnlyEligibleRowsOnTheFilteredCurrentPage()
    {
        var page = new TaskApprovalListPage();
        typeof(TaskApprovalListPage).GetProperty("NavigationManager", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(page, new Navigation("/ikys/task-approvals"));
        var items = Enumerable.Range(1, 12).Select(i => new TaskApprovalListItem
        {
            TaskApprovalId = i, FullName = $"Personel {i}", ManagerApprovalValue = i == 1 ? 0 : 1
        }).ToList();
        var table = new GenericTable<TaskApprovalListItem>();
        ParameterView.FromDictionary(new Dictionary<string, object?> { ["Items"] = items, ["PageSize"] = 5 }).SetParameterProperties(table);
        typeof(GenericTable<TaskApprovalListItem>).GetMethod("OnParametersSet", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(table, null);
        Set(page, "_items", items);
        Set(page, "_table", table);
        var toggle = typeof(TaskApprovalListPage).GetMethod("ToggleAllSelection", BindingFlags.NonPublic | BindingFlags.Instance)!;
        toggle.Invoke(page, [new ChangeEventArgs { Value = true }]);
        var selected = (HashSet<int>)typeof(TaskApprovalListPage).GetField("_selectedIds", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(page)!;
        Assert.Equal(new[] { 2, 3, 4, 5 }, selected.Order());
        typeof(GenericTable<TaskApprovalListItem>).GetProperty("SearchText", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(table, "Personel 12");
        toggle.Invoke(page, [new ChangeEventArgs { Value = true }]);
        Assert.Equal(new[] { 2, 3, 4, 5, 12 }, selected.Order());
        toggle.Invoke(page, [new ChangeEventArgs { Value = false }]);
        Assert.Equal(new[] { 2, 3, 4, 5 }, selected.Order());
    }

    public sealed class SeededPage : TaskApprovalListPage
    {
        protected override void OnInitialized()
        {
            Set(this, "_isLoading", false);
            Set(this, "_canManage", true);
            Set(this, "_canEdit", true);
            var now = DateTime.Now;
            Set(this, "_items", new List<TaskApprovalListItem>
            {
                new() { TaskApprovalId = 1, FullName = "Bekleyen", Reason = "<script>zararli</script>", PaymentStatus = false, ManagerApprovalValue = 0, EndDate = now },
                new() { TaskApprovalId = 2, FullName = "Onaylı", PaymentStatus = false, ManagerApprovalValue = 1, EndDate = now },
                new() { TaskApprovalId = 3, FullName = "Ödenmiş", PaymentStatus = true, IsPaid = true, ManagerApprovalValue = 3, EndDate = now },
                new() { TaskApprovalId = 4, FullName = "Reddedilmiş", PaymentStatus = false, ManagerApprovalValue = 2, RejectionNote = "Ret gerekçesi", EndDate = now },
                new() { TaskApprovalId = 5, FullName = "Süresi geçmiş", PaymentStatus = false, ManagerApprovalValue = 3, EndDate = now.AddDays(-15) }
            });
        }
    }

    private static void Set(object target, string field, object value)
        => typeof(TaskApprovalListPage).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(target, value);
    private sealed class Navigation : NavigationManager
    {
        public Navigation(string path) => Initialize("http://localhost/", "http://localhost" + path);
        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            Uri = ToAbsoluteUri(uri).AbsoluteUri;
            NotifyLocationChanged(false);
        }
    }
    private sealed class Capture : IComponentActivator
    {
        public TaskApprovalListPage? Page;
        public IComponent CreateInstance(Type type)
        {
            var component = (IComponent)Activator.CreateInstance(type)!;
            if (component is TaskApprovalListPage page) Page = page;
            return component;
        }
    }
#pragma warning disable BL0006
    private sealed class TestRenderer(IServiceProvider services, ILoggerFactory loggerFactory) : StaticHtmlRenderer(services, loggerFactory)
    {
        protected override IComponent ResolveComponentForRenderMode(Type componentType, int? parentComponentId,
            IComponentActivator componentActivator, IComponentRenderMode renderMode) => componentActivator.CreateInstance(componentType);
    }
#pragma warning restore BL0006
    private static T Fake<T>(Func<string, object?[]?, object?> invoke) where T : class
    {
        var proxy = DispatchProxy.Create<T, Stub>();
        ((Stub)(object)proxy).Call = invoke;
        return proxy;
    }
    public class Stub : DispatchProxy
    {
        public Func<string, object?[]?, object?> Call { get; set; } = null!;
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Call(targetMethod!.Name, args);
    }
}
