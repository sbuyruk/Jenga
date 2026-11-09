using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Jenga.Overtime.Tests;

public sealed class OvertimeDatabase : IAsyncLifetime, IDbContextFactory<ApplicationDbContext>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    public ApplicationDbContext CreateDbContext() => new TestContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);

    private sealed class TestContext(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var keep = new HashSet<Type>
            {
                typeof(Personel), typeof(IsBilgileri), typeof(BirimTanim), typeof(UnvanTanim),
                typeof(OvertimeEntry), typeof(OvertimeInterval), typeof(OvertimePeriod), typeof(OvertimeDelivery),
                typeof(OvertimeSchedule), typeof(OvertimePersonResult), typeof(OvertimeEvent)
            };
            foreach (var type in modelBuilder.Model.GetEntityTypes().Select(x => x.ClrType).Where(x => !keep.Contains(x)).ToList())
                modelBuilder.Ignore(type);
            // SQLite has no SQL Server rowversion; retain optimistic concurrency with generated tokens.
            modelBuilder.Entity<OvertimeEntry>().Property(x => x.Version).ValueGeneratedNever();
            modelBuilder.Entity<OvertimePeriod>().Property(x => x.Version).ValueGeneratedNever();
            modelBuilder.Entity<OvertimeDelivery>().Property(x => x.Version).ValueGeneratedNever();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries().Where(x => x.State is EntityState.Added or EntityState.Modified))
            {
                if (entry.Entity is OvertimeEntry record) record.Version = Guid.NewGuid().ToByteArray();
                if (entry.Entity is OvertimePeriod period) period.Version = Guid.NewGuid().ToByteArray();
                if (entry.Entity is OvertimeDelivery delivery) delivery.Version = Guid.NewGuid().ToByteArray();
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
        // Legacy SQL Server data permits a null title name despite the model's Required annotation.
        await db.Database.ExecuteSqlRawAsync("""
            PRAGMA foreign_keys = OFF;
            DROP TABLE UnvanTanim_Table;
            CREATE TABLE UnvanTanim_Table (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                GorTipId INTEGER NOT NULL, Adi TEXT NULL, KisaAdi TEXT NULL,
                Aciklama TEXT NULL, Olusturan TEXT NULL, OlusturmaTarihi TEXT NULL,
                Degistiren TEXT NULL, DegistirmeTarihi TEXT NULL
            );
            PRAGMA foreign_keys = ON;
            """);
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}

public sealed class OvertimeWorkflowTests(OvertimeDatabase database) : IClassFixture<OvertimeDatabase>
{
    private static readonly DateTime Monday = new(2026, 10, 26);
    private sealed class TestAuthorization(string login, bool hr) : IServiceAuthorizationContext
    {
        public Task<string?> GetUserNameAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(login);
        public Task<IReadOnlyList<int>> GetAuthorizedRegionIdsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<int>>([]);
        public Task<bool> HasPermissionAsync(ModuleName module, Operation operation, CancellationToken cancellationToken = default)
            => Task.FromResult(hr && module == ModuleName.IKYS && operation == Operation.Manage);
    }
    private IOvertimeService Service(string user, bool hr = false) => new OvertimeService(database, new TestAuthorization(user, hr), new NullLogService());
    private static T Success<T>(Result<T> result) { Assert.True(result.IsSuccess, $"{result.Error.Message} {result.Error.Exception}"); return result.Value; }
    private static OvertimeEntryInput Input(DateTime? date = null) => new()
    {
        WorkDate = date ?? Monday.AddDays(5), Reason = "Havalimanı görevi", Submit = true,
        Intervals = [new() { Start = new(8, 0), End = new(16, 0) }]
    };

