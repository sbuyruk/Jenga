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
                typeof(Personel), typeof(IsBilgileri), typeof(BirimTanim), typeof(UnvanTanim), typeof(GorevTanim),
                typeof(OvertimeEntry), typeof(OvertimeInterval), typeof(OvertimePeriod), typeof(OvertimeDelivery),
                typeof(OvertimeSchedule), typeof(OvertimePersonResult), typeof(OvertimeEvent), typeof(OvertimeDelegation)
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
            DELETE FROM OvertimeDelegation_Table; DELETE FROM OvertimeEvent_Table; DELETE FROM OvertimeInterval_Table;
            DELETE FROM OvertimeResult_Table; DELETE FROM OvertimeEntry_Table; DELETE FROM OvertimeDelivery_Table;
            DELETE FROM OvertimePeriod_Table; DELETE FROM OvertimeSchedule_Table;
            DELETE FROM IsBilgileri_Table; DELETE FROM GorevTanim_Table; DELETE FROM Personel_Table; DELETE FROM BirimTanim_Table; DELETE FROM UnvanTanim_Table;
            """);
        var people = new[] { "worker", "manager", "other", "hr" }.Select((x, i) => new Personel { Adi = x, Soyadi = "Test", SicilNo = i + 1, KullaniciAdi = x, Tipi = PersonelTipi.Kadrolu }).ToList();
        db.Personel_Table.AddRange(people);
        var title = new UnvanTanim { Adi = "Uzman", GorTipId = 1 };
        db.UnvanTanim_Table.Add(title);
        await db.SaveChangesAsync();
        var unit = new BirimTanim { Adi = "Test Birimi", KisaAdi = "TEST", ParentId = 0, AmirId = people[1].Id, Aktif = true, Sira = 1, BolgeId = 1 };
        db.BirimTanim_Table.Add(unit);
        await db.SaveChangesAsync();
        db.IsBilgileri_Table.AddRange(people.Select(person => new IsBilgileri { PersonelId = person.Id, BirimId = unit.Id, UnvanId = title.Id, CalismaDurumu = CalismaDurumu.Calisiyor }));
        db.OvertimeSchedules.Add(new() { TitleId = title.Id, WeeklyHours = normal });
        await db.SaveChangesAsync();
        return (people[0].Id, unit.Id, title.Id);
    }

    private async Task<OvertimeEntry> Entry(string user = "worker")
        => Assert.Single(Success(await Service(user).LoadAsync("mine", Monday, Monday.AddDays(13))).Entries);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReportSignaturesUseUnitManagerDutyAndCorrectDeputyInAncestorChain(bool hasDeputy)
    {
        var seed = await Reset();
        var managerId = await PersonId("manager");
        var otherId = await PersonId("other");
        var deputyId = await PersonId("hr");
        await using (var db = database.CreateDbContext())
        {
            var duty = new GorevTanim { Adi = "İdari İşler Müdürü", BirimId = seed.Unit, Aktif = true };
            var deputyUnit = new BirimTanim { Adi = hasDeputy ? "Genel Müdür Yardımcılığı" : "Genel Müdürlük", KisaAdi = "UST", ParentId = 0, AmirId = deputyId, Aktif = true, Sira = 1, BolgeId = 1 };
            db.GorevTanim_Table.Add(duty);
            db.BirimTanim_Table.Add(deputyUnit);
            await db.SaveChangesAsync();
            var intermediate = new BirimTanim { Adi = "Ara Birim", KisaAdi = "ARA", ParentId = deputyUnit.Id, AmirId = otherId, Aktif = true, Sira = 1, BolgeId = 1 };
            db.BirimTanim_Table.Add(intermediate);
            await db.SaveChangesAsync();
            (await db.BirimTanim_Table.SingleAsync(x => x.Id == seed.Unit)).ParentId = intermediate.Id;
            (await db.IsBilgileri_Table.SingleAsync(x => x.PersonelId == managerId)).GorevId = duty.Id;
            // A malformed ancestor cycle must not hang or select an unrelated manager.
            if (!hasDeputy) deputyUnit.ParentId = intermediate.Id;
            await db.SaveChangesAsync();
        }
        Success(await Service("worker").SaveEntryAsync(Input()));
        var entry = await Entry();
        Success(await Service("manager").ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));
        var periodId = Success(await Service("hr", true).CreatePeriodAsync("İmza testi", Monday, Monday.AddDays(6), Monday.AddDays(9)));
        var report = Success(await Service("manager").GetReportAsync(periodId, seed.Unit, null));
        var signatures = report.UnitSignatures[seed.Unit];
        Assert.Equal("manager Test", signatures.ManagerName);
        Assert.Equal("İdari İşler Müdürü", signatures.ManagerDuty);
        Assert.Equal(hasDeputy ? "hr Test" : "Bağlı GMY tanımlanmamış", signatures.DeputyGeneralManagerName);
        var personal = Success(await Service("worker").GetReportAsync(periodId, null, null));
        Assert.Equal(signatures.DeputyGeneralManagerName, personal.UnitSignatures[seed.Unit].DeputyGeneralManagerName);
    }

    private async Task<OvertimePeriod> Period()
        => Assert.Single(Success(await Service("hr", true).LoadAsync("hr", Monday, Monday.AddDays(13))).Periods);

    private async Task<OvertimeDelivery> Delivery()
        => Assert.Single(Success(await Service("hr", true).LoadAsync("hr", Monday, Monday.AddDays(13))).Deliveries);

    [Fact]
    public async Task EmptyUnitsCannotIssueChartsAndDoNotBlockSignaturePreparation()
    {
        var seed = await Reset();
        var otherId = await PersonId("other");
        int emptyUnitId;
        await using (var db = database.CreateDbContext())
        {
            var unit = new BirimTanim { Adi = "Mesaisiz Birim", KisaAdi = "BOS", ParentId = 0, AmirId = otherId, Aktif = true, Sira = 2, BolgeId = 1 };
            db.BirimTanim_Table.Add(unit);
            await db.SaveChangesAsync();
            emptyUnitId = unit.Id;
            (await db.IsBilgileri_Table.SingleAsync(x => x.PersonelId == otherId)).BirimId = unit.Id;
            await db.SaveChangesAsync();
        }
        var hr = Service("hr", true);
        var periodId = Success(await hr.CreatePeriodAsync("Test", Monday, Monday.AddDays(6), Monday.AddDays(9)));
        var workspace = Success(await hr.LoadAsync("hr", Monday, Monday.AddDays(6)));
        var empty = Assert.Single(workspace.Deliveries, x => x.UnitId == emptyUnitId);
        Assert.Equal(0, empty.EntryCount);
        Assert.Equal(0, empty.ApprovedEntryCount);
        Assert.False((await hr.IssueUnitAsync(empty.Id, empty.Version)).IsSuccess);
        Success(await Service("worker").SaveEntryAsync(Input()));
        var entry = await Entry();
        workspace = Success(await hr.LoadAsync("hr", Monday, Monday.AddDays(6)));
        Assert.Equal(0, Assert.Single(workspace.Deliveries, x => x.UnitId == seed.Unit).ApprovedEntryCount);
        Assert.False((await hr.PrepareSignatureAsync(periodId, (await Period()).Version)).IsSuccess);
        Success(await Service("manager").ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));
        // Counts cover the entire period even when the page filter covers only Monday.
        workspace = Success(await hr.LoadAsync("hr", Monday, Monday));
        var populated = Assert.Single(workspace.Deliveries, x => x.UnitId == seed.Unit);
        Assert.Equal(1, populated.EntryCount);
        Assert.Equal(1, populated.ApprovedEntryCount);
        Assert.False((await hr.PrepareSignatureAsync(periodId, (await Period()).Version)).IsSuccess);
        Success(await hr.IssueUnitAsync(populated.Id, populated.Version));
        workspace = Success(await hr.LoadAsync("hr", Monday, Monday.AddDays(6)));
        populated = Assert.Single(workspace.Deliveries, x => x.UnitId == seed.Unit);
        Success(await hr.ReceiveUnitAsync(populated.Id, populated.Version));
        Success(await hr.PrepareSignatureAsync(periodId, (await Period()).Version));
        Assert.Equal(OvertimePeriodStatus.AwaitingSignature, (await Period()).Status);
    }

    [Fact]
    public async Task ExternalPersonnelCanEnterAndDelegateOvertime_AndTypesRemainFrozenInReports()
    {
        var seed = await Reset();
        var externalId = await PersonId("other");
        await using (var db = database.CreateDbContext())
        {
            (await db.Personel_Table.SingleAsync(x => x.Id == externalId)).Tipi = PersonelTipi.KadroHarici;
            (await db.IsBilgileri_Table.SingleAsync(x => x.PersonelId == externalId)).KadroHariciKurum = "Kurum A";
            await db.SaveChangesAsync();
        }
        var hr = Service("hr", true);
        // An external operator may still enter overtime for an assigned permanent employee.
        Success(await hr.SetDelegationsAsync(externalId, [seed.Worker], []));
        var delegated = Input(); delegated.PersonelId = seed.Worker;
        Success(await Service("other").SaveEntryAsync(delegated));
        Success(await Service("other").SaveEntryAsync(Input()));
        var manager = Service("manager");
        foreach (var entry in Success(await manager.LoadAsync("team", Monday, Monday.AddDays(6))).Entries)
            Success(await manager.ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));
        var periodId = Success(await hr.CreatePeriodAsync("Karma personel", Monday, Monday.AddDays(6), Monday.AddDays(9)));
        var preview = Success(await hr.GetReportAsync(periodId, null, null));
        Assert.Equal(PersonelTipi.Kadrolu, Assert.Single(preview.Results, x => x.PersonelId == seed.Worker).PersonnelType);
        Assert.Equal(PersonelTipi.KadroHarici, Assert.Single(preview.Results, x => x.PersonelId == externalId).PersonnelType);
        Assert.Equal("Kurum A", Assert.Single(preview.Results, x => x.PersonelId == externalId).KadroHariciKurum);
        var delivery = await Delivery();
        Success(await manager.IssueUnitAsync(delivery.Id, delivery.Version));
        delivery = await Delivery();
        Success(await hr.ReceiveUnitAsync(delivery.Id, delivery.Version));
        Success(await hr.PrepareSignatureAsync(periodId, (await Period()).Version));
        await using (var db = database.CreateDbContext())
        {
            (await db.Personel_Table.SingleAsync(x => x.Id == seed.Worker)).Tipi = PersonelTipi.KadroHarici;
            (await db.Personel_Table.SingleAsync(x => x.Id == externalId)).Tipi = PersonelTipi.Kadrolu;
            (await db.IsBilgileri_Table.SingleAsync(x => x.PersonelId == externalId)).KadroHariciKurum = "Kurum B";
            await db.SaveChangesAsync();
        }
        var frozen = Success(await hr.GetReportAsync(periodId, null, null));
        Assert.Equal(PersonelTipi.Kadrolu, Assert.Single(frozen.Results, x => x.PersonelId == seed.Worker).PersonnelType);
        Assert.Equal(PersonelTipi.KadroHarici, Assert.Single(frozen.Results, x => x.PersonelId == externalId).PersonnelType);
        Assert.Equal("Kurum A", Assert.Single(frozen.Results, x => x.PersonelId == externalId).KadroHariciKurum);
        Assert.Equal(120, Assert.Single(frozen.Results, x => x.PersonnelType == PersonelTipi.Kadrolu).RoundedMinutes);
        Assert.Equal(120, Assert.Single(frozen.Results, x => x.PersonnelType == PersonelTipi.KadroHarici).RoundedMinutes);
        var unitChart = Success(await manager.GetReportAsync(periodId, seed.Unit, null));
        Assert.Equal(PersonelTipi.KadroHarici, Assert.Single(unitChart.Entries, x => x.PersonelId == externalId).PersonnelType);
        Assert.Equal(externalId, Assert.Single(Success(await Service("other").GetReportAsync(periodId, null, null)).Entries).PersonelId);
        Success(await hr.FinalizeAsync(periodId, (await Period()).Version, "imzali.pdf", "%PDF-1.7\nTest"u8.ToArray()));
        Assert.Equal(PersonelTipi.KadroHarici, Assert.Single(Success(await hr.GetReportAsync(periodId, null, null)).Results, x => x.PersonelId == externalId).PersonnelType);
    }

    [Fact]
    public async Task ExternalInstitutionIsRequired_OnlyHrCanAssignIt_AndWorkInformationPersistsIt()
    {
        await Reset();
        var externalId = await PersonId("other");
        await using (var db = database.CreateDbContext())
        {
            (await db.Personel_Table.SingleAsync(x => x.Id == externalId)).Tipi = PersonelTipi.KadroHarici;
            await db.SaveChangesAsync();
        }
        var external = Service("other");
        Assert.False((await external.SaveEntryAsync(Input())).IsSuccess);
        Assert.False((await external.SetExternalInstitutionAsync(externalId, "Kurum A")).IsSuccess);
        var hr = Service("hr", true);
        Assert.False((await hr.SetExternalInstitutionAsync(externalId, " ")).IsSuccess);
        Assert.False((await hr.SetExternalInstitutionAsync(externalId, new string('a', 201))).IsSuccess);
        var workService = new IsBilgileriService(database, new NullLogService());
        var info = Success(await workService.GetByPersonelIdAsync(externalId));
        Assert.False((await workService.UpdateAsync(info)).IsSuccess);
        Success(await hr.SetExternalInstitutionAsync(externalId, "  Kurum    A  "));
        info = Success(await workService.GetByPersonelIdAsync(externalId));
        Assert.Equal("Kurum A", info.KadroHariciKurum);
        info.KadroHariciKurum = "  Kurum B  ";
        Assert.True((await workService.UpdateAsync(info)).IsSuccess);
        Assert.Equal("Kurum B", Success(await workService.GetByPersonelIdAsync(externalId)).KadroHariciKurum);
        Success(await external.SaveEntryAsync(Input()));
        Assert.Equal("Kurum B", (await Entry("other")).KadroHariciKurum);
        Assert.Equal("Kurum B", Assert.Single(Success(await hr.LoadAsync("hr", Monday, Monday.AddDays(6))).ExternalInstitutions).Institution);
        Assert.Empty(Success(await external.LoadAsync("mine", Monday, Monday.AddDays(6))).ExternalInstitutions);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SharedActivityCreatesSeparateOwnedEntriesForEverySelectedPerson(bool submit)
    {
        var seed = await Reset();
        var operatorId = await PersonId("other");
        var managerId = await PersonId("manager");
        var hrId = await PersonId("hr");
        var targets = new List<int> { seed.Worker, managerId, hrId };
        var hr = Service("hr", true);
        Success(await hr.SetDelegationsAsync(operatorId, targets, []));
        var input = Input(); input.Submit = submit;
        input.PersonelIds = [.. targets, seed.Worker];
        input.Reason = "Yönetim Kurulu hazırlığı";
        input.Intervals = [new() { Start = new(17, 0), End = new(20, 0) }];
        var ids = Success(await Service("other").SaveEntriesAsync(input));
        Assert.Equal(3, ids.Count);
        await using var db = database.CreateDbContext();
        var entries = await db.OvertimeEntries.Include(x => x.Intervals).ToListAsync();
        Assert.Equal(targets.Order(), entries.Select(x => x.PersonelId).Order());
        Assert.All(entries, entry =>
        {
            Assert.Equal(submit ? OvertimeEntryStatus.Pending : OvertimeEntryStatus.Draft, entry.Status);
            Assert.Equal("Yönetim Kurulu hazırlığı", entry.Reason);
            Assert.Equal("other", entry.Olusturan);
            Assert.Equal(1020, Assert.Single(entry.Intervals).StartMinute);
            Assert.Equal(1200, Assert.Single(entry.Intervals).EndMinute);
        });
        Assert.Equal(3, await db.OvertimeEvents.CountAsync(x => x.EntryId != null));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SharedActivityRollsBackEveryPersonsEntriesAndAuditWhenALaterPersonConflicts(bool overnight)
    {
        var seed = await Reset();
        var operatorId = await PersonId("other");
        var hrId = await PersonId("hr");
        Success(await Service("hr", true).SetDelegationsAsync(operatorId, [seed.Worker, hrId], []));
        Success(await Service("hr").SaveEntryAsync(Input(overnight ? Monday.AddDays(6) : Monday.AddDays(5))));
        var input = Input(); input.PersonelIds = [seed.Worker, hrId];
        if (overnight)
        {
            input.EndWorkDate = input.WorkDate.AddDays(1);
            input.Intervals = [new() { Start = new(22, 0), End = new(2, 0) }];
        }
        var result = await Service("other").SaveEntriesAsync(input);
        Assert.False(result.IsSuccess);
        Assert.Contains("hr Test", result.Error.Message);
        await using var db = database.CreateDbContext();
        Assert.Equal(hrId, Assert.Single(await db.OvertimeEntries.ToListAsync()).PersonelId);
        Assert.Single(await db.OvertimeIntervals.ToListAsync());
        Assert.Equal(1, await db.OvertimeEvents.CountAsync(x => x.EntryId != null));
    }

    [Fact]
    public async Task SharedActivityRejectsUnauthorizedTargetsAndRevokedPermissionsWithoutSavingAnything()
    {
        var seed = await Reset();
        var operatorId = await PersonId("other");
        var hr = Service("hr", true);
        Success(await hr.SetDelegationsAsync(operatorId, [seed.Worker], []));
        var input = Input(); input.PersonelIds = [seed.Worker, await PersonId("manager")];
        Assert.False((await Service("other").SaveEntriesAsync(input)).IsSuccess);
        input.PersonelIds = [seed.Worker];
        Success(await hr.SetDelegationsAsync(operatorId, [], [seed.Worker]));
        Assert.False((await Service("other").SaveEntriesAsync(input)).IsSuccess);
        await using var db = database.CreateDbContext();
        Assert.Empty(await db.OvertimeEntries.ToListAsync());
    }

    [Fact]
    public async Task MissingPersonnelTypeDoesNotBlockEntryButPreventsSignaturePreparation()
    {
        var seed = await Reset();
        await using (var db = database.CreateDbContext())
        {
            (await db.Personel_Table.SingleAsync(x => x.Id == seed.Worker)).Tipi = null;
            await db.SaveChangesAsync();
        }
        Success(await Service("worker").SaveEntryAsync(Input()));
        var entry = await Entry();
        Success(await Service("manager").ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));
        var hr = Service("hr", true);
        var periodId = Success(await hr.CreatePeriodAsync("Eksik tip", Monday, Monday.AddDays(6), Monday.AddDays(9)));
        Assert.Null(Assert.Single(Success(await hr.GetReportAsync(periodId, null, null)).Results).PersonnelType);
        var delivery = await Delivery();
        Success(await hr.IssueUnitAsync(delivery.Id, delivery.Version));
        delivery = await Delivery();
        Success(await hr.ReceiveUnitAsync(delivery.Id, delivery.Version));
        Assert.False((await hr.PrepareSignatureAsync(periodId, (await Period()).Version)).IsSuccess);
    }

    [Theory]
    [InlineData("Personel ve İdari İşler Direktörlüğü")]
    [InlineData("Per.ve İd.İşl. Direktörlüğü")]
    public async Task SummaryDirectorComesFromActivePersonnelAndAdministrativeAffairsUnit(string unitName)
    {
        await Reset();
        var directorId = await PersonId("other");
        await using (var db = database.CreateDbContext())
        {
            var deputyUnit = new BirimTanim { Adi = "Genel Müdür Yardımcılığı", KisaAdi = "GMY", ParentId = 0, AmirId = await PersonId("manager"), Aktif = true, Sira = 1, BolgeId = 1 };
            db.BirimTanim_Table.Add(deputyUnit);
            await db.SaveChangesAsync();
            db.BirimTanim_Table.Add(new BirimTanim { Adi = unitName, KisaAdi = "PID", ParentId = deputyUnit.Id, AmirId = directorId, Aktif = true, Sira = 2, BolgeId = 1 });
            db.BirimTanim_Table.Add(new BirimTanim { Adi = unitName, KisaAdi = "ESKI", ParentId = 0, AmirId = await PersonId("manager"), Aktif = false, Sira = 3, BolgeId = 1 });
            await db.SaveChangesAsync();
        }
        var hr = Service("hr", true);
        var periodId = Success(await hr.CreatePeriodAsync("İmza testi", Monday, Monday.AddDays(6), Monday.AddDays(9)));
        var report = Success(await hr.GetReportAsync(periodId, null, null));
        Assert.True(report.CanSeeSummary);
        Assert.Equal("other Test", report.PersonnelDirectorName);
        Assert.Equal("manager Test", report.SummaryDeputyGeneralManagerName);
    }

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
        Assert.False((await other.GetReportAsync(id, null, seed.Worker)).IsSuccess);
        Assert.False((await other.GetReportAsync(id, seed.Unit, null)).IsSuccess);
        Assert.False((await Service("manager").GetReportAsync(id, seed.Unit, seed.Worker)).IsSuccess);
    }

    [Theory]
    [InlineData("revoked")]
    [InlineData("operatorLeft")]
    [InlineData("targetLeft")]
    public async Task DelegatedReportAccessIsLimitedToAssignedActivePersonnelAndRevokedImmediately(string change)
    {
        var seed = await Reset();
        var otherId = await PersonId("other");
        var managerId = await PersonId("manager");
        var hr = Service("hr", true);
        Success(await hr.SetDelegationsAsync(otherId, [seed.Worker], []));
        await using (var db = database.CreateDbContext())
        {
            var duty = new GorevTanim { Adi = "İdari İşler Görevlisi", BirimId = seed.Unit, Aktif = true };
            db.GorevTanim_Table.Add(duty);
            await db.SaveChangesAsync();
            (await db.IsBilgileri_Table.SingleAsync(x => x.PersonelId == otherId)).GorevId = duty.Id;
            await db.SaveChangesAsync();
        }
        Success(await Service("worker").SaveEntryAsync(Input()));
        var entry = await Entry();
        Success(await Service("manager").ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));
        // Unapproved entries must never appear on the printable chart.
        Success(await Service("worker").SaveEntryAsync(Input(Monday.AddDays(4))));
        var periodId = Success(await hr.CreatePeriodAsync("Yetkili çıktı testi", Monday, Monday.AddDays(6), Monday.AddDays(9)));
        var delegated = Service("other");
        var report = Success(await delegated.GetReportAsync(periodId, null, seed.Worker));
        Assert.Equal(seed.Worker, Assert.Single(report.Entries).PersonelId);
        Assert.Equal(entry.Id, Assert.Single(report.Entries).Id);
        Assert.False(report.CanSeeSummary);
        Assert.Equal("other Test", report.DelegateName);
        Assert.Equal("İdari İşler Görevlisi", report.DelegateDuty);
        Assert.Null(Success(await Service("worker").GetReportAsync(periodId, null, null)).DelegateName);
        Assert.Empty(report.Events);
        Assert.False((await delegated.GetReportAsync(periodId, null, managerId)).IsSuccess);
        Assert.False((await delegated.GetReportAsync(periodId, seed.Unit, null)).IsSuccess);
        Assert.False((await delegated.GetReportAsync(periodId, seed.Unit, seed.Worker)).IsSuccess);
        Assert.Empty(Success(await delegated.GetReportAsync(periodId, null, null)).Entries);
        if (change == "revoked")
            Success(await hr.SetDelegationsAsync(otherId, [], [seed.Worker]));
        else
        {
            await using var db = database.CreateDbContext();
            var id = change == "operatorLeft" ? otherId : seed.Worker;
            (await db.IsBilgileri_Table.SingleAsync(x => x.PersonelId == id)).CalismaDurumu = CalismaDurumu.Ayrildi;
            await db.SaveChangesAsync();
        }
        Assert.False((await delegated.GetReportAsync(periodId, null, seed.Worker)).IsSuccess);
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

    private async Task<int> PersonId(string login)
    {
        await using var db = database.CreateDbContext();
        return await db.Personel_Table.Where(x => x.KullaniciAdi == login).Select(x => x.Id).SingleAsync();
    }

    [Fact]
    public async Task OnlyHrCanManageEntryDelegations_AndGrantRequiresAUserAccount()
    {
        var seed = await Reset();
        var otherId = await PersonId("other");
        var ordinary = Service("other");
        Assert.False((await ordinary.GetDelegationsAsync()).IsSuccess);
        Assert.False((await ordinary.SetDelegationsAsync(otherId, [seed.Worker], [])).IsSuccess);
        var hr = Service("hr", true);
        Assert.False((await hr.SetDelegationsAsync(otherId, [otherId], [])).IsSuccess);
        Assert.False((await hr.SetDelegationsAsync(otherId, [int.MaxValue], [])).IsSuccess);
        await using (var db = database.CreateDbContext())
        {
            var noAccount = new Personel { Adi = "Hesapsız", Soyadi = "Personel", SicilNo = 99 };
            db.Personel_Table.Add(noAccount);
            await db.SaveChangesAsync();
            Assert.False((await hr.SetDelegationsAsync(noAccount.Id, [seed.Worker], [])).IsSuccess);
        }
        Assert.Empty(Success(await hr.GetDelegationsAsync()).Assignments);
    }

    [Fact]
    public async Task DelegateCanEnterForAssignedPersonnel_WithCorrectOwnerAndAudit_ButCannotApprove()
    {
        var seed = await Reset(40);
        var otherId = await PersonId("other");
        Success(await Service("hr", true).SetDelegationsAsync(otherId, [seed.Worker], []));
        var delegated = Service("other");
        var input = Input(); input.PersonelId = seed.Worker; input.Submit = false;
        Success(await delegated.SaveEntryAsync(input));

        var entry = await Entry();
        Assert.Equal(seed.Worker, entry.PersonelId);
        Assert.Equal("worker Test", entry.FullName);
        Assert.Equal("other", entry.Olusturan);
        Assert.Equal(seed.Unit, entry.UnitId);
        Assert.Equal(2400, entry.NormalWeeklyMinutes);
        Assert.Empty(Success(await delegated.LoadAsync("mine", Monday, Monday.AddDays(6))).Entries);
        var workspace = Success(await delegated.LoadAsync("delegated", Monday, Monday.AddDays(6)));
        Assert.Equal(seed.Worker, Assert.Single(workspace.DelegatedPeople).Id);
        Assert.Equal(entry.Id, Assert.Single(workspace.Entries).Id);
        Assert.Equal("other", Assert.Single(Success(await delegated.GetEntryHistoryAsync(entry.Id))).Olusturan);
        input.Id = entry.Id; input.Version = entry.Version; input.Submit = true;
        Success(await delegated.SaveEntryAsync(input));
        entry = await Entry();
        Assert.False((await delegated.ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, "")).IsSuccess);
        Success(await Service("manager").ReviewAsync(entry.Id, entry.Version, OvertimeEntryStatus.Approved, ""));
        input.Version = (await Entry()).Version;
        Assert.False((await delegated.SaveEntryAsync(input)).IsSuccess);
    }

    [Fact]
    public async Task RevocationImmediatelyBlocksReadsEditsWithdrawalsAndNewEntries_WithoutDeletingRecords()
    {
        var seed = await Reset();
        var otherId = await PersonId("other");
        var hr = Service("hr", true);
        Success(await hr.SetDelegationsAsync(otherId, [seed.Worker], []));
        var delegated = Service("other");
        var input = Input(); input.PersonelId = seed.Worker;
        Success(await delegated.SaveEntryAsync(input));
        var entry = await Entry();
        Success(await hr.SetDelegationsAsync(otherId, [], [seed.Worker]));

        input.Id = entry.Id; input.Version = entry.Version;
        Assert.False((await delegated.SaveEntryAsync(input)).IsSuccess);
        Assert.False((await delegated.WithdrawAsync(entry.Id, entry.Version)).IsSuccess);
        Assert.False((await delegated.GetEntryHistoryAsync(entry.Id)).IsSuccess);
        var newInput = Input(Monday.AddDays(4)); newInput.PersonelId = seed.Worker;
        Assert.False((await delegated.SaveEntryAsync(newInput)).IsSuccess);
        var workspace = Success(await delegated.LoadAsync("delegated", Monday, Monday.AddDays(6)));
        Assert.Empty(workspace.Entries);
        Assert.Empty(workspace.DelegatedPeople);
        Assert.Equal(entry.Id, (await Entry()).Id);
        await using var db = database.CreateDbContext();
        Assert.False(Assert.Single(await db.OvertimeDelegations.ToListAsync()).IsActive);
        Assert.Equal(2, await db.OvertimeEvents.CountAsync(x => x.Action == "Mesai giriş yetkileri güncellendi"));
    }

    [Fact]
    public async Task ReplacingDelegationsRemovesOldTargets_AndStaleChangesCannotOverwriteNewPermissions()
    {
        var seed = await Reset();
        var otherId = await PersonId("other");
        var managerId = await PersonId("manager");
        var hr = Service("hr", true);
        Success(await hr.SetDelegationsAsync(otherId, [seed.Worker], []));
        Success(await hr.SetDelegationsAsync(otherId, [managerId], [seed.Worker]));
        Assert.False((await hr.SetDelegationsAsync(otherId, [], [seed.Worker])).IsSuccess);
        Assert.Equal(managerId, Assert.Single(Success(await hr.GetDelegationsAsync()).Assignments).TargetPersonelId);
        var input = Input(); input.PersonelId = seed.Worker;
        Assert.False((await Service("other").SaveEntryAsync(input)).IsSuccess);
        Success(await hr.SetDelegationsAsync(otherId, [seed.Worker], [managerId]));
        Success(await Service("other").SaveEntryAsync(input));
        await using var db = database.CreateDbContext();
        Assert.Equal(2, await db.OvertimeDelegations.CountAsync());
        Assert.Equal(1, await db.OvertimeDelegations.CountAsync(x => x.IsActive));
    }

    [Fact]
    public async Task DelegationScopeCannotBeExpandedByTamperingWithPersonOrEntryIds()
    {
        var seed = await Reset();
        var otherId = await PersonId("other");
        var managerId = await PersonId("manager");
        var hr = Service("hr", true);
        var input = Input(); input.PersonelId = seed.Worker;
        Assert.False((await Service("other").SaveEntryAsync(input)).IsSuccess);
        Assert.False((await hr.SaveEntryAsync(input)).IsSuccess);
        Success(await hr.SetDelegationsAsync(otherId, [seed.Worker, managerId], []));
        input.Submit = false;
        Success(await Service("other").SaveEntryAsync(input));
        var entry = await Entry();
        input.Id = entry.Id; input.Version = entry.Version; input.PersonelId = managerId;
        Assert.False((await Service("other").SaveEntryAsync(input)).IsSuccess);
        Assert.Equal(seed.Worker, (await Entry()).PersonelId);
    }

    [Fact]
    public async Task OvernightDelegatedEntryKeepsAssignedOwnerOnBothDays()
    {
        var seed = await Reset();
        var otherId = await PersonId("other");
        Success(await Service("hr", true).SetDelegationsAsync(otherId, [seed.Worker], []));
        var input = Input(Monday.AddDays(4));
        input.PersonelId = seed.Worker;
        input.EndWorkDate = Monday.AddDays(5);
        input.Intervals = [new() { Start = new(22, 0), End = new(6, 0) }];
        Success(await Service("other").SaveEntryAsync(input));
        var entries = Success(await Service("worker").LoadAsync("mine", Monday, Monday.AddDays(6))).Entries;
        Assert.Equal(2, entries.Count);
        Assert.All(entries, x => { Assert.Equal(seed.Worker, x.PersonelId); Assert.Equal("other", x.Olusturan); });
    }

    [Fact]
    public async Task PersonnelWithoutLoginCanBeATarget_AndTheirOwnWorkScheduleIsUsed()
    {
        var seed = await Reset(40);
        var otherId = await PersonId("other");
        int targetId;
        await using (var db = database.CreateDbContext())
        {
            var driver = new Personel { Adi = "Şoför", Soyadi = "Test", SicilNo = 88 };
            var title = new UnvanTanim { Adi = "Şoför", GorTipId = 1 };
            db.Personel_Table.Add(driver);
            db.UnvanTanim_Table.Add(title);
            await db.SaveChangesAsync();
            targetId = driver.Id;
            db.IsBilgileri_Table.Add(new() { PersonelId = driver.Id, BirimId = seed.Unit, UnvanId = title.Id, CalismaDurumu = CalismaDurumu.Calisiyor });
            db.OvertimeSchedules.Add(new() { TitleId = title.Id, WeeklyHours = 45 });
            await db.SaveChangesAsync();
        }
        Success(await Service("hr", true).SetDelegationsAsync(otherId, [targetId], []));
        var input = Input(); input.PersonelId = targetId;
        Success(await Service("other").SaveEntryAsync(input));
        var workspace = Success(await Service("other").LoadAsync("delegated", Monday, Monday.AddDays(6)));
        Assert.Equal("", Assert.Single(workspace.DelegatedPeople).UserName);
        var entry = Assert.Single(workspace.Entries);
        Assert.Equal(targetId, entry.PersonelId);
        Assert.Equal(2700, entry.NormalWeeklyMinutes);
        Assert.Equal("Şoför Test", entry.FullName);
    }

    [Fact]
    public async Task DelegationPeopleListsExcludeDepartedPersonnelAndPeopleWithoutWorkInformation()
    {
        var seed = await Reset();
        var otherId = await PersonId("other");
        int departedId, noInfoId;
        await using (var db = database.CreateDbContext())
        {
            var departed = new Personel { Adi = "Ayrılan", Soyadi = "Personel", KullaniciAdi = "departed", SicilNo = 77 };
            var noInfo = new Personel { Adi = "Bilgisiz", Soyadi = "Personel", KullaniciAdi = "noinfo", SicilNo = 78 };
            db.Personel_Table.AddRange(departed, noInfo);
            await db.SaveChangesAsync();
            departedId = departed.Id; noInfoId = noInfo.Id;
            db.IsBilgileri_Table.Add(new() { PersonelId = departedId, BirimId = seed.Unit, UnvanId = seed.Title, CalismaDurumu = CalismaDurumu.Ayrildi });
            await db.SaveChangesAsync();
        }
        var hr = Service("hr", true);
        var people = Success(await hr.GetDelegationsAsync()).People;
        Assert.Contains(people, x => x.Id == seed.Worker);
        Assert.DoesNotContain(people, x => x.Id == departedId || x.Id == noInfoId);
        Assert.False((await hr.SetDelegationsAsync(otherId, [departedId], [])).IsSuccess);
        Assert.False((await hr.SetDelegationsAsync(otherId, [noInfoId], [])).IsSuccess);
        Assert.False((await hr.SetDelegationsAsync(departedId, [seed.Worker], [])).IsSuccess);
        Assert.False((await hr.SetDelegationsAsync(noInfoId, [seed.Worker], [])).IsSuccess);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExistingDelegationStopsWhenOperatorOrTargetIsNoLongerWorking(bool operatorLeaves)
    {
        var seed = await Reset();
        var otherId = await PersonId("other");
        var hr = Service("hr", true);
        var delegated = Service("other");
        Success(await hr.SetDelegationsAsync(otherId, [seed.Worker], []));
        var input = Input(); input.PersonelId = seed.Worker;
        Success(await delegated.SaveEntryAsync(input));
        var entry = await Entry();
        await using (var db = database.CreateDbContext())
        {
            var id = operatorLeaves ? otherId : seed.Worker;
            var info = await db.IsBilgileri_Table.SingleAsync(x => x.PersonelId == id);
            info.CalismaDurumu = CalismaDurumu.Ayrildi;
            await db.SaveChangesAsync();
        }

        var workspace = Success(await delegated.LoadAsync("delegated", Monday, Monday.AddDays(6)));
        Assert.Empty(workspace.DelegatedPeople);
        Assert.Empty(workspace.Entries);
        Assert.False((await delegated.GetEntryHistoryAsync(entry.Id)).IsSuccess);
        var newInput = Input(Monday.AddDays(4)); newInput.PersonelId = seed.Worker;
        Assert.False((await delegated.SaveEntryAsync(newInput)).IsSuccess);
        Assert.False((await hr.SetDelegationsAsync(otherId, [seed.Worker], [seed.Worker])).IsSuccess);
        Success(await hr.SetDelegationsAsync(otherId, [], [seed.Worker]));
        Assert.Equal(entry.Id, (await Entry()).Id);
    }
}
