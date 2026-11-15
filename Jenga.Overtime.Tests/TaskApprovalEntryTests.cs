using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Jenga.Overtime.Tests;

public sealed class TaskApprovalEntryTests
{
    [Theory]
    [InlineData(1, 0, 0.5)]
    [InlineData(12, 0, 0.5)]
    [InlineData(12, 1, 1)]
    [InlineData(24, 0, 1)]
    [InlineData(24, 1, 1.5)]
    public void AllowanceRoundsAtTwelveHoursAndCountsPartialMinutes(int hours, int minutes, double days)
    {
        var start = new DateTime(2026, 11, 15, 0, 0, 0);
        var result = TaskApprovalAllowanceCalculator.Calculate(Input(start, start.AddHours(hours).AddMinutes(minutes)), [Rate(1, 100)]);
        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal((decimal)days, result.EarnedDays);
        Assert.Equal((decimal)days * 100, result.TotalAllowance);
    }

    [Fact]
    public void SeriesChangesUseActualTimesAndSupportMoreThanTwoRates()
    {
        var first = Rate(1, 100); first.BitisTarihi = new DateTime(2026, 12, 31);
        var second = Rate(2, 200); second.BaslangicTarihi = new(2027, 1, 1);
        var result = TaskApprovalAllowanceCalculator.Calculate(Input(new(2026, 12, 31, 20, 0, 0), new(2027, 1, 1, 9, 0, 0)), [first, second]);
        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal(150m, result.TotalAllowance);
        Assert.True(result.HasMultipleRates);
        Assert.Equal(0m, result.DailyAllowance);
        second.BitisTarihi = new DateTime(2027, 1, 1);
        var third = Rate(3, 300); third.BaslangicTarihi = new(2027, 1, 2);
        result = TaskApprovalAllowanceCalculator.Calculate(Input(new(2026, 12, 31, 8, 0, 0), new(2027, 1, 2, 8, 0, 0)), [first, second, third]);
        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal(450m, result.TotalAllowance);
    }

    [Fact]
    public void MissingRatesGapsCurrenciesAndInvalidDatesBlockCalculation()
    {
        var input = Input(new(2026, 11, 15, 8, 0, 0), new(2026, 11, 15, 17, 0, 0));
        Assert.False(TaskApprovalAllowanceCalculator.Calculate(input, []).IsValid);
        Assert.False(TaskApprovalAllowanceCalculator.Calculate(Input(new(2026, 11, 15), new(2026, 11, 15)), [Rate(1, 100)]).IsValid);
        Assert.False(TaskApprovalAllowanceCalculator.Calculate(Input(new(2026, 11, 15), new(2027, 1, 1)), [Rate(1, 100)]).IsValid);
        var first = Rate(1, 100); first.BitisTarihi = new DateTime(2026, 11, 15);
        var next = Rate(2, 200); next.BaslangicTarihi = new(2026, 11, 17);
        input = Input(new(2026, 11, 15, 8, 0, 0), new(2026, 11, 17, 17, 0, 0));
        Assert.False(TaskApprovalAllowanceCalculator.Calculate(input, [first, next]).IsValid);
        next.BaslangicTarihi = new(2026, 11, 16); next.ParaBirimi = "EUR";
        Assert.False(TaskApprovalAllowanceCalculator.Calculate(input, [first, next]).IsValid);
    }

    [Fact]
    public async Task SaveUsesDatabaseRatesAndApprovalGroupAndSendsSeparateNotifications()
    {
        await using var data = await EntryDatabase.CreateAsync();
        var input = Request(1);
        input.Model.Sure = "999 gün"; input.Model.Yevmiye = "999999";
        input.Model.AmirOnayi = 1; input.Model.Odendi = true;
        var result = await data.Service().SaveAsync(input);
        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.Null(result.Value.Warning);
        await using var db = data.CreateDbContext();
        var saved = await db.GorevOnay_Table.SingleAsync(x => x.Id == result.Value.Id);
        Assert.Equal("150,00", saved.Yevmiye);
        Assert.Equal("300,00", saved.GunlukYevmiye);
        Assert.Equal("0,5 gün", saved.Sure);
        Assert.Equal(0, saved.AmirOnayi);
        Assert.False(saved.Odendi);
        Assert.Equal("actor", saved.Olusturan);
        Assert.Equal(2, data.Email.Messages.Count);
        Assert.Contains(data.Email.Messages, x => x.Recipients.SequenceEqual(["worker@example.test"]) && x.Body.Contains("/ikys/task-approvals/personal"));
        Assert.Contains(data.Email.Messages, x => x.Recipients.Contains("manager@example.test") && x.Recipients.Contains("group@example.test"));
        saved.GorevinSebebi = "Güncellendi";
        var update = Request(1); update.Model = saved;
        Assert.True((await data.Service().SaveAsync(update)).IsSuccess);
        Assert.Equal(2, data.Email.Messages.Count);
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(3, 3)]
    [InlineData(1, 0)]
    public async Task DatabaseGroupDeterminesWhetherManagerApprovalIsRequired(int personId, int expectedStatus)
    {
        await using var data = await EntryDatabase.CreateAsync();
        var request = Request(personId); request.CurrentPersonelId = 4; request.Auth = "IKYS";
        var result = await data.Service(manage: true).SaveAsync(request);
        Assert.True(result.IsSuccess, result.Error.Message);
        await using var db = data.CreateDbContext();
        Assert.Equal(expectedStatus, (await db.GorevOnay_Table.SingleAsync(x => x.Id == result.Value.Id)).AmirOnayi);
        Assert.DoesNotContain(data.Email.Messages, x => x.Recipients.Contains("group@example.test"));
        Assert.Contains(data.Email.Messages, x => x.Body.Contains("/ikys/task-approvals/personal"));
    }

