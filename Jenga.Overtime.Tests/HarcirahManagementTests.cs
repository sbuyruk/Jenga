using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Jenga.Overtime.Tests;

public sealed class HarcirahManagementTests
{
    [Fact]
    public async Task FiltersDefaultToTurkeyAndLatestSeriesAndKeepHistoricalPeriods()
    {
        await using var data = await Database.CreateAsync();
        var page = await data.Service().GetAsync();
        Assert.True(page.IsSuccess, page.Error.Message);
        Assert.Equal("Türkiye", page.Value.Country);
        Assert.Equal(2, page.Value.SelectedSeriesId);
        Assert.Equal(new[] { 2, 1 }, page.Value.Series.Select(x => x.Id));
        Assert.Equal(2, page.Value.Rows.Count);
        Assert.All(page.Value.Rows, x => Assert.Equal(2, x.SeriId));
        Assert.Contains("Devam ediyor", page.Value.Series[0].Label);
        var old = await data.Service().GetAsync(" türkiye ", 1);
        Assert.True(old.IsSuccess);
        Assert.All(old.Value.Rows, x => Assert.Equal(1, x.SeriId));
        Assert.True((await data.Service().GetAsync("Türkiye", 7)).IsFailure);
        Assert.True((await data.Service().GetAsync("Tanımsız")).IsFailure);
    }

    [Fact]
    public async Task AllOperationsRequireManageEvenForUsersWithViewAndEdit()
    {
        await using var data = await Database.CreateAsync();
        var service = data.Service(manage: false);
        Assert.True((await service.GetAsync()).IsFailure);
        Assert.True((await service.UpdateAsync(new(3, 500, "Yeni", 100, null))).IsFailure);
        Assert.True((await service.UndoSeriesAsync(new("Türkiye", 2, "version"))).IsFailure);
        Assert.True((await service.CreateSeriesAsync(new("Türkiye", new(2026, 1, 1), 2))).IsFailure);
        await using var db = data.CreateDbContext();
        Assert.Equal(7, await db.Harcirah_Table.CountAsync());
        Assert.Equal(100m, (await db.Harcirah_Table.SingleAsync(x => x.Id == 3)).Miktar);
    }