    private async Task<(int Worker, int Unit, int Title)> Reset(int normal = 40)
    {
        await using var db = database.CreateDbContext();
        await db.Database.ExecuteSqlRawAsync("""
            DELETE FROM OvertimeEvent_Table; DELETE FROM OvertimeInterval_Table;
            DELETE FROM OvertimeResult_Table; DELETE FROM OvertimeEntry_Table; DELETE FROM OvertimeDelivery_Table;
            DELETE FROM OvertimePeriod_Table; DELETE FROM OvertimeSchedule_Table;
            DELETE FROM IsBilgileri_Table; DELETE FROM Personel_Table; DELETE FROM BirimTanim_Table; DELETE FROM UnvanTanim_Table;
            """);
        var people = new[] { "worker", "manager", "other", "hr" }.Select((x, i) => new Personel { Adi = x, Soyadi = "Test", SicilNo = i + 1, KullaniciAdi = x }).ToList();
        db.Personel_Table.AddRange(people);
        var title = new UnvanTanim { Adi = "Uzman", GorTipId = 1 };
        db.UnvanTanim_Table.Add(title);
        await db.SaveChangesAsync();
        var unit = new BirimTanim { Adi = "Test Birimi", KisaAdi = "TEST", ParentId = 0, AmirId = people[1].Id, Aktif = true, Sira = 1, BolgeId = 1 };
        db.BirimTanim_Table.Add(unit);
        await db.SaveChangesAsync();
        db.IsBilgileri_Table.Add(new() { PersonelId = people[0].Id, BirimId = unit.Id, UnvanId = title.Id, CalismaDurumu = 0 });
        db.OvertimeSchedules.Add(new() { TitleId = title.Id, WeeklyHours = normal });
        await db.SaveChangesAsync();
        return (people[0].Id, unit.Id, title.Id);
    }

    private async Task<OvertimeEntry> Entry(string user = "worker")
        => Assert.Single(Success(await Service(user).LoadAsync("mine", Monday, Monday.AddDays(13))).Entries);

    private async Task<OvertimePeriod> Period()
        => Assert.Single(Success(await Service("hr", true).LoadAsync("hr", Monday, Monday.AddDays(13))).Periods);

    private async Task<OvertimeDelivery> Delivery()
        => Assert.Single(Success(await Service("hr", true).LoadAsync("hr", Monday, Monday.AddDays(13))).Deliveries);