    [Fact]
    public async Task ScopeAndSignatureListsExcludeForeignUnitsInactiveAndNonStaffPeople()
    {
        await using var data = await EntryDatabase.CreateAsync();
        var hr = await data.Service(manage: true).LoadAsync(new() { CurrentPersonelId = 4, Auth = "IKYS" });
        Assert.True(hr.IsSuccess, hr.Error.Message);
        Assert.DoesNotContain(hr.Value.PersonOptions, x => x.Id is 7 or 8);
        Assert.Equal(new[] { 4 }, hr.Value.PerDirOptions.Select(x => x.Id));
        Assert.Equal(new[] { 5 }, hr.Value.PerUzmOptions.Select(x => x.Id));
        Assert.Equal(8, hr.Value.Model.BaslangicTarihi!.Value.Hour);
        var unit = await data.Service().LoadAsync(new() { CurrentPersonelId = 2, Auth = "BIRIM", QueryBirimTanimId = 1 });
        Assert.True(unit.IsSuccess, unit.Error.Message);
        Assert.Contains(unit.Value.PersonOptions, x => x.Id == 3);
        Assert.DoesNotContain(unit.Value.PersonOptions, x => x.Id == 6);
        var foreign = await data.Service().LoadAsync(new() { CurrentPersonelId = 2, Auth = "BIRIM", QueryBirimTanimId = 9 });
        Assert.Empty(foreign.Value.PersonOptions);
        Assert.True((await data.Service().LoadAsync(new() { CurrentPersonelId = 1, CanManage = true, Auth = "IKYS" })).IsFailure);
        var request = Request(6); request.Model.OnayImza = 6;
        Assert.True((await data.Service().SaveAsync(request)).IsFailure);
        var foreignCalculation = Input(DateTime.Today.AddHours(8), DateTime.Today.AddHours(17));
        foreignCalculation.CurrentPersonelId = 2; foreignCalculation.PersonelId = 6; foreignCalculation.Auth = "BIRIM";
        Assert.False((await data.Service().CalculateAsync(foreignCalculation)).IsValid);
        request = Request(1); request.Model.OnayImza = 6;
        Assert.True((await data.Service().SaveAsync(request)).IsFailure);
    }

    [Fact]
    public async Task ManagedUnitCreationRejectsSpoofedOwnershipAndDoesNotGrantGlobalAccess()
    {
        await using var data = await EntryDatabase.CreateAsync();
        var request = Request(3); request.CurrentPersonelId = 2; request.Auth = "BIRIM";
        Assert.True((await data.Service().SaveAsync(request)).IsSuccess);
        request = Request(6); request.CurrentPersonelId = 2; request.Auth = "BIRIM";
        Assert.True((await data.Service().SaveAsync(request)).IsFailure);
        request = Request(3); request.CurrentPersonelId = 1;
        Assert.True((await data.Service().SaveAsync(request)).IsFailure);
    }

    [Fact]
    public async Task NotificationFailureDoesNotUndoSaveAndPaidRecordCannotBeChangedOrDeleted()
    {
        await using var data = await EntryDatabase.CreateAsync();
        data.Email.Fail = true;
        var result = await data.Service().SaveAsync(Request(1));
        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.NotNull(result.Value.Warning);
        await using var db = data.CreateDbContext();
        var entity = await db.GorevOnay_Table.SingleAsync(x => x.Id == result.Value.Id);
        entity.Odendi = true; await db.SaveChangesAsync();
        var update = Request(1); update.Model.Id = entity.Id;
        Assert.True((await data.Service().SaveAsync(update)).IsFailure);
        Assert.True((await data.Service().DeleteAsync(entity.Id, 1, null)).IsFailure);
        var loaded = await data.Service(manage: true).LoadAsync(new() { GorevOnayId = entity.Id, Auth = "IKYS", CurrentPersonelId = 4 });
        Assert.True(loaded.IsSuccess, loaded.Error.Message);
        Assert.False(loaded.Value.ShowUpdate);
        Assert.False(loaded.Value.ShowDelete);
        Assert.True(loaded.Value.IsPaid);
    }

