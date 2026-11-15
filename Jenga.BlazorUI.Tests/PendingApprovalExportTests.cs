using System.Reflection;
using Jenga.BlazorUI.Components.Common;
using Jenga.BlazorUI.Components.IKYS;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class PendingApprovalExportTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExcelPayloadMatchesVisibleColumnOrderAndIncludesAllPages(bool review)
    {
        var js = new ExportRuntime();
        var table = new ExportTable();
        typeof(GenericTable<PendingApprovalItem>).GetProperty("JSRuntime", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(table, js);
        var items = Enumerable.Range(1, 12).Select(i => new PendingApprovalItem
        {
            GorevOnayId = 100 + i, PersonelId = 200 + i, AdiSoyadi = $"Personel {i}",
            BaslangicTarihi = new(2026, 11, 15, 9, 30, 0), BitisTarihi = new(2026, 11, 16, 18, 15, 0),
            Sure = "1,5", GorevinSebebi = "Görev sebebi", GorevinYeri = "Ankara", UlasimAraci = "Uçak",
            Transfer = "Transfer bilgisi", Konaklama = "Otel", Aciklama = "Görev açıklaması",
            AmirOnayi = 2, OnayRedAciklama = "Ret gerekçesi"
        }).ToList();
        ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(table.Items)] = items,
            [nameof(table.PageSize)] = 5,
            [nameof(table.ExportFileName)] = "bekleyen-islemler.xlsx",
            [nameof(table.HeaderTemplate)] = (RenderFragment)(_ => { }),
            [nameof(table.ExportHeaders)] = review ? PendingApprovalExport.ReviewHeaders : PendingApprovalExport.TaskHeaders,
            [nameof(table.ExportRow)] = (Func<PendingApprovalItem, IReadOnlyList<object?>>)
                (review ? PendingApprovalExport.ReviewRow : PendingApprovalExport.TaskRow)
        }).SetParameterProperties(table);
        table.Initialize();

        await (Task)typeof(GenericTable<PendingApprovalItem>).GetMethod("ExportToExcel", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(table, null)!;

        Assert.Equal("bekleyen-islemler.xlsx", js.FileName);
        Assert.Equal(review ? 11 : 9, js.Headers.Length);
        Assert.Equal(12, js.Rows.Length);
        Assert.All(js.Rows, row => Assert.Equal(js.Headers.Length, row.Length));
        var first = js.Headers.Zip(js.Rows[0]).ToDictionary(x => x.First, x => x.Second);
        Assert.Equal("Personel 1", first["Adı Soyadı"]);
        Assert.Equal(review ? "15.11.2026" : "15.11.2026 09:30", first["Başlangıç Tarihi"]);
        Assert.Equal(review ? "16.11.2026" : "16.11.2026 18:15", first["Bitiş Tarihi"]);
        Assert.Equal("Görev sebebi", first["Görevin Sebebi"]);
        Assert.Equal("Ankara", first["Görevin Yeri"]);
        Assert.Equal("Uçak", first["Ulaşım Aracı"]);
        Assert.Equal("Transfer bilgisi", first["Transfer"]);
        Assert.Equal("Otel", first["Konaklama"]);
        Assert.Equal("Görev açıklaması", first["Açıklama"]);
        if (review)
        {
            Assert.Equal("Reddedildi", first["Durum"]);
            Assert.Equal("Ret gerekçesi", first["Ret Gerekçesi"]);
        }
        Assert.DoesNotContain("İşlemler", js.Headers);
        Assert.DoesNotContain("GorevOnayId", js.Headers);
        Assert.DoesNotContain("PersonelId", js.Headers);
        Assert.Equal("Personel 12", js.Rows[^1][0]);
    }

    private sealed class ExportTable : GenericTable<PendingApprovalItem>
    {
        public void Initialize() => OnParametersSet();
    }

    private sealed class ExportRuntime : IJSRuntime, IJSObjectReference
    {
        public string? FileName { get; private set; }
        public string[] Headers { get; private set; } = [];
        public string[][] Rows { get; private set; } = [];
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier == "import") return ValueTask.FromResult((TValue)(object)this);
            if (identifier == "getTableHeaderTexts") return ValueTask.FromResult((TValue)(object)new[] { "Yanlış başlık" });
            Assert.Equal("downloadXlsx", identifier);
            FileName = (string)args![0]!;
            Headers = ((IReadOnlyList<string>)args[1]!).ToArray();
            Rows = (string[][])args[2]!;
            return ValueTask.FromResult(default(TValue)!);
        }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