    [Fact]
    public async Task CompleteWorkflow_FreezesRecordsAndArchivesSignedDocument()
    {
        var seed = await Reset();
        IOvertimeService worker = Service("DOMAIN\\worker"), manager = Service("manager"), hr = Service("hr", true);
        Success(await worker.SaveEntryAsync(Input()));
        var entry = await Entry();
        Success(await manager.ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));
        var id = Success(await hr.CreatePeriodAsync("Ekim son haftası", Monday, Monday.AddDays(6), Monday.AddDays(9)));
        var delivery = await Delivery();
        Success(await manager.IssueUnitAsync(delivery.Id, delivery.Version));
        var frozen = Success(await manager.GetReportAsync(id, seed.Unit, null));
        Assert.False(frozen.IsPreview);
        Assert.False((await worker.SaveEntryAsync(Input(Monday.AddDays(4)))).IsSuccess);
        delivery = await Delivery();
        Success(await hr.ReceiveUnitAsync(delivery.Id, delivery.Version));
        Assert.Equal(frozen.Reference, Success(await manager.GetReportAsync(id, seed.Unit, null)).Reference);
        var period = await Period();
        Success(await hr.PrepareSignatureAsync(id, period.Version));
        var report = Success(await hr.GetReportAsync(id, null, null));
        var person = Assert.Single(report.Results);
        Assert.Equal(120, person.RawMinutes);
        Assert.Equal(120, person.RoundedMinutes);
        Assert.False(report.IsPreview);
        var bytes = System.Text.Encoding.ASCII.GetBytes("%PDF-1.4\nTest signed document");
        period = await Period();
        Success(await hr.FinalizeAsync(id, period.Version, "imzali.pdf", bytes));
        var finalized = await Period();
        Assert.Equal(OvertimePeriodStatus.Finalized, finalized.Status);
        Assert.Equal(report.Reference, Success(await hr.GetReportAsync(id, null, null)).Reference);
        Assert.Equal(bytes, Success(await hr.GetSignedDocumentAsync(id)).Data);
        Assert.False((await hr.ReopenPeriodAsync(id, finalized.Version, "Değişiklik")).IsSuccess);
        Assert.False((await worker.GetSignedDocumentAsync(id)).IsSuccess);
    }

    [Fact]
    public async Task EntryOwnership_ManagerScopeAndHrPermissionAreEnforcedInService()
    {
        await Reset();
        var worker = Service("worker");
        Success(await worker.SaveEntryAsync(Input()));
        var entry = await Entry();
        Assert.Empty(Success(await Service("other").LoadAsync("mine", Monday, Monday.AddDays(6))).Entries);
        Assert.Empty(Success(await Service("other").LoadAsync("team", Monday, Monday.AddDays(6))).Entries);
        Assert.False((await Service("other").ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, "")).IsSuccess);
        Assert.False((await Service("other").GetEntryHistoryAsync(entry.Id)).IsSuccess);
        Assert.False((await worker.LoadAsync("hr", Monday, Monday.AddDays(6))).IsSuccess);
        Assert.False((await worker.CreatePeriodAsync("Test", Monday, Monday.AddDays(6), Monday.AddDays(7))).IsSuccess);
        var maliciousEdit = Input(); maliciousEdit.Id = entry.Id; maliciousEdit.Version = entry.Version;
        Assert.False((await Service("other").SaveEntryAsync(maliciousEdit)).IsSuccess);
        Assert.False((await Service("").LoadAsync("mine", Monday, Monday.AddDays(6))).IsSuccess);
    }

    [Fact]
    public async Task CorrectionRequiresReasonAndKeepsHistoryAndConcurrency()
    {
        await Reset();
        IOvertimeService worker = Service("worker"), manager = Service("manager");
        Success(await worker.SaveEntryAsync(Input()));
        var entry = await Entry();
        Assert.False((await manager.ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Correction, " ")).IsSuccess);
        Success(await manager.ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Correction, "Saatleri kontrol edin"));
        Assert.False((await manager.ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, "")).IsSuccess);
        var correction = await Entry();
        var input = Input(); input.Id = correction.Id; input.Version = correction.Version; input.Reason = "Düzeltilmiş açıklama";
        Success(await worker.SaveEntryAsync(input));
        var history = Success(await worker.GetEntryHistoryAsync(entry.Id));
        Assert.Equal(3, history.Count);
        using var snapshot = System.Text.Json.JsonDocument.Parse(history[0].Details);
        Assert.Equal("Havalimanı görevi", snapshot.RootElement.GetProperty("After").GetProperty("Reason").GetString());
        Assert.Contains("Saatleri kontrol edin", history[1].Details);
        Assert.Equal(OvertimeEntryStatus.Pending, (await Entry()).Status);
    }

    [Fact]
    public async Task ValidationRejectsMissingReasonOverlapDuplicateDayAndPartialWeeks()
    {
        await Reset();
        IOvertimeService worker = Service("worker"), hr = Service("hr", true);
        var blank = Input(); blank.Reason = " ";
        Assert.False((await worker.SaveEntryAsync(blank)).IsSuccess);
        var overlap = Input(); overlap.Intervals.Add(new() { Start = new(15, 0), End = new(17, 0) });
        Assert.False((await worker.SaveEntryAsync(overlap)).IsSuccess);
        Success(await worker.SaveEntryAsync(Input()));
        Assert.False((await worker.SaveEntryAsync(Input())).IsSuccess);
        Assert.False((await hr.CreatePeriodAsync("Bölünmüş", Monday.AddDays(1), Monday.AddDays(6), Monday.AddDays(7))).IsSuccess);
        Success(await hr.CreatePeriodAsync("Tam hafta", Monday, Monday.AddDays(6), Monday.AddDays(7)));
        Assert.False((await hr.CreatePeriodAsync("Çakışan", Monday, Monday.AddDays(13), Monday.AddDays(14))).IsSuccess);
    }

    [Fact]
    public async Task SignatureRequiresReceivedDocumentsAndReopeningInvalidatesPriorRevision()
    {
        await Reset(45);
        IOvertimeService worker = Service("worker"), manager = Service("manager"), hr = Service("hr", true);
        Success(await worker.SaveEntryAsync(Input()));
        var id = Success(await hr.CreatePeriodAsync("Test", Monday, Monday.AddDays(6), Monday.AddDays(8)));
        var delivery = await Delivery();
        Assert.False((await manager.IssueUnitAsync(delivery.Id, delivery.Version)).IsSuccess);
        var entry = await Entry();
        Success(await manager.ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));
        Success(await manager.IssueUnitAsync(delivery.Id, delivery.Version));
        var oldReference = Success(await manager.GetReportAsync(id, delivery.UnitId, null)).Reference;
        var period = await Period();
        Assert.False((await hr.PrepareSignatureAsync(id, period.Version)).IsSuccess);
        delivery = await Delivery();
        Success(await hr.ReopenUnitAsync(delivery.Id, delivery.Version, "Evrak ile saatler farklı"));
        var corrected = await Entry();
        Assert.Equal(OvertimeEntryStatus.Correction, corrected.Status);
        Assert.Null(corrected.PeriodId);
        var edit = Input(); edit.Id = corrected.Id; edit.Version = corrected.Version;
        Success(await worker.SaveEntryAsync(edit));
        corrected = await Entry();
        Success(await manager.ReviewAsync(corrected.Id, corrected.Version, OvertimeEntryStatus.Approved, ""));
        delivery = await Delivery();
        Success(await manager.IssueUnitAsync(delivery.Id, delivery.Version));
        Assert.NotEqual(oldReference, Success(await manager.GetReportAsync(id, delivery.UnitId, null)).Reference);
    }

    [Fact]
    public async Task ScheduleChangesPreserveAlreadyDeclaredWorkingGroup()
    {
        var seed = await Reset(40);
        Success(await Service("worker").SaveEntryAsync(Input()));
        Success(await Service("hr", true).SetScheduleAsync(seed.Title, 45));
        Assert.Equal(2400, (await Entry()).NormalWeeklyMinutes);
    }

    [Fact]
    public async Task HrTitleOptionsIgnoreLegacyNullAndBlankNames_AndKeepUnconfiguredTitles()
    {
        var seed = await Reset();
        await using (var db = database.CreateDbContext())
        {
            await db.Database.ExecuteSqlRawAsync("""
                INSERT INTO UnvanTanim_Table (GorTipId, Adi)
                VALUES (1, NULL), (1, ''), (1, '   '), (1, 'Şoför');
                """);
        }

        var workspace = Success(await Service("hr", true).LoadAsync("hr", Monday, Monday.AddDays(6)));

        Assert.Equal(2, workspace.Titles.Count);
        Assert.Equal(40, Assert.Single(workspace.Titles, x => x.Id == seed.Title).WeeklyHours);
        Assert.Null(Assert.Single(workspace.Titles, x => x.Name == "Şoför").WeeklyHours);
    }

    [Fact]
    public async Task ManagerCannotApproveOwnEntry_ExistingParentUnitProvidesSuperiorApproval()
    {
        var seed = await Reset();
        await using (var db = database.CreateDbContext())
        {
            var manager = await db.Personel_Table.SingleAsync(x => x.KullaniciAdi == "manager");
            var superior = await db.Personel_Table.SingleAsync(x => x.KullaniciAdi == "other");
            var parent = new BirimTanim { Adi = "Üst Birim", KisaAdi = "UST", ParentId = 0, AmirId = superior.Id, Aktif = true, Sira = 2, BolgeId = 1 };
            db.BirimTanim_Table.Add(parent);
            await db.SaveChangesAsync();
            var unit = await db.BirimTanim_Table.SingleAsync(x => x.Id == seed.Unit);
            unit.ParentId = parent.Id;
            db.IsBilgileri_Table.Add(new() { PersonelId = manager.Id, BirimId = seed.Unit, UnvanId = seed.Title, CalismaDurumu = 0 });
            await db.SaveChangesAsync();
        }
        Success(await Service("manager").SaveEntryAsync(Input()));
        var entry = await Entry("manager");
        Assert.False((await Service("manager").ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, "")).IsSuccess);
        var team = Success(await Service("other").LoadAsync("team", Monday, Monday.AddDays(6)));
        Assert.Equal(entry.Id, Assert.Single(team.Entries).Id);
        Success(await Service("other").ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));
    }

    [Fact]
    public async Task NonHrReportsCannotExposeAnotherPersonsRecordsOrUnit()
    {
        var seed = await Reset();
        Success(await Service("worker").SaveEntryAsync(Input()));
        var entry = await Entry();
        Success(await Service("manager").ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));
        var id = Success(await Service("hr", true).CreatePeriodAsync("Test", Monday, Monday.AddDays(6), Monday.AddDays(9)));
        var other = Service("other");
        Assert.Empty(Success(await other.GetReportAsync(id, null, seed.Worker)).Entries);
        Assert.False((await other.GetReportAsync(id, seed.Unit, null)).IsSuccess);
        Assert.False((await Service("manager").GetReportAsync(id, seed.Unit, seed.Worker)).IsSuccess);
    }

    [Fact]
    public async Task ApprovedEntryCanReturnForCorrectionUntilUnitIsFrozen()
    {
        await Reset();
        var manager = Service("manager");
        Success(await Service("worker").SaveEntryAsync(Input()));
        var entry = await Entry();
        Success(await manager.ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));
        entry = await Entry();
        Success(await manager.ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Correction, "Görev saati düzeltilecek"));
        Assert.Equal(OvertimeEntryStatus.Correction, (await Entry()).Status);
    }

    [Fact]
    public async Task EditingOnlyIntervalsAlsoChangesEntryVersion()
    {
        await Reset();
        var worker = Service("worker");
        var initial = Input(); initial.Submit = false;
        Success(await worker.SaveEntryAsync(initial));
        var entry = await Entry();
        var update = Input(); update.Id = entry.Id; update.Version = entry.Version.ToArray(); update.Submit = false;
        update.Intervals[0].End = new(17, 0);
        Success(await worker.SaveEntryAsync(update));
        var updated = await Entry();
        Assert.False(entry.Version.SequenceEqual(updated.Version));
        Assert.False((await worker.SaveEntryAsync(update)).IsSuccess);
    }

    [Fact]
    public async Task RecalledSignatureRequiresNewRevision_AndRejectsInvalidSignedFile()
    {
        await Reset();
        IOvertimeService worker = Service("worker"), manager = Service("manager"), hr = Service("hr", true);
        Success(await worker.SaveEntryAsync(Input()));
        var entry = await Entry();
        Success(await manager.ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));
        var id = Success(await hr.CreatePeriodAsync("Test", Monday, Monday.AddDays(6), Monday.AddDays(8)));
        var delivery = await Delivery();
        Success(await manager.IssueUnitAsync(delivery.Id, delivery.Version));
        delivery = await Delivery();
        Success(await hr.ReceiveUnitAsync(delivery.Id, delivery.Version));
        var period = await Period();
        Success(await hr.PrepareSignatureAsync(id, period.Version));
        var first = Success(await hr.GetReportAsync(id, null, null));
        period = await Period();
        Assert.False((await hr.FinalizeAsync(id, period.Version, "imza.pdf", [1, 2, 3])).IsSuccess);
        Assert.False((await hr.ReopenPeriodAsync(id, period.Version, " ")).IsSuccess);
        Success(await hr.ReopenPeriodAsync(id, period.Version, "Listeyi kontrol et"));
        var preview = Success(await hr.GetReportAsync(id, null, null));
        Assert.True(preview.IsPreview);
        Assert.Contains(preview.Events, x => x.Action == "İmza listesi geri alındı");
        period = await Period();
        Success(await hr.PrepareSignatureAsync(id, period.Version));
        Assert.NotEqual(first.Reference, Success(await hr.GetReportAsync(id, null, null)).Reference);
    }

    [Fact]
    public async Task OvernightEntrySplitsIntoCalendarDays_AndAppliesWeekendRestOnlyOnSaturday()
    {
        await Reset(45);
        var input = Input(Monday.AddDays(4));
        input.EndWorkDate = Monday.AddDays(5);
        input.Intervals = [new() { Start = new(22, 0), End = new(6, 0) }];
        Success(await Service("worker").SaveEntryAsync(input));

        var entries = Success(await Service("worker").LoadAsync("mine", Monday, Monday.AddDays(6))).Entries.OrderBy(x => x.WorkDate).ToList();
        Assert.Equal(2, entries.Count);
        Assert.Equal(Monday.AddDays(4), entries[0].WorkDate);
        Assert.Equal(1440, Assert.Single(entries[0].Intervals).EndMinute);
        Assert.Equal(Monday.AddDays(5), entries[1].WorkDate);
        Assert.Equal(0, Assert.Single(entries[1].Intervals).StartMinute);
        Assert.Equal(360, entries[1].Intervals[0].EndMinute);
        Assert.All(entries, x => Assert.Equal(OvertimeEntryStatus.Pending, x.Status));
        var week = Assert.Single(OvertimeCalculator.Calculate(entries, Monday, Monday.AddDays(6)));
        Assert.Equal(480, week.GrossMinutes);
        Assert.Equal(30, week.RestMinutes);
        Assert.Equal(450, week.OvertimeMinutes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OvernightConflictRollsBackEveryDayAndHistory(bool editExisting)
    {
        await Reset();
        var worker = Service("worker");
        var occupied = Input(Monday.AddDays(5));
        Success(await worker.SaveEntryAsync(occupied));
        var input = Input(Monday.AddDays(4));
        byte[]? previousVersion = null;
        if (editExisting)
        {
            var draft = Input(Monday.AddDays(4));
            draft.Submit = false;
            draft.Reason = "Önceki taslak";
            Success(await worker.SaveEntryAsync(draft));
            var entry = Success(await worker.LoadAsync("mine", Monday.AddDays(4), Monday.AddDays(4))).Entries.Single();
            input.Id = entry.Id;
            input.Version = entry.Version;
            previousVersion = entry.Version;
        }
        input.EndWorkDate = Monday.AddDays(5);
        input.Intervals = [new() { Start = new(22, 0), End = new(6, 0) }];

        Assert.False((await worker.SaveEntryAsync(input)).IsSuccess);

        var entries = Success(await worker.LoadAsync("mine", Monday, Monday.AddDays(6))).Entries;
        Assert.Equal(editExisting ? 2 : 1, entries.Count);
        await using var db = database.CreateDbContext();
        Assert.Equal(editExisting ? 2 : 1, await db.OvertimeEvents.CountAsync());
        if (editExisting)
        {
            var original = Assert.Single(entries, x => x.WorkDate == Monday.AddDays(4));
            Assert.Equal("Önceki taslak", original.Reason);
            Assert.Equal(OvertimeEntryStatus.Draft, original.Status);
            Assert.Equal(previousVersion, original.Version);
        }
    }

    [Fact]
    public async Task EndingExactlyAtMidnightDoesNotCreateAnEmptyNextDay()
    {
        await Reset();
        var input = Input(Monday);
        input.EndWorkDate = Monday.AddDays(1);
        input.Intervals = [new() { Start = new(22, 0), End = TimeOnly.MinValue }];
        Success(await Service("worker").SaveEntryAsync(input));
        var entry = await Entry();
        Assert.Equal(Monday, entry.WorkDate);
        Assert.Equal(1440, Assert.Single(entry.Intervals).EndMinute);
    }

    [Fact]
    public async Task InvalidOvernightDateRangeDoesNotCreateAnyRecords()
    {
        await Reset();
        var input = Input(Monday);
        input.EndWorkDate = Monday.AddDays(2);
        input.Intervals = [new() { Start = new(22, 0), End = new(6, 0) }];
        Assert.False((await Service("worker").SaveEntryAsync(input)).IsSuccess);
        await using var db = database.CreateDbContext();
        Assert.Empty(await db.OvertimeEntries.ToListAsync());
        Assert.Empty(await db.OvertimeEvents.ToListAsync());
    }

    [Fact]
    public async Task SecondEntryOnApprovedDayExplainsCorrectionAndPreservesExistingRecord()
    {
        await Reset();
        var worker = Service("worker");
        var input = Input();
        Success(await worker.SaveEntryAsync(input));
        var entry = await Entry();
        Success(await Service("manager").ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));

        var result = await worker.SaveEntryAsync(Input());

        Assert.False(result.IsSuccess);
        Assert.Contains("onaylı", result.Error.Message);
        Assert.Contains("düzeltmeye", result.Error.Message);
        Assert.Equal(OvertimeEntryStatus.Approved, (await Entry()).Status);
        Success(await worker.SaveEntryAsync(Input(input.WorkDate.AddDays(1))));
        Assert.Equal(2, Success(await worker.LoadAsync("mine", Monday, Monday.AddDays(13))).Entries.Count);
    }
}
