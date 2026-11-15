using System.Net;
using System.Reflection;
using Jenga.BlazorUI.Components.IKYS;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class TaskApprovalReportTests
{
    [Theory]
    [InlineData(true, "Var")]
    [InlineData(false, "Yok")]
    [InlineData(null, "—")]
    public async Task SingleTaskUsesPortraitFormWithTwoSignatures(bool? requested, string expectedRequest)
    {
        var row = Row(1, "Yolaç Test", "2.700,00");
        row.VehicleRequested = requested;
        row.Description = "Temsilcilik faaliyetleri";
        row.Reason = "Millî Ağaçlandırma Günü Etkinliğine katılım, mülki amir ve bağışçı ziyaretleri ile taşınmaz tapu işlemleri";
        var raw = await RenderAsync([row]);
        var html = WebUtility.HtmlDecode(raw);
        foreach (var text in new[] { "Göreve Giden Personel", "Yevmiyesi", "2.700,00", "Görevin Süresi :", "Gideceği Yer :",
            "Başlangıç Tarihi :", "Araç Talebi :", expectedRequest, "06 TEST 01", "Diğer Hususlar", "Temsilcilik faaliyetleri",
            "Uzman Test", "Direktör Test", "GENEL MÜDÜRLÜK MAKAMINA" }) Assert.Contains(text, html);
        Assert.DoesNotContain("Brüt Yevmiye", html);
        Assert.DoesNotContain("Genel Toplam", html);
        Assert.DoesNotContain("Finans Test", html);
        Assert.DoesNotContain("Muh. ve Fin. Dir.", html);
        Assert.DoesNotContain("approval-table", html);
        var preview = Environment.GetEnvironmentVariable("JENGA_REPORT_SINGLE_PREVIEW_PATH");
        if (requested == false && !string.IsNullOrEmpty(preview)) await File.WriteAllTextAsync(preview, raw);
    }

    [Fact]
    public async Task ReferenceReportRendersColumnsTotalsAndRecordedSignatures()
    {
        var items = new[] { Row(1, "Ahmet Test", "2.025,00"), Row(2, "Ebu Test", "1.635,00"), Row(3, "Ömer Test", "2.025,00") };
        var html = await RenderAsync(items);
        foreach (var text in new[] { "TSKGV GÖREV ONAYI", "Net Yevmiye", "Brüt Yevmiye", "İş Avansı", "Araç Plakası",
            "5.685,00 TL", "5.728,15 TL", "2.040,37 TL", "1.647,41 TL", "500,00 TL", "06 TEST 01",
            "GENEL MÜDÜRLÜK MAKAMINA", "Uzman Test", "Direktör Test", "Finans Test", "Per.İd.İşl.Dir. V." })
            Assert.Contains(text, WebUtility.HtmlDecode(html));
        var heading = html[html.IndexOf("<thead", StringComparison.Ordinal)..html.IndexOf("</thead>", StringComparison.Ordinal)];
        Assert.Equal(13, System.Text.RegularExpressions.Regex.Matches(heading, @"<th\b").Count);
        Assert.Equal(3, html.Split("class=\"person-total\"").Length - 1);
        Assert.DoesNotContain("<script>zararli</script>", html);
        var preview = Environment.GetEnvironmentVariable("JENGA_REPORT_PREVIEW_PATH");
        if (!string.IsNullOrEmpty(preview)) await File.WriteAllTextAsync(preview, html);
    }

    [Fact]
    public async Task MultipleTasksShareNameAndCurrencySubtotalsWithoutMergingCurrencies()
    {
        var first = Row(1, "Personel Test", "100,00");
        var second = Row(2, "Personel Test", "50,00"); second.PersonelId = 1; second.Currency = "EUR";
        var html = WebUtility.HtmlDecode(await RenderAsync([first, second]));
        Assert.Contains("rowspan=\"4\"", html);
        Assert.Contains("Genel Toplam (EUR)", html);
        Assert.Contains("Genel Toplam (TL)", html);
        Assert.DoesNotContain("150,00", html);
    }

    private static TaskApprovalListItem Row(int id, string name, string amount) => new()
    {
        TaskApprovalId = id, PersonelId = id, FullName = name, AllowanceText = amount, Currency = "TL", DailyAllowanceText = "1.350,00 TL",
        DutyTitle = "Uzman", Reason = "Maçka, Yomra ve Arsin ziyareti <script>zararli</script>", Destination = "Maçka, Yomra, Arsin",
        StartDate = new(2026, 11, 12, 6, 30, 0), EndDate = new(2026, 11, 13, 18, 30, 0), DurationText = "1,5 gün",
        AdvanceText = "500,00 TL", VehiclePlate = "06 TEST 01", Transportation = "Vakıf Aracı",
        SpecialistSignature = new(1, "Uzman Test", "Kd.Uzm."), PersonnelDirectorSignature = new(2, "Direktör Test", "Per.İd.İşl.Dir.", true),
        FinanceDirectorSignature = new(3, "Finans Test", "Muh. ve Fin. Dir.")
    };

    private static async Task<string> RenderAsync(IReadOnlyList<TaskApprovalListItem> items)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(DispatchProxy.Create<IJSRuntime, JsStub>());
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var root = await renderer.RenderComponentAsync<TaskApprovalReportTable>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                ["Sections"] = TaskApprovalReportBuilder.Build(items).Value, ["ReportDate"] = new DateTime(2026, 11, 15)
            }));
            return root.ToHtmlString();
        });
    }

    public class JsStub : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? method, object?[]? args) => throw new InvalidOperationException(method!.Name);
    }
}
