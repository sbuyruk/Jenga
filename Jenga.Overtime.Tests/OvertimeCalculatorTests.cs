using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.IKYS;
using Xunit;

namespace Jenga.Overtime.Tests;

public class OvertimeCalculatorTests
{
    private static readonly DateTime Monday = new(2026, 10, 26);
    private static OvertimeEntry Entry(int day, int normalHours, params (int Start, int End)[] intervals) => new()
    {
        PersonelId = 1, WorkDate = Monday.AddDays(day), NormalWeeklyMinutes = normalHours * 60,
        Intervals = intervals.Select(x => new OvertimeInterval { StartMinute = x.Start, EndMinute = x.End }).ToList()
    };

    [Theory]
    [InlineData(0, 0)] [InlineData(1, 15)] [InlineData(240, 15)]
    [InlineData(241, 30)] [InlineData(450, 30)] [InlineData(451, 60)]
    public void WeekendRest_UsesInclusiveBoundaries(int gross, int rest)
        => Assert.Equal(rest, OvertimeCalculator.RestMinutes(gross));

    [Theory]
    [InlineData(0, 0)] [InlineData(1, 30)] [InlineData(30, 30)] [InlineData(31, 60)]
    [InlineData(420, 420)] [InlineData(421, 450)]
    public void FinalRounding_GoesToNextHalfHour(int raw, int rounded)
        => Assert.Equal(rounded, OvertimeCalculator.RoundTotal(raw));

    [Theory]
    [InlineData(40, 120)] [InlineData(45, 420)]
    public void EightHourSaturday_HasOneHourRestAndCommon45HourThreshold(int normalHours, int expected)
    {
        var week = Assert.Single(OvertimeCalculator.Calculate([Entry(5, normalHours, (480, 960))], Monday, Monday.AddDays(6)));
        Assert.Equal(60, week.RestMinutes);
        Assert.Equal(expected, week.OvertimeMinutes);
    }

    [Fact]
    public void SeparateIntervalsOnSaturday_AreCombinedBeforeRest()
    {
        var week = Assert.Single(OvertimeCalculator.Calculate([Entry(5, 45, (480, 660), (780, 960))], Monday, Monday.AddDays(6)));
        Assert.Equal(360, week.GrossMinutes);
        Assert.Equal(30, week.RestMinutes);
        Assert.Equal(330, week.OvertimeMinutes);
    }

    [Fact]
    public void SaturdayAndSunday_GetSeparateRestDeductions()
    {
        var week = Assert.Single(OvertimeCalculator.Calculate([Entry(5, 45, (480, 660)), Entry(6, 45, (480, 660))], Monday, Monday.AddDays(6)));
        Assert.Equal(30, week.RestMinutes);
    }

    [Fact]
    public void Weekday_HasNoWeekendRest()
    {
        var week = Assert.Single(OvertimeCalculator.Calculate([Entry(0, 45, (1020, 1140))], Monday, Monday.AddDays(6)));
        Assert.Equal(0, week.RestMinutes);
        Assert.Equal(120, week.OvertimeMinutes);
    }

    [Theory]
    [InlineData(240, 15)]
    [InlineData(241, 30)]
    [InlineData(450, 30)]
    [InlineData(451, 60)]
    public void WeekdayPublicHolidayUsesWeekendRestThresholds(int gross, int rest)
    {
        var week = Assert.Single(OvertimeCalculator.Calculate([Entry(3, 45, (480, 480 + gross))], Monday, Monday.AddDays(6), new HashSet<DateTime> { Monday.AddDays(3) }));
        Assert.Equal(rest, week.RestMinutes);
        Assert.Equal(gross - rest, week.OvertimeMinutes);
    }

    [Fact]
    public void HolidayAndWeekendOverlapDoesNotDeductRestTwice()
    {
        var week = Assert.Single(OvertimeCalculator.Calculate([Entry(5, 45, (480, 600))], Monday, Monday.AddDays(6), new HashSet<DateTime> { Monday.AddDays(5) }));
        Assert.Equal(15, week.RestMinutes);
    }

    [Fact]
    public void SeparateIntervalsOnWeekdayHolidayAreCombinedBeforeRest()
    {
        var week = Assert.Single(OvertimeCalculator.Calculate([Entry(3, 45, (480, 660), (780, 960))], Monday, Monday.AddDays(6), new HashSet<DateTime> { Monday.AddDays(3) }));
        Assert.Equal(30, week.RestMinutes);
        Assert.Equal(330, week.OvertimeMinutes);
    }

    [Fact]
    public void FortyHourGroup_FirstFiveExtraHoursDoNotEarnOvertime()
    {
        var week = Assert.Single(OvertimeCalculator.Calculate([Entry(0, 40, (1020, 1320))], Monday, Monday.AddDays(6)));
        Assert.Equal(0, week.OvertimeMinutes);
    }

    [Fact]
    public void WeeklyShortfall_CannotOffsetAnotherWeeksOvertime()
    {
        var weeks = OvertimeCalculator.Calculate([Entry(0, 40, (1020, 1140)), Entry(7, 40, (900, 1320))], Monday, Monday.AddDays(13));
        Assert.Equal([0, 120], weeks.Select(x => x.OvertimeMinutes));
    }

    [Fact]
    public void RoundingIsAppliedOnceAfterSummingWeeks()
    {
        var weeks = OvertimeCalculator.Calculate([Entry(0, 45, (1020, 1150)), Entry(7, 45, (1020, 1205))], Monday, Monday.AddDays(13));
        Assert.Equal(315, weeks.Sum(x => x.OvertimeMinutes));
        Assert.Equal(330, OvertimeCalculator.RoundTotal(weeks.Sum(x => x.OvertimeMinutes)));
        Assert.NotEqual(330, weeks.Sum(x => OvertimeCalculator.RoundTotal(x.OvertimeMinutes)));
    }

    [Fact]
    public void WeekMayCrossMonthBoundary()
    {
        var week = Assert.Single(OvertimeCalculator.Calculate([Entry(4, 45, (1020, 1080)), Entry(6, 45, (600, 720))], Monday, Monday.AddDays(6)));
        Assert.Equal(Monday, week.Monday);
        Assert.Equal(165, week.OvertimeMinutes);
    }

    [Fact]
    public void PartialWeeksAreRejected()
        => Assert.Throws<ArgumentException>(() => OvertimeCalculator.Calculate([], Monday.AddDays(1), Monday.AddDays(6)));

    [Fact]
    public void EntriesOutsidePeriodAreRejected()
        => Assert.Throws<ArgumentException>(() => OvertimeCalculator.Calculate([Entry(7, 45, (1020, 1080))], Monday, Monday.AddDays(6)));

    [Fact]
    public void InconsistentWorkGroupsWithinWeekRequireHrReview()
        => Assert.Throws<ArgumentException>(() => OvertimeCalculator.Calculate([Entry(0, 40, (1020, 1080)), Entry(1, 45, (1020, 1080))], Monday, Monday.AddDays(6)));

    [Fact]
    public void CalendarDaysAtMidnightKeepCorrectWeekOwnership()
    {
        var week = Assert.Single(OvertimeCalculator.Calculate([Entry(6, 45, (1380, 1440))], Monday, Monday.AddDays(6)));
        Assert.Equal(60, week.GrossMinutes);
        Assert.Equal(45, week.OvertimeMinutes);
    }
}
