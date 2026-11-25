using System.Net;
using System.Reflection;
using Jenga.BlazorUI.Components.Pages.IKYS;
using Jenga.BlazorUI.Services.Common;
using Jenga.BlazorUI.Services.Common.Toast;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Status = Jenga.Models.IKYS.IzinTalep.OnayDurumuTipi;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class LeaveRequestEntryPageTests
{
    [Theory]
    [InlineData(true, null, null, ToastType.Success, false)]
    [InlineData(true, 42, 42, ToastType.Warning, false)]
    [InlineData(true, 99, null, ToastType.Error, false)]
    [InlineData(false, 42, 17, ToastType.Success, false)]
    [InlineData(false, null, 17, ToastType.Success, true)]
    [InlineData(true, 42, 42, ToastType.Success, true)]
    public async Task EntryUsesManagePermissionAndShowsStandardSaveNotification(
        bool canManage, int? selectedId, int? expectedId, ToastType expectedToast, bool futureOffset)
    {
        var page = new LeaveRequestEntryPage();
        typeof(LeaveRequestEntryPage).GetProperty(nameof(LeaveRequestEntryPage.SelectedPersonelId))!.SetValue(page, selectedId);

        var scopes = new List<bool>();
        var processedPeople = new List<int?>();
        var navigation = new TestNavigation();
        var notifications = new List<(ToastModel Toast, string Path)>();
        var toastService = new ToastService();
        toastService.OnShow += toast => notifications.Add((toast, new Uri(navigation.Uri).AbsolutePath));
        SetService("Navigation", navigation);
        SetService("ToastService", toastService);
        SetService("CurrentUser", Stub<ICurrentUserService>((name, args) =>
        {
            Assert.Equal(nameof(ICurrentUserService.HasPermissionAsync), name);
            Assert.Equal(ModuleName.IKYS, args[0]);
            Assert.Equal(Operation.Manage, args[1]);
            return Task.FromResult(canManage);
        }));
        SetService("Workflow", Stub<ILeaveRequestWorkflow>((name, args) =>
        {
            var office = (bool)args[1]!;
            if (name == nameof(ILeaveRequestWorkflow.ListPendingForEntryAsync))
            {
                Assert.Equal(canManage, office);
                Assert.Equal(expectedId, ((LeaveRequestInput)args[0]!).PersonelId);
                Assert.Equal(LeaveType.Paid, ((LeaveRequestInput)args[0]!).Type);
                return Task.FromResult(Result.Success(new List<LeaveRequestRow>()));
            }
            scopes.Add(office);
            if (name == nameof(ILeaveRequestWorkflow.LoadFormAsync))
            {
                List<LeaveOption> people = office ? [new(17, "Talep Sahibi"), new(42, "Diğer Personel")] : [new(17, "Talep Sahibi")];
                return Task.FromResult(Result.Success(new LeaveRequestForm(
                    new LeaveRequestInput { PersonelId = office ? null : 17, Type = LeaveType.Paid }, people, [], [], [])));
            }
            if (name is nameof(ILeaveRequestWorkflow.CalculateAsync) or nameof(ILeaveRequestWorkflow.SaveAsync))
                processedPeople.Add(((LeaveRequestInput)args[0]!).PersonelId);
            if (name == nameof(ILeaveRequestWorkflow.SaveAsync))
                Assert.Equal(futureOffset, ((LeaveRequestInput)args[0]!).RequestFuturePeriodOffset);
            return name switch
            {
                nameof(ILeaveRequestWorkflow.CalculateAsync) => Task.FromResult(Result.Success(new LeaveCalculation("1", "Gün", null, null,
                    CanRequestFuturePeriodOffset: futureOffset))),
                nameof(ILeaveRequestWorkflow.SaveAsync) => Task.FromResult(expectedToast == ToastType.Error
                    ? Result.Failure<LeaveWriteResult>(Error.Validation("Personel seçiniz."))
                    : Result.Success(new LeaveWriteResult(1, NotificationFailed: expectedToast == ToastType.Warning))),
                _ => throw new InvalidOperationException(name)
            };
        }));

        await CallAsync("LoadAsync");
        await CallAsync("CalculateAsync");
        await CallAsync("SaveAsync");

        Assert.Equal(new[] { canManage, canManage, canManage }, scopes);
        Assert.Equal(new[] { expectedId, expectedId }, processedPeople);
        var (notification, path) = Assert.Single(notifications);
        Assert.Equal(expectedToast == ToastType.Error ? "/ikys/leave-request-entry" :
            futureOffset && canManage ? "/ikys/leave-requests/manage" : "/ikys/leave-requests", path);
        if (futureOffset) Assert.Equal("?requestId=1", new Uri(navigation.Uri).Query);
        Assert.Equal(expectedToast, notification.Type);
        Assert.Equal(expectedToast switch
        {
            ToastType.Error => "Personel seçiniz.",
            ToastType.Warning => "İzin talebi gönderildi. E-posta bildirimi gönderilemedi.",
            _ => futureOffset ? "Mahsup dilekçesi oluşturuldu." : "İzin talebi gönderildi."
        }, notification.Message);

        void SetService(string name, object service) =>
            typeof(LeaveRequestEntryPage).GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, service);

        Task CallAsync(string name) =>
            (Task)typeof(LeaveRequestEntryPage).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, null)!;
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task DetailQueryOpensOnlyTheRequestedAuthorizedDetail(bool office, bool canViewRequest)
    {
        var page = new LeaveRequestsPage();
        var pageType = typeof(LeaveRequestsPage);
        pageType.GetProperty(nameof(LeaveRequestsPage.Scope))!.SetValue(page, office ? "manage" : null);
        pageType.GetProperty(nameof(LeaveRequestsPage.RequestedDetailId))!.SetValue(page, 71);
        pageType.GetProperty("CurrentUser", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page,
            Stub<ICurrentUserService>((_, args) => Task.FromResult((Operation)args[1]! != Operation.Manage || office)));
        var detailCalls = new List<int>();
        var filters = new List<LeaveListFilter>();
        var detail = new LeaveRequestDetail(new LeaveRequestRow(71, "Personel", "Yıllık izin", 1, null, null, "2", "Gün",
            IzinTalep.OnayDurumuTipi.KontrolEdildi), null, null, null, null, [], [], "/izin-belgesi");
        pageType.GetProperty("Workflow", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page,
            Stub<ILeaveRequestWorkflow>((name, args) =>
            {
                if (name == nameof(ILeaveRequestWorkflow.ListAsync))
                {
                    filters.Add((LeaveListFilter)args[1]!);
                    return Task.FromResult(Result.Success(new LeaveRequestList([], 0)));
                }
                Assert.Equal(nameof(ILeaveRequestWorkflow.DetailAsync), name);
                Assert.Equal(office, args[1]);
                detailCalls.Add((int)args[0]!);
                return Task.FromResult(canViewRequest ? Result.Success(detail)
                    : Result.Failure<LeaveRequestDetail>(Error.NotFound("İzin talebi bulunamadı.")));
            }));

        await (Task)pageType.GetMethod("InitializeAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, null)!;

        Assert.Equal(71, Assert.Single(detailCalls));
        Assert.Equal(canViewRequest ? detail : null, pageType.GetField("_detail", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page));
        await (Task)pageType.GetMethod("SetGroupAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, [LeaveListFilter.Personal])!;
        await (Task)pageType.GetMethod("ClearFiltersAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, null)!;
        var expectedDefault = office ? LeaveListFilter.General : LeaveListFilter.All;
        Assert.Equal(new[] { expectedDefault, LeaveListFilter.Personal, expectedDefault }, filters);
    }

    [Theory]
    [InlineData(true, Status.IslemBekliyor, "Kontrol/Red", "Kabul Et")]
    [InlineData(true, Status.DilekceBekleniyor, "Kontrol/Red", "Kabul Et")]
    [InlineData(true, Status.KontrolEdildi, "Kayıt/Red", "Onayla")]
    [InlineData(true, Status.KayitlaraIslendi, "Belge ve Detay", null)]
    [InlineData(true, Status.Reddedildi, "İncele", null)]
    [InlineData(true, Status.IptalEdildi, "İncele", null)]
    [InlineData(false, Status.KontrolEdildi, "Belge ve Detay", null)]
    public async Task SelectedRowOpensTheDecisionForItsScopeAndStatus(bool office, Status status, string action, string? decision)
    {
        var row = new LeaveRequestRow(71, "Personel", "Yıllık izin", 1, null, null, "2", "Gün", status);
        var start = new DateTime(2026, 1, 1);
        var period = new IzinDonem { Id = 1, IzinTipi = (int)LeaveType.Paid, BaslangicTarihi = start, BitisTarihi = start.AddYears(1).AddDays(-1) };
        var next = start.AddYears(1);
        var detail = new LeaveRequestDetail(row, "Ankara", "Mevcut açıklama", null, null, [period],
            [new LeaveMovementRow(1, "Yıllık izin", (int)LeaveType.Paid, next, next, "1", "Gün")], null,
            DecisionPeriod: decision is null ? null : period);
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        services.AddSingleton(Stub<ICurrentUserService>((_, _) => Task.FromResult(true)));
        services.AddSingleton(Stub<IJSRuntime>((method, _) => throw new InvalidOperationException(method)));
        services.AddSingleton(Stub<ILeaveRequestWorkflow>((method, args) => method switch
        {
            nameof(ILeaveRequestWorkflow.ListAsync) => Task.FromResult(Result.Success(new LeaveRequestList([row], 1))),
            nameof(ILeaveRequestWorkflow.DetailAsync) when Equals(args[0], row.Id) && Equals(args[1], office) && Equals(args[3], office)
                => Task.FromResult(Result.Success(detail)),
            _ => throw new InvalidOperationException(method)
        }));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new PageRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var root = renderer.BeginRenderingComponent(typeof(SelectedRowPage), ParameterView.FromDictionary(
                new Dictionary<string, object?> { [nameof(LeaveRequestsPage.Scope)] = office ? "manage" : null }));
            await root.QuiescenceTask;
            return WebUtility.HtmlDecode(root.ToHtmlString());
        });

        Assert.Contains(action, html);
        Assert.Contains("Ankara", html);
        Assert.Equal(decision is not null, html.Contains("<dialog"));
        Assert.Equal(decision is not null, html.Contains("Yeni izin dönemi"));
        Assert.Equal(decision is not null, html.Contains("Güncel Dönemin İzin Hareketleri"));
        Assert.Equal(decision == "Kabul Et", html.Contains(">Kabul Et</button>"));
        Assert.Equal(decision == "Onayla", html.Contains(">Onayla</button>"));
        Assert.Equal(decision is not null, html.Contains(">Reddet</button>"));
        if (decision is not null) Assert.Contains("Mevcut açıklama</textarea>", html);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DecisionClosesAndRefreshesOnSuccessAndKeepsTheNoteOnFailure(bool success)
    {
        var page = new LeaveRequestsPage();
        var pageType = typeof(LeaveRequestsPage);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var row = new LeaveRequestRow(71, "Personel", "Yıllık izin", 1, null, null, "2", "Gün", Status.IslemBekliyor);
        var detail = new LeaveRequestDetail(row, null, "Önceki açıklama", null, null, [], [], null);
        pageType.GetProperty(nameof(LeaveRequestsPage.Scope))!.SetValue(page, "manage");
        var refreshCount = 0;
        pageType.GetProperty("Workflow", flags)!.SetValue(page, Stub<ILeaveRequestWorkflow>((method, args) =>
        {
            if (method == nameof(ILeaveRequestWorkflow.DetailAsync)) return Task.FromResult(Result.Success(detail));
            if (method == nameof(ILeaveRequestWorkflow.ListAsync))
            {
                refreshCount++;
                return Task.FromResult(Result.Success(new LeaveRequestList([], 0)));
            }
            Assert.Equal(nameof(ILeaveRequestWorkflow.DecideAsync), method);
            Assert.Equal(row.Id, args[0]);
            Assert.Equal(LeaveDecision.Reject, args[1]);
            Assert.Equal("Karar açıklaması", args[2]);
            return Task.FromResult(success ? Result.Success(new LeaveWriteResult(row.Id, false)) :
                Result.Failure<LeaveWriteResult>(Error.Conflict("Talebin durumu değişti.")));
        }));

        await (Task)pageType.GetMethod("OpenDetailAsync", flags)!.Invoke(page, [row.Id])!;
        var note = pageType.GetField("_note", flags)!;
        Assert.Equal(detail.Description, note.GetValue(page));
        note.SetValue(page, "Karar açıklaması");
        await (Task)pageType.GetMethod("DecideAsync", flags)!.Invoke(page, [LeaveDecision.Reject])!;

        Assert.Equal(success ? 1 : 0, refreshCount);
        Assert.Equal(success ? null : detail, pageType.GetField("_detail", flags)!.GetValue(page));
        Assert.Equal(success ? null : "Karar açıklaması", note.GetValue(page));
        Assert.Equal(success ? null : "Talebin durumu değişti.", pageType.GetField("_decisionError", flags)!.GetValue(page));
    }

    private sealed class SelectedRowPage : LeaveRequestsPage
    {
        protected override Task OnParametersSetAsync()
        {
            RequestedDetailId = 71;
            return (Task)typeof(LeaveRequestsPage).GetMethod("InitializeAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(this, null)!;
        }
    }

#pragma warning disable BL0006
    private sealed class PageRenderer(IServiceProvider services, ILoggerFactory logger) : StaticHtmlRenderer(services, logger)
    {
        protected override IComponent ResolveComponentForRenderMode(Type componentType, int? parentComponentId,
            IComponentActivator componentActivator, IComponentRenderMode renderMode) => componentActivator.CreateInstance(componentType);
    }
#pragma warning restore BL0006

    private sealed class TestNavigation : NavigationManager
    {
        public TestNavigation() => Initialize("http://localhost/", "http://localhost/ikys/leave-request-entry");

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            Assert.False(forceLoad);
            Uri = ToAbsoluteUri(uri).AbsoluteUri;
            NotifyLocationChanged(false);
        }
    }

    private static T Stub<T>(Func<string, object?[], object> call) where T : class
    {
        var proxy = DispatchProxy.Create<T, ServiceStub>();
        ((ServiceStub)(object)proxy).Call = call;
        return proxy;
    }

    public class ServiceStub : DispatchProxy
    {
        public Func<string, object?[], object> Call { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Call(method!.Name, args ?? []);
    }
}
