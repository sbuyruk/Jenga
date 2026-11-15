using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.IKYS;
using Xunit;

namespace Jenga.Overtime.Tests;

public sealed class TaskApprovalReportTests
{
    [Fact]
    public void ReferenceReportTotalsSumRoundedGrossRows()
    {
        var report = TaskApprovalReportBuilder.Build([Row(1, "2.025,00 TL"), Row(2, "1.635,00"), Row(3, "2.025,00")]);
        Assert.True(report.IsSuccess, report.Error.Message);
        var section = Assert.Single(report.Value);
        Assert.Equal(new[] { 2040.37m, 1647.41m, 2040.37m }, section.Rows.Select(x => x.GrossAmount));
        Assert.Equal(new TaskApprovalReportTotal("TL", 5685m, 5728.15m), Assert.Single(section.Totals));
    }

    [Fact]
    public void CurrencyTotalsRemainSeparateAndTryAliasCombinesWithTl()
    {
        var report = TaskApprovalReportBuilder.Build([Row(1, "100,00"), Row(2, "200,00 TRY", "TRY"), Row(3, "50,00 EUR", "EUR")]);
        var section = Assert.Single(report.Value);
        Assert.Equal(new[] { "EUR", "TL" }, section.Totals.Select(x => x.Currency));
        Assert.Equal(300m, section.Totals.Single(x => x.Currency == "TL").NetAmount);
        Assert.Equal(50m, section.Totals.Single(x => x.Currency == "EUR").NetAmount);
    }

    [Fact]
    public void DifferentSignatoriesAndDeputyFlagsProduceSeparateDocuments()
    {
        var first = Row(1, "100"); var second = Row(2, "100"); var third = Row(3, "100");
        first.PersonnelDirectorSignature = new(10, "İmzacı", "Direktör");
        second.PersonnelDirectorSignature = new(10, "İmzacı", "Direktör", true);
        third.PersonnelDirectorSignature = first.PersonnelDirectorSignature;
        var sections = TaskApprovalReportBuilder.Build([first, second, third]).Value;
        Assert.Equal(2, sections.Count);
        Assert.Equal(2, sections[0].Rows.Count);
        Assert.True(sections[1].PersonnelDirector.IsDeputy);
    }

    [Theory]
    [InlineData(null, "TL")]
    [InlineData("", "TL")]
    [InlineData("bozuk", "TL")]
    [InlineData("-1,00", "TL")]
    [InlineData("1.23", "TL")]
    [InlineData("1,23 EUR", "TL")]
    [InlineData("1,23", null)]
    public void InvalidAmountsCannotSilentlyBecomeZero(string? value, string? currency)
    {
        var report = TaskApprovalReportBuilder.Build([Row(15, value, currency)]);
        Assert.True(report.IsFailure);
        Assert.Contains("#15", report.Error.Message);
    }

    [Fact]
    public void ZeroAmountAndEmptySelectionAreSupported()
    {
        Assert.Equal(0m, TaskApprovalReportBuilder.Build([Row(1, "0,00")]).Value[0].Rows[0].GrossAmount);
        Assert.Empty(TaskApprovalReportBuilder.Build([]).Value);
    }

    private static TaskApprovalListItem Row(int id, string? amount, string? currency = "TL") => new()
    {
        TaskApprovalId = id, PersonelId = id, FullName = "Personel " + id, AllowanceText = amount, Currency = currency
    };
}
