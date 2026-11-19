using System.Reflection;
using Jenga.BlazorUI.Components.Pages.IKYS;
using Jenga.BlazorUI.Services.Common;
using Jenga.BlazorUI.Services.Common.Toast;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Results;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class LeaveRequestEntryPageTests
{
    [Theory]
    [InlineData(true, null, null, ToastType.Success)]
    [InlineData(true, 42, 42, ToastType.Warning)]
    [InlineData(true, 99, null, ToastType.Error)]
    [InlineData(false, 42, 17, ToastType.Success)]
    public async Task EntryUsesManagePermissionAndShowsStandardSaveNotification(
        bool canManage, int? selectedId, int? expectedId, ToastType expectedToast)
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
                return Task.FromResult(Result.Success(new List<LeaveRequestRow>()));
            }
            scopes.Add(office);
            if (name == nameof(ILeaveRequestWorkflow.LoadFormAsync))
            {
                List<LeaveOption> people = office ? [new(17, "Talep Sahibi"), new(42, "Diğer Personel")] : [new(17, "Talep Sahibi")];
                return Task.FromResult(Result.Success(new LeaveRequestForm(
                    new LeaveRequestInput { PersonelId = office ? null : 17 }, people, [], [], [])));
            }
            if (name is nameof(ILeaveRequestWorkflow.CalculateAsync) or nameof(ILeaveRequestWorkflow.SaveAsync))
                processedPeople.Add(((LeaveRequestInput)args[0]!).PersonelId);
            return name switch
            {
                nameof(ILeaveRequestWorkflow.CalculateAsync) => Task.FromResult(Result.Success(new LeaveCalculation("1", "Gün", null, null))),
                nameof(ILeaveRequestWorkflow.SaveAsync) => Task.FromResult(expectedToast == ToastType.Error
                    ? Result.Failure<LeaveWriteResult>(Error.Validation("Personel seçiniz."))
                    : Result.Success(new LeaveWriteResult(1, NotificationFailed: expectedToast == ToastType.Warning))),
                _ => throw new InvalidOperationException(name)
            };
        }));

        await CallAsync("LoadAsync");
        var pendingLink = typeof(LeaveRequestEntryPage).GetMethod("PendingRequestUrl", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(page, [71]);
        Assert.Equal(canManage ? "/ikys/leave-requests/manage?requestId=71" : "/ikys/leave-requests?requestId=71", pendingLink);
        await CallAsync("CalculateAsync");
        await CallAsync("SaveAsync");

        Assert.Equal(new[] { canManage, canManage, canManage }, scopes);
        Assert.Equal(new[] { expectedId, expectedId }, processedPeople);
        var (notification, path) = Assert.Single(notifications);
        Assert.Equal(expectedToast == ToastType.Error ? "/ikys/leave-request-entry" : "/ikys/leave-requests", path);
        Assert.Equal(expectedToast, notification.Type);
        Assert.Equal(expectedToast switch
        {
            ToastType.Error => "Personel seçiniz.",
            ToastType.Warning => "İzin talebi gönderildi. E-posta bildirimi gönderilemedi.",
            _ => "İzin talebi gönderildi."
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
    public async Task PendingRequestLinkOpensOnlyTheRequestedAuthorizedDetail(bool office, bool canViewRequest)
    {
        var page = new LeaveRequestsPage();
        var pageType = typeof(LeaveRequestsPage);
        pageType.GetProperty(nameof(LeaveRequestsPage.Scope))!.SetValue(page, office ? "manage" : null);
        pageType.GetProperty(nameof(LeaveRequestsPage.RequestedDetailId))!.SetValue(page, 71);
        pageType.GetProperty("CurrentUser", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page,
            Stub<ICurrentUserService>((_, args) => Task.FromResult((Operation)args[1]! != Operation.Manage || office)));
        var detailCalls = new List<int>();
        var detail = new LeaveRequestDetail(new LeaveRequestRow(71, "Personel", "Yıllık izin", 1, null, null, "2", "Gün",
            IzinTalep.OnayDurumuTipi.KontrolEdildi), null, null, null, null, [], [], "/izin-belgesi");
        pageType.GetProperty("Workflow", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page,
            Stub<ILeaveRequestWorkflow>((name, args) =>
            {
                if (name == nameof(ILeaveRequestWorkflow.ListAsync))
                    return Task.FromResult(Result.Success(new LeaveRequestList([], 0)));
                Assert.Equal(nameof(ILeaveRequestWorkflow.DetailAsync), name);
                Assert.Equal(office, args[1]);
                detailCalls.Add((int)args[0]!);
                return Task.FromResult(canViewRequest ? Result.Success(detail)
                    : Result.Failure<LeaveRequestDetail>(Error.NotFound("İzin talebi bulunamadı.")));
            }));

        await (Task)pageType.GetMethod("InitializeAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, null)!;

        Assert.Equal(71, Assert.Single(detailCalls));
        Assert.Equal(canViewRequest ? detail : null, pageType.GetField("_detail", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page));
    }

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