    [Fact]
    public async Task UpdateChangesOnlyAmountDescriptionAndAuditAndRejectsStaleWrites()
    {
        await using var data = await Database.CreateAsync();
        Assert.True((await data.Service().UpdateAsync(new(3, 125.50m, "Yeni tutar", 100m, null))).IsSuccess);
        await using var db = data.CreateDbContext();
        var row = await db.Harcirah_Table.AsNoTracking().SingleAsync(x => x.Id == 3);
        Assert.Equal(125.50m, row.Miktar);
        Assert.Equal("Yeni tutar", row.Aciklama);
        Assert.Equal("admin", row.Degistiren);
        Assert.Equal("seed", row.Olusturan);
        Assert.Equal("TL", row.ParaBirimi);
        Assert.Equal(2, row.SeriId);
        Assert.Equal(new DateTime(2025, 7, 1), row.BaslangicTarihi);
        Assert.Null(row.BitisTarihi);
        Assert.True((await data.Service().UpdateAsync(new(3, 200m, "Eski ekran", 100m, null))).IsFailure);
        Assert.Equal(125.50m, (await db.Harcirah_Table.AsNoTracking().SingleAsync(x => x.Id == 3)).Miktar);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1.234)]
    public async Task InvalidAmountsCannotBeSaved(double amount)
    {
        await using var data = await Database.CreateAsync();
        Assert.True((await data.Service().UpdateAsync(new(3, (decimal)amount, null, 100m, null))).IsFailure);
    }

    [Theory]
    [InlineData("Türkiye", 2, "TL", 2)]
    [InlineData("İngiltere", 7, "Sterlin", 1)]
    [InlineData("Avrupa", 3, "Euro", 1)]
    [InlineData("Avrupa Harici", 3, "USD", 1)]
    public async Task NewSeriesClosesOnlySelectedCountryAndCopiesLatestGroupDefinitions(string country, int previousSeries, string currency, int count)
    {
        await using var data = await Database.CreateAsync();
        var result = await data.Service().CreateSeriesAsync(new(country, new(2026, 1, 1), previousSeries));
        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.Equal(count, result.Value.Count);
        Assert.Equal(previousSeries + 1, result.Value.SeriesId);
        await using var db = data.CreateDbContext();
        var rows = await db.Harcirah_Table.Where(x => x.Ulke == country).ToListAsync();
        Assert.All(rows.Where(x => x.SeriId == previousSeries), x => Assert.Equal(new DateTime(2025, 12, 31), x.BitisTarihi));
        Assert.All(rows.Where(x => x.SeriId == result.Value.SeriesId), x =>
        {
            Assert.Equal(currency, x.ParaBirimi); Assert.Equal(0m, x.Miktar); Assert.Null(x.BitisTarihi);
            Assert.Equal("admin", x.Olusturan); Assert.Equal("admin", x.Degistiren);
        });
        Assert.All(await db.Harcirah_Table.Where(x => x.Ulke != country && x.SeriId != 1).ToListAsync(), x => Assert.Null(x.BitisTarihi));
        if (country == "Türkiye")
        {
            var group = rows.Single(x => x.SeriId == result.Value.SeriesId && x.KadroGrupId == 1);
            Assert.Equal("Yeni Grup 1", group.KadroGrubu);
            Assert.Equal(11, group.Sira);
            Assert.All(rows.Where(x => x.SeriId == 1), x => Assert.Equal(new DateTime(2025, 6, 30), x.BitisTarihi));
        }
    }

    [Fact]
    public async Task DuplicateBackdatedAndStaleSeriesRequestsCannotAlterExistingPeriods()
    {
        await using var data = await Database.CreateAsync();
        var service = data.Service();
        Assert.True((await service.CreateSeriesAsync(new("Türkiye", new(2025, 7, 1), 2))).IsFailure);
        Assert.True((await service.CreateSeriesAsync(new("Türkiye", new(2026, 1, 1), 1))).IsFailure);
        Assert.True((await service.CreateSeriesAsync(new("Tanımsız", new(2026, 1, 1), 0))).IsFailure);
        Assert.True((await service.CreateSeriesAsync(new("Türkiye", DateTime.MinValue, 2))).IsFailure);
        Assert.True((await service.CreateSeriesAsync(new("Türkiye", new(2026, 1, 1), 2))).IsSuccess);
        Assert.True((await service.CreateSeriesAsync(new("Türkiye", new(2026, 1, 1), 3))).IsFailure);
        Assert.True((await service.CreateSeriesAsync(new("Türkiye", new(2027, 1, 1), 2))).IsFailure);
        await using var db = data.CreateDbContext();
        Assert.Equal(9, await db.Harcirah_Table.CountAsync());
        Assert.Equal(2, await db.Harcirah_Table.CountAsync(x => x.Ulke == "Türkiye" && x.BitisTarihi == null));
    }

    [Fact]
    public async Task InsertFailureRollsBackPriorSeriesClosureAndEveryNewRow()
    {
        await using var data = await Database.CreateAsync();
        await using var db = data.CreateDbContext();
        await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER RejectNewTariff BEFORE INSERT ON Harcirah_Table WHEN NEW.SeriId = 3 AND NEW.Ulke = 'Türkiye' AND NEW.KadroGrupId = 2 BEGIN SELECT RAISE(ABORT, 'test failure'); END;");
        Assert.True((await data.Service().CreateSeriesAsync(new("Türkiye", new(2026, 1, 1), 2))).IsFailure);
        Assert.Equal(7, await db.Harcirah_Table.CountAsync());
        Assert.All(await db.Harcirah_Table.AsNoTracking().Where(x => x.Ulke == "Türkiye" && x.SeriId == 2).ToListAsync(), x => Assert.Null(x.BitisTarihi));
    }

    [Fact]
    public async Task ManagedRatesAreUsedAcrossSeriesBoundaryByTaskCalculator()
    {
        await using var data = await Database.CreateAsync();
        var created = await data.Service().CreateSeriesAsync(new("Türkiye", new(2026, 1, 1), 2));
        Assert.True(created.IsSuccess);
        await using var db = data.CreateDbContext();
        var row = await db.Harcirah_Table.AsNoTracking().SingleAsync(x => x.Ulke == "Türkiye" && x.SeriId == created.Value.SeriesId && x.KadroGrupId == 1);
        Assert.True((await data.Service().UpdateAsync(new(row.Id, 300, null, 0, null))).IsSuccess);
        var rates = await db.Harcirah_Table.AsNoTracking().Where(x => x.Ulke == "Türkiye" && x.KadroGrupId == 1).ToListAsync();
        var result = TaskApprovalAllowanceCalculator.Calculate(new()
        {
            StartDate = new(2025, 12, 31), StartTime = "20:00", EndDate = new(2026, 1, 1), EndTime = "09:00", Country = "Türkiye"
        }, rates);
        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal(200m, result.TotalAllowance);
    }

    [Fact]
    public async Task UndoRemovesEveryFutureRowAndReopensPreviousSeriesWithoutChangingOtherHistory()
    {
        await using var data = await Database.CreateAsync();
        var request = await PrepareUndoAsync(data);
        var result = await data.Service().UndoSeriesAsync(request);
        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.Equal(new HarcirahUndoSeriesResult(2, 2), result.Value);
        await using var db = data.CreateDbContext();
        Assert.Equal(7, await db.Harcirah_Table.CountAsync());
        Assert.All(await db.Harcirah_Table.Where(x => x.Ulke == "Türkiye" && x.SeriId == 2).ToListAsync(), x =>
        { Assert.Null(x.BitisTarihi); Assert.Equal("admin", x.Degistiren); });
        Assert.All(await db.Harcirah_Table.Where(x => x.SeriId == 1).ToListAsync(), x => Assert.Equal(new DateTime(2025, 6, 30), x.BitisTarihi));
        Assert.All(await db.Harcirah_Table.Where(x => x.Ulke != "Türkiye").ToListAsync(), x => Assert.Null(x.BitisTarihi));
        var rates = await db.Harcirah_Table.Where(x => x.Ulke == "Türkiye" && x.KadroGrupId == 1).ToListAsync();
        var calculation = TaskApprovalAllowanceCalculator.Calculate(new()
        { StartDate = DateTime.Today.AddDays(31), EndDate = DateTime.Today.AddDays(31), StartTime = "08:00", EndTime = "17:00", Country = "Türkiye" }, rates);
        Assert.True(calculation.IsValid, calculation.ErrorMessage);
        Assert.Equal(50m, calculation.TotalAllowance);
        Assert.True((await data.Service().UndoSeriesAsync(request)).IsFailure);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task TodayAndPastPeriodsCannotBeUndone(int offset)
    {
        await using var data = await Database.CreateAsync();
        Assert.True((await data.Service().CreateSeriesAsync(new("Türkiye", DateTime.Today.AddDays(offset), 2))).IsSuccess);
        var page = await data.Service().GetAsync("Türkiye", 3);
        Assert.False(page.Value.Undo.CanUndo);
        Assert.True((await data.Service().UndoSeriesAsync(new("Türkiye", 3, "forged"))).IsFailure);
        await using var db = data.CreateDbContext();
        Assert.Equal(9, await db.Harcirah_Table.CountAsync());
    }

    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, false, false)]
    [InlineData(2, false, false)]
    [InlineData(3, false, false)]
    [InlineData(4, false, false)]
    [InlineData(0, true, false)]
    [InlineData(0, false, true)]
    public async Task AllApprovalStatesAndUnknownDatesBlockUndoEvenAfterConfirmationWasOpened(int status, bool missingStart, bool missingEnd)
    {
        await using var data = await Database.CreateAsync();
        var request = await PrepareUndoAsync(data);
        var start = DateTime.Today.AddDays(30);
        await using var db = data.CreateDbContext();
        db.GorevOnay_Table.Add(new()
        {
            AmirOnayi = status, BaslangicTarihi = missingStart ? null : start.AddDays(-1),
            BitisTarihi = missingEnd ? null : start, ParaBirimi = "Euro", Odendi = status == 1
        });
        await db.SaveChangesAsync();
        Assert.False((await data.Service().GetAsync("Türkiye", 3)).Value.Undo.CanUndo);
        Assert.True((await data.Service().UndoSeriesAsync(request)).IsFailure);
        Assert.Equal(9, await db.Harcirah_Table.CountAsync());
        Assert.All(await db.Harcirah_Table.Where(x => x.Ulke == "Türkiye" && x.SeriId == 2).ToListAsync(), x => Assert.Equal(start.AddDays(-1), x.BitisTarihi));
    }

    [Fact]
    public async Task TaskEndingBeforePeriodDoesNotPreventUndo()
    {
        await using var data = await Database.CreateAsync();
        await using var db = data.CreateDbContext();
        db.GorevOnay_Table.Add(new() { BaslangicTarihi = DateTime.Today, BitisTarihi = DateTime.Today.AddDays(30).AddTicks(-1) });
        await db.SaveChangesAsync();
        var request = await PrepareUndoAsync(data);
        Assert.True((await data.Service().UndoSeriesAsync(request)).IsSuccess);
        Assert.Equal(1, await db.GorevOnay_Table.CountAsync());
    }

    [Fact]
    public async Task OlderPeriodCannotBeUndoneAfterAnotherPeriodIsCreated()
    {
        await using var data = await Database.CreateAsync();
        var request = await PrepareUndoAsync(data);
        Assert.True((await data.Service().CreateSeriesAsync(new("Türkiye", DateTime.Today.AddDays(60), 3))).IsSuccess);
        Assert.False((await data.Service().GetAsync("Türkiye", 3)).Value.Undo.CanUndo);
        Assert.True((await data.Service().UndoSeriesAsync(request)).IsFailure);
        await using var db = data.CreateDbContext();
        Assert.Equal(11, await db.Harcirah_Table.CountAsync());
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task StaleSnapshotCannotUndoModifiedCurrentOrPreviousPeriod(int series)
    {
        await using var data = await Database.CreateAsync();
        var request = await PrepareUndoAsync(data);
        await using var db = data.CreateDbContext();
        var row = await db.Harcirah_Table.SingleAsync(x => x.Ulke == "Türkiye" && x.SeriId == series && x.KadroGrupId == 1);
        Assert.True((await data.Service().UpdateAsync(new(row.Id, row.Miktar + 10, "Değişti", row.Miktar, row.Aciklama))).IsSuccess);
        Assert.True((await data.Service().UndoSeriesAsync(request)).IsFailure);
        Assert.NotEqual(request.Version, (await data.Service().GetAsync("Türkiye", 3)).Value.Undo.Version);
        Assert.Equal(9, await db.Harcirah_Table.CountAsync());
    }

    [Fact]
    public async Task InconsistentPreviousClosureCannotBeReopened()
    {
        await using var data = await Database.CreateAsync();
        var request = await PrepareUndoAsync(data);
        await using var db = data.CreateDbContext();
        var row = await db.Harcirah_Table.SingleAsync(x => x.Id == 3);
        row.BitisTarihi = DateTime.Today.AddDays(20); await db.SaveChangesAsync();
        Assert.False((await data.Service().GetAsync("Türkiye", 3)).Value.Undo.CanUndo);
        Assert.True((await data.Service().UndoSeriesAsync(request)).IsFailure);
        Assert.Equal(9, await db.Harcirah_Table.CountAsync());
    }

    [Fact]
    public async Task DeleteFailureRollsBackWholePeriodAndPreviousReopening()
    {
        await using var data = await Database.CreateAsync();
        var request = await PrepareUndoAsync(data);
        await using var db = data.CreateDbContext();
        await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER RejectTariffUndo BEFORE DELETE ON Harcirah_Table WHEN OLD.SeriId = 3 AND OLD.Ulke = 'Türkiye' AND OLD.KadroGrupId = 2 BEGIN SELECT RAISE(ABORT, 'test failure'); END;");
        Assert.True((await data.Service().UndoSeriesAsync(request)).IsFailure);
        Assert.Equal(9, await db.Harcirah_Table.CountAsync());
        Assert.All(await db.Harcirah_Table.Where(x => x.Ulke == "Türkiye" && x.SeriId == 2).ToListAsync(), x => Assert.Equal(DateTime.Today.AddDays(29), x.BitisTarihi));
    }

    [Fact]
    public async Task EmptyVersionCannotUndoOtherwiseEligiblePeriod()
    {
        await using var data = await Database.CreateAsync();
        var request = await PrepareUndoAsync(data);
        Assert.True((await data.Service().UndoSeriesAsync(request with { Version = "" })).IsFailure);
    }

    private static async Task<HarcirahUndoSeriesRequest> PrepareUndoAsync(Database data)
    {
        var created = await data.Service().CreateSeriesAsync(new("Türkiye", DateTime.Today.AddDays(30), 2));
        Assert.True(created.IsSuccess, created.Error.Message);
        var page = await data.Service().GetAsync("Türkiye", created.Value.SeriesId);
        Assert.True(page.IsSuccess, page.Error.Message);
        Assert.True(page.Value.Undo.CanUndo, page.Value.Undo.Message);
        return new("Türkiye", created.Value.SeriesId, page.Value.Undo.Version);
    }

    private sealed class Auth(bool manage) : IServiceAuthorizationContext
    {
        public Task<bool> HasPermissionAsync(ModuleName module, Operation operation, CancellationToken cancellationToken = default) => Task.FromResult(operation != Operation.Manage || manage);
        public Task<string?> GetUserNameAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>("admin");
        public Task<IReadOnlyList<int>> GetAuthorizedRegionIdsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<int>>([]);
    }

    private sealed class Database : IDbContextFactory<ApplicationDbContext>, IAsyncDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        public ApplicationDbContext CreateDbContext() => new Context(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        public HarcirahService Service(bool manage = true) => new(this, new Auth(manage), NullLogService.Instance);
        public static async Task<Database> CreateAsync()
        {
            var data = new Database(); await data._connection.OpenAsync();
            await using var db = data.CreateDbContext(); await db.Database.EnsureCreatedAsync();
            db.Harcirah_Table.AddRange(
                Rate(1, "Türkiye", 1, 1, 10), Rate(2, "Türkiye", 1, 2, 20), Rate(3, "Türkiye", 2, 1, 100), Rate(4, "Türkiye", 2, 2, 200),
                Rate(5, "İngiltere", 7, 1, 30, "Sterlin"), Rate(6, "Avrupa", 3, 1, 40, "Euro"), Rate(7, "Avrupa Harici", 3, 1, 50, "USD"));
            await db.SaveChangesAsync(); return data;
        }
        private static Harcirah Rate(int id, string country, int series, int group, decimal amount, string currency = "TL") => new()
        {
            Id = id, Ulke = country, SeriId = series, KadroGrupId = group, KadroGrubu = (series == 1 ? "Eski" : "Yeni") + " Grup " + group,
            Sira = series == 1 ? group : group + 10, Miktar = amount, ParaBirimi = currency, Olusturan = "seed",
            BaslangicTarihi = series == 2 ? new(2025, 7, 1) : new(2025, 1, 1), BitisTarihi = series == 1 ? new DateTime(2025, 6, 30) : null
        };
        public ValueTask DisposeAsync() => _connection.DisposeAsync();
        private sealed class Context(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
        {
            protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);
                foreach (var type in builder.Model.GetEntityTypes().Select(x => x.ClrType).Where(x => x != typeof(Harcirah) && x != typeof(GorevOnay)).ToList()) builder.Ignore(type);
            }
        }
    }
}