    [Fact]
    public async Task MissingDefinitionsBlockSaveAndRejectedTasksDoNotBlockNewDates()
    {
        await using var data = await EntryDatabase.CreateAsync();
        await using var db = data.CreateDbContext();
        db.GorevOnay_Table.Add(new() { PersonelId = 1, AmirOnayi = 2, BaslangicTarihi = DateTime.Today, BitisTarihi = DateTime.Today.AddDays(1) });
        await db.SaveChangesAsync();
        Assert.True((await data.Service().SaveAsync(Request(1))).IsSuccess);
        Assert.True((await data.Service().SaveAsync(Request(1))).IsFailure);
        await db.Harcirah_Table.ExecuteDeleteAsync();
        var next = Request(1); next.Model.BaslangicTarihi = DateTime.Today.AddDays(2).AddHours(8); next.Model.BitisTarihi = DateTime.Today.AddDays(2).AddHours(17);
        Assert.True((await data.Service().SaveAsync(next)).IsFailure);
    }

    [Fact]
    public async Task DisablingAllowanceKeepsDurationAndRateButSavesZeroPayment()
    {
        await using var data = await EntryDatabase.CreateAsync();
        var request = Request(1); request.CalculateAllowance = false;
        var result = await data.Service().SaveAsync(request);
        Assert.True(result.IsSuccess, result.Error.Message);
        await using var db = data.CreateDbContext();
        var saved = await db.GorevOnay_Table.SingleAsync(x => x.Id == result.Value.Id);
        Assert.Equal("0,00", saved.Yevmiye);
        Assert.Equal("300,00", saved.GunlukYevmiye);
        Assert.Equal("0,5 gün", saved.Sure);
    }

    [Fact]
    public void OpenEndedTariffAppliesUntilNewerTariffStarts()
    {
        var first = Rate(1, 100); first.BitisTarihi = null;
        var next = Rate(2, 200); next.BaslangicTarihi = new(2027, 1, 1); next.BitisTarihi = null;
        var result = TaskApprovalAllowanceCalculator.Calculate(Input(new(2026, 12, 31, 20, 0, 0), new(2027, 1, 1, 9, 0, 0)), [first, next]);
        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal(150m, result.TotalAllowance);
        Assert.True(result.HasMultipleRates);
        result = TaskApprovalAllowanceCalculator.Calculate(Input(new(2036, 1, 1, 8, 0, 0), new(2036, 1, 1, 17, 0, 0)), [first, next]);
        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal(100m, result.TotalAllowance);
    }

    [Fact]
    public async Task NullTariffEndDateCanBeLoadedCalculatedAndSaved()
    {
        await using var data = await EntryDatabase.CreateAsync();
        await using var db = data.CreateDbContext();
        await db.Harcirah_Table.ExecuteUpdateAsync(x => x.SetProperty(r => r.BitisTarihi, (DateTime?)null));
        var service = data.Service();
        var loaded = await service.LoadAsync(new() { CurrentPersonelId = 1 });
        Assert.True(loaded.IsSuccess, loaded.Error.Message);
        var input = Input(DateTime.Today.AddHours(8), DateTime.Today.AddHours(17));
        input.CurrentPersonelId = 1;
        var calculation = await service.CalculateAsync(input);
        Assert.True(calculation.IsValid, calculation.ErrorMessage);
        Assert.Equal(150m, calculation.TotalAllowance);
        var saved = await service.SaveAsync(Request(1));
        Assert.True(saved.IsSuccess, saved.Error.Message);
        Assert.Equal("150,00", (await db.GorevOnay_Table.SingleAsync(x => x.Id == saved.Value.Id)).Yevmiye);
    }

    private static Harcirah Rate(int id, decimal amount) => new()
    {
        Id = id, SeriId = id, KadroGrupId = 3, KadroGrubu = "Grup 3", Ulke = "Türkiye", Miktar = amount,
        ParaBirimi = "TL", BaslangicTarihi = new(2020, 1, 1), BitisTarihi = new DateTime(2035, 12, 31)
    };
    private static TaskApprovalCalculationInput Input(DateTime start, DateTime end) => new()
    {
        PersonelId = 1, StartDate = start, EndDate = end, StartTime = start.ToString("HH:mm"), EndTime = end.ToString("HH:mm")
    };
    private static TaskApprovalEntrySaveRequest Request(int owner) => new()
    {
        CurrentPersonelId = 1, Model = new()
        {
            PersonelId = owner, GorevinSebebi = "Görev", GorevinYeri = "Ankara", Konaklama = "Limit Dahilinde",
            BaslangicTarihi = DateTime.Today.AddHours(8), BitisTarihi = DateTime.Today.AddHours(17)
        }
    };

