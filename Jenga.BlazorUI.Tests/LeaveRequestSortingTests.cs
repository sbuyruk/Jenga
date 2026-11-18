using System.Collections;
using System.Reflection;
using Jenga.BlazorUI.Components.Common;
using Jenga.BlazorUI.Components.Pages.IKYS;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class LeaveRequestSortingTests
{
    [Theory]
    [InlineData("Dates", new[] { 2, 3, 4, 1 })]
    [InlineData("Duration", new[] { 4, 2, 3, 1 })]
    public void TableUsesDateValuesAndNumericDurationsInBothDirections(string field, int[] ascending)
    {
        var rows = new[]
        {
            Row(1, new DateTime(2026, 12, 2), 10, "10", "Gün"),
            Row(2, new DateTime(2026, 11, 27), 0, "10:00:00", "Saat"),
            Row(3, new DateTime(2026, 11, 27), 1, "2", "Gün"),
            Row(4, new DateTime(2026, 11, 28), 0, "02:00:00", "Saat")
        };
        var rowType = typeof(LeaveRequestsPage).GetNestedType("LeaveTableRow", BindingFlags.NonPublic)!;
        var tableType = typeof(GenericTable<>).MakeGenericType(rowType);
        var items = Array.CreateInstance(rowType, rows.Length);
        for (var index = 0; index < rows.Length; index++)
            items.SetValue(Activator.CreateInstance(rowType, rows[index]), index);

        var table = Activator.CreateInstance(tableType)!;
        ParameterView.FromDictionary(new Dictionary<string, object?> { ["Items"] = items }).SetParameterProperties(table);
        tableType.GetMethod("OnParametersSet", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(table, null);

        int[] Sort()
        {
            tableType.GetMethod("SortBy")!.Invoke(table, [field]);
            var page = (IEnumerable)tableType.GetProperty("CurrentPageItems")!.GetValue(table)!;
            var request = rowType.GetProperty("Request")!;
            return page.Cast<object>().Select(item => ((LeaveRequestRow)request.GetValue(item)!).Id).ToArray();
        }

        Assert.Equal(ascending, Sort());
        Assert.Equal(ascending.Reverse(), Sort());
    }

    private static LeaveRequestRow Row(int id, DateTime start, int extraDays, string duration, string unit) =>
        new(id, "Personel", "İzin", null, start, start.AddDays(extraDays), duration, unit, null);
}
