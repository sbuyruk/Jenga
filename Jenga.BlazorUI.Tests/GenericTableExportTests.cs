using System.Reflection;
using System.Text.RegularExpressions;
using Jenga.BlazorUI.Components.Common;
using Jenga.BlazorUI.Components.Inventory;
using Jenga.BlazorUI.Components.Menus;
using Jenga.BlazorUI.Components.Pages.IKYS;
using Jenga.BlazorUI.Components.Pages.NBYS;
using Jenga.BlazorUI.Components.Pages.TBYS.Kontrol;
using Jenga.DataAccess.Services.TBYS;
using Jenga.Models.Common;
using Jenga.Models.Inventory;
using Jenga.Models.TBYS.Kontrol;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class GenericTableExportTests
{
    [Fact]
    public void EveryExportEnabledTemplateHasAnExplicitHeaderAndRowMapping()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Jenga.sln")))
            directory = directory.Parent;
        Assert.NotNull(directory);
        var files = Directory.GetFiles(Path.Combine(directory.FullName, "Jenga.BlazorUI", "Components"), "*.razor", SearchOption.AllDirectories);
        var count = 0;
        foreach (var file in files)
        foreach (Match table in Regex.Matches(File.ReadAllText(file), @"<GenericTable\b[\s\S]*?</GenericTable>"))
        {
            if (table.Value.Contains("ShowExport=\"false\"", StringComparison.Ordinal)) continue;
            count++;
            Assert.True(table.Value.Contains("ExportHeaders=", StringComparison.Ordinal), $"{file}: Excel başlık eşleştirmesi yok.");
            Assert.True(table.Value.Contains("ExportRow=", StringComparison.Ordinal), $"{file}: Excel satır eşleştirmesi yok.");
        }
        Assert.True(count >= 22);
    }

    [Theory]
    [InlineData("brand")]
    [InlineData("category")]
    [InlineData("model")]
    [InlineData("material")]
    [InlineData("unit")]
    [InlineData("entry")]
    [InlineData("exit")]
    [InlineData("inventory")]
    [InlineData("chart")]
    [InlineData("movement")]
    [InlineData("menu")]
    [InlineData("role")]
    [InlineData("personnel")]
    [InlineData("approval")]
    [InlineData("donor")]
    [InlineData("plan")]
    [InlineData("inspection")]
    public async Task ExcelHeadersAndValuesStayAlignedAcrossPagesAndFilters(string scenario)
    {
        var (component, values, expected, methodName, headerName) = Scenario(scenario);
        var type = component.GetType();
        var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)!;
        var rowType = method.GetParameters()[0].ParameterType;
        var row = CreateRow(rowType);
        foreach (var (property, value) in values)
            rowType.GetProperty(property)!.SetValue(row, value);
        var headers = (IReadOnlyList<string>)type.GetField(headerName, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        var tableType = typeof(GenericTable<>).MakeGenericType(rowType);
        var table = Activator.CreateInstance(tableType)!;
        var runtime = new ExportRuntime();
        tableType.GetProperty("JSRuntime", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(table, runtime);
        var rowFactory = method.CreateDelegate(typeof(Func<,>).MakeGenericType(rowType, typeof(IReadOnlyList<object?>)), method.IsStatic ? null : component);
        var items = Array.CreateInstance(rowType, 12);
        for (var i = 0; i < items.Length; i++) items.SetValue(row, i);
        ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            ["Items"] = items, ["PageSize"] = 5, ["ExportHeaders"] = headers, ["ExportRow"] = rowFactory
        }).SetParameterProperties(table);
        tableType.GetMethod("OnParametersSet", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(table, null);

        async Task Export() => await (Task)tableType.GetMethod("ExportToExcel", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(table, null)!;
        await Export();
        Assert.Equal(12, runtime.Rows.Length);
        Assert.Equal(expected.Keys, runtime.Headers);
        Assert.All(runtime.Rows, exported => Assert.Equal(expected.Values, exported));

        tableType.GetProperty("ExportOnlyCurrentPage")!.SetValue(table, true);
        await Export();
        Assert.Equal(5, runtime.Rows.Length);
        tableType.GetProperty("ExportOnlyCurrentPage")!.SetValue(table, false);
        tableType.GetProperty("SearchText", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(table, "__bulunmayan_kayit__");
        await Export();
        Assert.Empty(runtime.Rows);
        Assert.Equal(expected.Keys, runtime.Headers);
    }

    private static object CreateRow(Type type)
    {
        if (type.Name == "RoleRow") return Activator.CreateInstance(type, new Role())!;
        if (type.Name == "MenuRow") return Activator.CreateInstance(type, new MenuItem(), new List<MenuItem>())!;
        if (type.Name == "PersonelSatir") return Activator.CreateInstance(type, "", "", "", (DateTime?)null)!;
        return Activator.CreateInstance(type, nonPublic: true)!;
    }

    private static (object Component, Dictionary<string, object?> Values, Dictionary<string, string> Expected, string Method, string Headers) Scenario(string scenario)
    {
        object component;
        Dictionary<string, object?> values;
        Dictionary<string, string> expected;
        var date = new DateTime(2026, 11, 15, 9, 30, 0);
        switch (scenario)
        {
            case "brand":
                component = Prepare(new MaterialBrandTable(), new() { ["Categories"] = new List<MaterialCategory> { new() { Id = 9, CategoryName = "Kategori adı" } } });
                values = new() { ["Id"] = 111, ["BrandName"] = "Marka", ["CategoryId"] = 9, ["Aciklama"] = "Not" };
                expected = new() { ["Ad"] = "Marka", ["Kategori"] = "Kategori adı", ["Açıklama"] = "Not" }; break;
            case "category":
                component = Prepare(new MaterialCategoryTable(), new() { ["Categories"] = new List<MaterialCategory> { new() { Id = 9, CategoryName = "Üst kategori" } } });
                values = new() { ["Id"] = 111, ["CategoryName"] = "Kategori", ["ParentCategoryId"] = 9, ["Aciklama"] = "Not" };
                expected = new() { ["Ad"] = "Kategori", ["Açıklama"] = "Not", ["Üst Kategori"] = "Üst kategori" }; break;
            case "model":
                component = Prepare(new MaterialModelTable(), new() { ["Brands"] = new List<MaterialBrand> { new() { Id = 9, BrandName = "Marka adı" } } });
                values = new() { ["Id"] = 111, ["ModelName"] = "Model", ["BrandId"] = 9, ["Aciklama"] = "Not" };
                expected = new() { ["Ad"] = "Model", ["Marka"] = "Marka adı", ["Açıklama"] = "Not" }; break;
            case "material":
                component = Prepare(new MaterialTable(), new() { ["Categories"] = new List<MaterialCategory> { new() { Id = 9, CategoryName = "Kategori" } }, ["MaterialUnits"] = new List<MaterialUnit> { new() { Id = 8, Name = "Adet" } } });
                values = new() { ["Id"] = 111, ["MaterialName"] = "Malzeme", ["CategoryId"] = 9, ["MaterialUnitId"] = 8, ["IsAsset"] = true, ["Aciklama"] = "Not" };
                expected = new() { ["Ad"] = "Malzeme", ["Kategori"] = "Kategori", ["Birim"] = "Adet", ["Varlık mı"] = "Evet", ["Açıklama"] = "Not" }; break;
            case "unit":
                component = new MaterialUnitTable(); values = new() { ["Id"] = 111, ["Name"] = "Adet", ["Symbol"] = "ad", ["Aciklama"] = "Not" };
                expected = new() { ["Birim Adı"] = "Adet", ["Sembol"] = "ad", ["Açıklama"] = "Not" }; break;
            case "entry":
                component = new MaterialEntryTable(); values = new() { ["Id"] = 111, ["EntryDate"] = date, ["MaterialName"] = "Malzeme", ["BrandName"] = "Marka", ["ModelName"] = "Model", ["Quantity"] = 123, ["QuantityText"] = "12 ad", ["UnitName"] = "gizli alan", ["LocationPath"] = "Bina / Oda", ["PersonName"] = "Personel" };
                expected = new() { ["Tarih"] = "2026-11-15", ["Malzeme"] = "Malzeme", ["Marka"] = "Marka", ["Model"] = "Model", ["Miktar"] = "12 ad", ["Lokasyon"] = "Bina / Oda", ["Personel"] = "Personel" }; break;
            case "exit":
                component = new MaterialExitTable(); values = new() { ["Id"] = 111, ["ExitDate"] = date, ["ExitDateText"] = "15.11.2026", ["MaterialName"] = "Malzeme", ["Quantity"] = 12, ["UnitName"] = "Adet", ["LocationName"] = "Oda", ["ExitTypeText"] = "Çıkış", ["Description"] = "Not" };
                expected = new() { ["Tarih"] = "15.11.2026", ["Malzeme"] = "Malzeme", ["Miktar"] = "12", ["Birim"] = "Adet", ["Lokasyon"] = "Oda", ["Çıkış Tipi"] = "Çıkış", ["Açıklama"] = "Not" }; break;
            case "inventory":
                component = new MaterialInventoryTable(); values = new() { ["MaterialId"] = 111, ["MaterialName"] = "Malzeme", ["CategoryName"] = "Kategori", ["TotalQuantity"] = 3m, ["UnitSymbol"] = "ad", ["HasCriticalStock"] = true, ["IsCritical"] = true, ["CriticalStock"] = 5 };
                expected = new() { ["Malzeme"] = "Malzeme", ["Kategori"] = "Kategori", ["Toplam Miktar"] = "3", ["Birim"] = "ad", ["Kritik"] = "Kritik: 5" }; break;
            case "chart":
                component = new MaterialInventoryChart<object>(); values = new() { ["MaterialId"] = 111, ["MaterialName"] = "Malzeme", ["BrandName"] = "Marka", ["ModelName"] = "Model", ["LocationName"] = "Oda", ["Quantity"] = 12m, ["UnitSymbol"] = "ad" };
                expected = new() { ["Malzeme"] = "Malzeme", ["Marka"] = "Marka", ["Model"] = "Model", ["Konum"] = "Oda", ["Miktar"] = "12", ["Birim"] = "ad" }; break;
            case "movement":
                component = new MaterialMovementTable(); values = new() { ["ActionType"] = "Transfer", ["FromLocation"] = "Oda 1", ["FromPerson"] = "Kişi 1", ["ToLocation"] = "Oda 2", ["ToPerson"] = "Kişi 2", ["Quantity"] = 12m, ["UnitSymbol"] = "ad", ["Brand"] = "Marka", ["Model"] = "Model", ["Operation"] = "Teslim", ["Date"] = date };
                expected = new() { ["Hareket"] = "Transfer", ["Nereden"] = "Oda 1", ["Kimden"] = "Kişi 1", ["Nereye"] = "Oda 2", ["Kime"] = "Kişi 2", ["Miktar"] = "12 ad", ["Marka"] = "Marka", ["Model"] = "Model", ["İşlem"] = "Teslim", ["Tarih"] = "15.11.2026 09:30" }; break;
            case "menu":
                component = new MenuItemTable(); values = new() { ["Id"] = 111, ["Title"] = "Menü", ["Url"] = "/sayfa", ["ParentTitle"] = "Üst menü", ["IsVisible"] = false };
                expected = new() { ["Id"] = "111", ["Başlık"] = "Menü", ["Url"] = "/sayfa", ["Üst Menüsü"] = "Üst menü", ["Görünür"] = "Hayır" }; break;
            case "role":
                component = new RoleTable(); values = new() { ["Id"] = 111, ["Name"] = "Rol", ["Aciklama"] = "Not" };
                expected = new() { ["Ad"] = "Rol", ["Açıklama"] = "Not" }; break;
            case "personnel":
                component = new PersonelDashboard(); values = new() { ["AdSoyad"] = "Personel", ["BirimAdi"] = "Birim", ["Unvan"] = "Ünvan", ["BaslamaTarihi"] = date };
                expected = new() { ["Ad Soyad"] = "Personel", ["Birim"] = "Birim", ["Ünvan"] = "Ünvan", ["Başlama Tarihi"] = "15.11.2026" }; break;
            case "approval":
                component = new TaskApprovalListPage(); values = new() { ["TaskApprovalId"] = 111, ["PersonelId"] = 222, ["FullName"] = "Personel", ["Reason"] = "Görev", ["StartDate"] = date, ["EndDate"] = date.AddDays(1), ["Destination"] = "Ankara", ["ManagerApprovalValue"] = 2, ["RejectionNote"] = "Ret gerekçesi" };
                expected = new() { ["Adı Soyadı"] = "Personel", ["Görevin Sebebi"] = "Görev", ["Gidiş Tarihi"] = "15.11.2026 09:30", ["Dönüş Tarihi"] = "16.11.2026 09:30", ["Görevin Yeri"] = "Ankara", ["Amir Onayı"] = "Reddedildi", ["Ret Gerekçesi"] = "Ret gerekçesi" }; break;
            case "donor":
                component = new ManagerSummaryNakitPage(); values = new() { ["ProvinceId"] = 111, ["ProvinceName"] = "Ankara", ["Month"] = 11, ["MonthLabel"] = "Kasım", ["DonorCount"] = 1234, ["NewDonorCount"] = 56 };
                expected = new() { ["İl"] = "Ankara", ["Ay"] = "Kasım", ["Bağışçı"] = "1.234", ["Yeni Bağışçı"] = "56" }; break;
            case "plan":
                component = new TasinmazKontrolListe(); values = new() { ["TasinmazId"] = 111, ["AcikTaslakId"] = 5, ["Adres"] = "Adres", ["AdaNo"] = "Ada", ["ParselNo"] = "Parsel", ["Cinsi"] = "Arsa", ["Ili"] = "Ankara", ["Ilcesi"] = "Çankaya", ["BolgeAdi"] = "Merkez" };
                expected = new() { ["Durum"] = "Taslak açık", ["Adres"] = "Adres", ["Ada"] = "Ada", ["Parsel"] = "Parsel", ["Cinsi"] = "Arsa", ["İli"] = "Ankara", ["İlçesi"] = "Çankaya", ["Bölge"] = "Merkez", ["Periyot"] = KontrolPeriyodu.Etiket(null, null), ["Son Kontrol"] = "—", ["Sonraki Kontrol"] = "—" }; break;
            case "inspection":
                component = new TasinmazKontrolListe(); values = new() { ["Id"] = 111, ["TasinmazId"] = 222, ["RaporNo"] = "R-1", ["Durum"] = KontrolDurumu.Tamamlandi, ["KontrolTarihi"] = date, ["EmlakSicilNo"] = "S-1", ["Cinsi"] = "Arsa", ["Ili"] = "Ankara", ["Ilcesi"] = "Çankaya", ["BolgeAdi"] = "Merkez", ["KontrolEdenler"] = "Personel", ["RevizeEdilenKontrolId"] = 12 };
                expected = new() { ["#"] = "#111 (rev. #12)", ["Rapor No"] = "R-1", ["Durum"] = "Tamamlandı", ["Tarih"] = "15.11.2026", ["Sicil No"] = "S-1", ["Cinsi"] = "Arsa", ["İli / İlçesi"] = "Ankara / Çankaya", ["Bölge"] = "Merkez", ["Kontrol Edenler"] = "Personel", ["Sonraki"] = "—" }; break;
            default: throw new ArgumentOutOfRangeException(nameof(scenario));
        }
        return (component, values, expected,
            scenario == "plan" ? "BuildPlanExportRow" : scenario == "inspection" ? "BuildKontrolExportRow" : "BuildExportRow",
            scenario == "plan" ? "PlanExportHeaders" : scenario == "inspection" ? "KontrolExportHeaders" : "TableExportHeaders");
    }

    private static T Prepare<T>(T component, Dictionary<string, object?> parameters) where T : ComponentBase
    {
        ParameterView.FromDictionary(parameters).SetParameterProperties(component);
        component.GetType().GetMethod("OnParametersSet", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(component, null);
        return component;
    }

    private sealed class ExportRuntime : IJSRuntime, IJSObjectReference
    {
        public string[] Headers { get; private set; } = [];
        public string[][] Rows { get; private set; } = [];
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => InvokeAsync<T>(identifier, CancellationToken.None, args);
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier == "import") return ValueTask.FromResult((T)(object)this);
            Assert.Equal("downloadXlsx", identifier);
            Headers = ((IReadOnlyList<string>)args![1]!).ToArray();
            Rows = (string[][])args[2]!;
            return ValueTask.FromResult(default(T)!);
        }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