    private sealed class Auth(bool manage) : IServiceAuthorizationContext
    {
        public Task<bool> HasPermissionAsync(ModuleName module, Operation operation, CancellationToken cancellationToken = default)
            => Task.FromResult(module == ModuleName.IKYS && (operation != Operation.Manage || manage));
        public Task<string?> GetUserNameAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>("actor");
        public Task<IReadOnlyList<int>> GetAuthorizedRegionIdsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<int>>([]);
    }
    private sealed class Mail : IEmailService
    {
        public bool Fail;
        public List<(string[] Recipients, string Body)> Messages { get; } = [];
        public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default) => SendAsync([to], subject, body, cancellationToken);
        public Task SendAsync(IEnumerable<string> recipients, string subject, string body, CancellationToken cancellationToken = default)
        {
            if (Fail) throw new InvalidOperationException("SMTP test error");
            Messages.Add((recipients.ToArray(), body)); return Task.CompletedTask;
        }
    }
    private sealed class EntryDatabase : IDbContextFactory<ApplicationDbContext>, IAsyncDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        public Mail Email { get; } = new();
        public ApplicationDbContext CreateDbContext() => new Context(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        public TaskApprovalEntryService Service(bool manage = false)
        {
            var auth = new Auth(manage);
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["TaskApproval:BaseUrl"] = "https://jenga.example.test", ["TaskApproval:NotificationGroup"] = "group@example.test" }).Build();
            return new(this, NullLogService.Instance, new GorevOnayService(this, NullLogService.Instance, Email, auth), auth, Email, config);
        }
        public static async Task<EntryDatabase> CreateAsync()
        {
            var data = new EntryDatabase(); await data._connection.OpenAsync();
            await using var db = data.CreateDbContext(); await db.Database.EnsureCreatedAsync();
            db.Personel_Table.AddRange(Enumerable.Range(1, 8).Select(i => new Personel
            { Id = i, Adi = "Personel", Soyadi = i.ToString(), SicilNo = i, Tipi = i == 7 ? PersonelTipi.KadroHarici : PersonelTipi.Kadrolu }));
            db.BirimTanim_Table.AddRange(new[] { (1, 0, 2), (5, 0, 4), (6, 1, 2), (9, 0, 6) }.Select(x => new BirimTanim
            { Id = x.Item1, ParentId = x.Item2, AmirId = x.Item3, Adi = "Birim", KisaAdi = "B", Aktif = true, BolgeId = 1, Sira = 1 }));
            db.UnvanTanim_Table.Add(new() { Id = 1, Adi = "Uzman", GorTipId = 1 });
            await db.SaveChangesAsync();
            foreach (var i in Enumerable.Range(1, 8))
            {
                var unit = i is 4 or 5 ? 5 : i == 3 ? 6 : i == 6 ? 9 : 1;
                db.IsBilgileri_Table.Add(new() { PersonelId = i, BirimId = unit, UnvanId = 1, GorevId = i, CalismaDurumu = i == 8 ? CalismaDurumu.Ayrildi : CalismaDurumu.Calisiyor });
                db.GorevTanim_Table.Add(new() { Id = i, PersonelId = i, BirimId = unit, Adi = i == 4 ? "Direktör" : "Uzman", Aktif = true, HarcirahGrupId = i == 2 ? 2 : i == 3 ? 1 : 3 });
            }
            db.IletisimBilgileri_Table.AddRange(new IletisimBilgileri { PersonelId = 1, InternetEPosta = "worker@example.test" }, new IletisimBilgileri { PersonelId = 2, InternetEPosta = "manager@example.test" }, new IletisimBilgileri { PersonelId = 3, InternetEPosta = "child@example.test" });
            for (var i = 1; i <= 3; i++) { var rate = Rate(i, i * 100); rate.KadroGrupId = i; rate.KadroGrubu = "Grup " + i; db.Harcirah_Table.Add(rate); }
            await db.SaveChangesAsync(); return data;
        }
        public ValueTask DisposeAsync() => _connection.DisposeAsync();
        private sealed class Context(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                var keep = new HashSet<Type> { typeof(Personel), typeof(IsBilgileri), typeof(BirimTanim), typeof(UnvanTanim), typeof(GorevTanim), typeof(GorevOnay), typeof(IletisimBilgileri), typeof(Harcirah) };
                foreach (var type in modelBuilder.Model.GetEntityTypes().Select(x => x.ClrType).Where(x => !keep.Contains(x)).ToList()) modelBuilder.Ignore(type);
            }
        }
    }
}
