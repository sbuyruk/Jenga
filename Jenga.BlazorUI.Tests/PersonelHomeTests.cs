using System.Net;
using System.Reflection;
using Jenga.BlazorUI.Components.IKYS;
using Jenga.BlazorUI.Services.Common;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.IKYS;
using Jenga.Utility.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class PersonelHomeTests
{
    [Fact]
    public async Task SelectedPersonUsesHomeLayoutWithoutChangingIdentityOrReadingRestrictedSections()
    {
        var requestedId = 0;
        var factory = new UnusedDatabaseFactory();
        var summary = new PersonelSummary
        {
            PersonelId = 2, FullName = "Seçilen Personel", SicilNo = 186, UnitName = "Test Birimi", Title = "Uzman",
            WorkEmail = "selected@example.test",
            TaskApprovals = [new(5, "Görev Onayı", DateTime.Today, "<p><strong>Görev açıklaması</strong></p>",
                "/ikys/task-approvals?SecilenId=5", 0, DateTime.Today, "Ankara")]
        };
        var summaryService = Fake<IPersonelSummaryService>((method, args) =>
        {
            Assert.Equal("GetAsync", method);
            requestedId = (int)args![0]!;
            return Task.FromResult(Result.Success(summary));
        });
        var currentUser = Fake<ICurrentUserService>((method, _) => method switch
        {
            "GetCurrentPersonelAsync" => Task.FromResult<Personel?>(new() { Id = 1, Adi = "Oturum", Soyadi = "Kullanıcısı" }),
            "HasPermissionAsync" => Task.FromResult(false),
            _ => throw new InvalidOperationException(method)
        });

        var html = await RenderAsync(factory, currentUser, summaryService, 2);
        Assert.Equal(2, requestedId);
        Assert.Contains("workspace-hero", html);
        Assert.Contains("Seçilen Personel", html);
        Assert.Contains("Test Birimi", html);
        Assert.Contains("Görev açıklaması", html);
        Assert.Contains("Onay Bekliyor", html);
        Assert.DoesNotContain("<strong>Görev açıklaması</strong>", html);
        Assert.DoesNotContain("Oturum Kullanıcısı", html);
        Assert.Contains("Bu bilgileri görüntüleme yetkiniz bulunmuyor.", html);
        Assert.DoesNotContain("yüklenemedi", html);
        Assert.Equal(1, factory.CreatedContexts);
    }

    [Fact]
    public async Task DeniedProfileNeverReadsDashboardData()
    {
        var factory = new UnusedDatabaseFactory();
        var current = Fake<ICurrentUserService>((method, _) => method == "GetCurrentPersonelAsync"
            ? Task.FromResult<Personel?>(new() { Id = 1, Adi = "Oturum", Soyadi = "Kullanıcısı" })
            : throw new InvalidOperationException(method));
        var denied = Fake<IPersonelSummaryService>((_, _) => Task.FromResult(
            Result.Failure<PersonelSummary>(Error.Forbidden("Bu personele erişiminiz yok."))));
        var html = await RenderAsync(factory, current, denied, 2);
        Assert.Contains("Bu personele erişiminiz yok.", html);
        Assert.DoesNotContain("workspace-hero", html);
        Assert.Equal(0, factory.CreatedContexts);
    }

    [Fact]
    public async Task DefaultHomeDoesNotRequireAnotherPersonnelProfile()
    {
        var factory = new UnusedDatabaseFactory();
        var current = Fake<ICurrentUserService>((method, _) => method == "GetCurrentPersonelAsync"
            ? Task.FromResult<Personel?>(null) : throw new InvalidOperationException(method));
        var summary = Fake<IPersonelSummaryService>((method, _) => throw new InvalidOperationException(method));
        var html = await RenderAsync(factory, current, summary, null);
        Assert.Contains("Merhaba, Kullanıcı", html);
        Assert.Contains("İzin Taleplerim", html);
        Assert.Equal(0, factory.CreatedContexts);
    }

    private static async Task<string> RenderAsync(UnusedDatabaseFactory factory, ICurrentUserService current,
        IPersonelSummaryService summary, int? personelId)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(factory);
        services.AddSingleton(current);
        services.AddSingleton(summary);
        services.AddSingleton(Fake<IGorevOnayService>((method, _) => throw new InvalidOperationException(method)));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?> { ["PersonelId"] = personelId });
            var root = await renderer.RenderComponentAsync<PersonelHome>(parameters);
            return WebUtility.HtmlDecode(root.ToHtmlString());
        });
    }

    private sealed class UnusedDatabaseFactory : IDbContextFactory<ApplicationDbContext>
    {
        public int CreatedContexts { get; private set; }
        public ApplicationDbContext CreateDbContext()
        {
            CreatedContexts++;
            return new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=invalid;Database=Unused;Integrated Security=True;Connect Timeout=1").Options);
        }
    }

    private static T Fake<T>(Func<string, object?[]?, object?> invoke) where T : class
    {
        var instance = DispatchProxy.Create<T, ServiceStub>();
        ((ServiceStub)(object)instance).InvokeMethod = invoke;
        return instance;
    }

    public class ServiceStub : DispatchProxy
    {
        public Func<string, object?[]?, object?> InvokeMethod { get; set; } = null!;
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => InvokeMethod(targetMethod!.Name, args);
    }
}
