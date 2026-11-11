using System.Net;
using System.Reflection;
using Jenga.BlazorUI.Components.Pages.IKYS;
using Jenga.BlazorUI.Services.Common.Toast;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class OvertimeReportTests
{
    [Theory]
    [InlineData(1, "Kadrolu Test", "1:30")]
    [InlineData(2, "Harici Test", "3:30")]
    [InlineData(0, "Tipsiz Test", "1:00")]
    public async Task PrintableChartContainsOnlySelectedPersonnelGroupAndItsTotal(int group, string expectedName, string total)
    {
        var report = new OvertimeReport { CanSeeSummary = true, Period = new() { Id = 1, StartDate = new(2026, 10, 26), EndDate = new(2026, 11, 1) } };
        foreach (var (id, name, type, minutes) in new (int, string, PersonelTipi?, int)[]
        {
            (1, "Kadrolu Test", PersonelTipi.Kadrolu, 90), (2, "Harici Test", PersonelTipi.KadroHarici, 210), (3, "Tipsiz Test", null, 60)
        })
        {
            report.Results.Add(new() { PersonelId = id, FullName = name, PersonnelType = type, KadroHariciKurum = type == PersonelTipi.KadroHarici ? "Kurum A" : null, RoundedMinutes = minutes, WeeksJson = "[]" });
            report.Entries.Add(new() { PersonelId = id, FullName = name, PersonnelType = type, Title = "Uzman", NormalWeeklyMinutes = 2400, Reason = "Görev" });
        }
        var html = await RenderAsync(report, group);
        Assert.Contains(expectedName, html);
        foreach (var other in new[] { "Kadrolu Test", "Harici Test", "Tipsiz Test" }.Where(x => x != expectedName))
            Assert.DoesNotContain(other, html);
        Assert.Matches($"<th[^>]*>{System.Text.RegularExpressions.Regex.Escape(total)}</th>", html);
        if (group == 2)
        {
            Assert.Contains("KADRO HARİCİ PERSONEL FAZLA ÇALIŞMA ÇİZELGESİ", html);
            Assert.Contains("Kurum A kurumuna iletilmek üzere", html);
            Assert.DoesNotContain("ücretlerinin ödenmesini arz ederim", html);
        }
    }

    [Theory]
    [InlineData("kurum a", "Harici A", "Harici B", "1:30")]
    [InlineData("Kurum B", "Harici B", "Harici A", "3:30")]
    public async Task ExternalChartPrintsOnlySelectedInstitution(string institution, string included, string excluded, string total)
    {
        var report = new OvertimeReport { CanSeeSummary = true };
        report.Results.Add(new() { PersonelId = 1, FullName = "Harici A", PersonnelType = PersonelTipi.KadroHarici, KadroHariciKurum = "Kurum A", RoundedMinutes = 90 });
        report.Results.Add(new() { PersonelId = 2, FullName = "Harici B", PersonnelType = PersonelTipi.KadroHarici, KadroHariciKurum = "Kurum B", RoundedMinutes = 210 });
        var html = await RenderAsync(report, 2, institution);
        Assert.Contains(included, html);
        Assert.DoesNotContain(excluded, html);
        Assert.Matches($"<th[^>]*>{total}</th>", html);
    }

    [Fact]
    public async Task InstitutionIsRequiredForExternalPrintout()
    {
        var report = new OvertimeReport { CanSeeSummary = true };
        report.Results.Add(new() { PersonelId = 1, FullName = "Eksik Kurum", PersonnelType = PersonelTipi.KadroHarici, RoundedMinutes = 90 });
        var html = await RenderAsync(report, 2);
        Assert.Contains("Kurum bilgisi eksik", html);
        Assert.Matches("<button[^>]*disabled[^>]*>Yazdır</button>", html);
    }

    private static async Task<string> RenderAsync(OvertimeReport report, int group, string? institution = null)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<IComponentActivator>(new ReportActivator(report, group, institution));
        services.AddSingleton(DispatchProxy.Create<IOvertimeService, UnusedService>());
        services.AddSingleton<IToastService, ToastService>();
        services.AddSingleton<IJSRuntime, TestJs>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new TestRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var root = renderer.BeginRenderingComponent(typeof(OvertimeReportPage), ParameterView.Empty);
            await root.QuiescenceTask;
            return WebUtility.HtmlDecode(root.ToHtmlString());
        });
    }

    private sealed class ReportActivator(OvertimeReport report, int group, string? institution) : IComponentActivator
    {
        public IComponent CreateInstance(Type componentType)
        {
            var component = (IComponent)Activator.CreateInstance(componentType)!;
            if (component is OvertimeReportPage page)
            {
                page.PersonnelGroup = group;
                page.Institution = institution;
                typeof(OvertimeReportPage).GetField("_report", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(page, report);
            }
            return component;
        }
    }

#pragma warning disable BL0006 // Render the interactive page without a browser or JS lifecycle.
    private sealed class TestRenderer(IServiceProvider services, ILoggerFactory loggerFactory) : StaticHtmlRenderer(services, loggerFactory)
    {
        protected override IComponent ResolveComponentForRenderMode(Type componentType, int? parentComponentId,
            IComponentActivator componentActivator, IComponentRenderMode renderMode) => componentActivator.CreateInstance(componentType);
    }
#pragma warning restore BL0006

    public class UnusedService : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => throw new InvalidOperationException(targetMethod?.Name);
    }
    private sealed class TestJs : IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => ValueTask.FromResult(default(T)!);
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<T>(identifier, args);
    }
}
