using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using Status = Jenga.Models.IKYS.IzinTalep.OnayDurumuTipi;

namespace Jenga.Overtime.Tests;

public sealed class LeaveRequestWorkflowTests
{
    private static readonly DateTime Friday = new(2026, 11, 13);
    private static readonly DateTime EntitlementStart = new(2018, 3, 1);

    [Theory]
    [InlineData(LeaveType.Paternity)]
    [InlineData(LeaveType.Marriage)]
    [InlineData(LeaveType.Bereavement)]
    public async Task CalendarLeaveCountsBothDatesAndWeekend(LeaveType type)
    {
        await using var data = await TestData.CreateAsync();
        var service = data.Service("employee");
        var input = Request(type, Friday, Friday.AddDays(3));

        Assert.Equal("1", Success(await service.CalculateAsync(Request(type, Friday, Friday), false)).Duration);
        Assert.Equal("4", Success(await service.CalculateAsync(input, false)).Duration);
        var id = Success(await service.SaveAsync(input, false)).Id;
        var row = Success(await service.DetailAsync(id, false)).Row;
        Assert.Equal("4", row.Duration);
        Assert.Equal(Status.IslemBekliyor, row.Status);
        Assert.Equal("İzin " + (int)type, Assert.Single(Success(await service.ListAsync(false, LeaveListFilter.General, null, 0)).Items).TypeName);
    }

    [Fact]
    public async Task CompleteTableListKeepsPersonnelAndStatusScopeAcrossPages()
    {
        await using var data = await TestData.CreateAsync();
        var size = ILeaveRequestWorkflow.PageSize;
        await using (var db = data.CreateDbContext())
        {
            db.IzinTalep_Table.AddRange(Enumerable.Range(0, size + 2).Select(day => new IzinTalep
            {
                PersonelId = 2, Aktif = true, IzinTipi = (int)LeaveType.Paternity,
                BaslangicTarihi = Friday.AddDays(day), OnayDurumu = Status.IslemBekliyor
            }));
            db.IzinTalep_Table.AddRange(
                new IzinTalep { PersonelId = 3, Aktif = true, IzinTipi = (int)LeaveType.Paternity, OnayDurumu = Status.IslemBekliyor },
                new IzinTalep { PersonelId = 2, Aktif = true, IzinTipi = (int)LeaveType.Personal, OnayDurumu = Status.KontrolEdildi });
            await db.SaveChangesAsync();
        }

        var employee = data.Service("employee");
        var complete = Success(await employee.ListAsync(false, LeaveListFilter.General, Status.IslemBekliyor));
        var firstPage = Success(await employee.ListAsync(false, LeaveListFilter.General, Status.IslemBekliyor, 0));
        Assert.Equal(size + 2, complete.Total);
        Assert.Equal(size + 2, complete.Items.Count);
        Assert.Equal(complete.Items.Take(size).Select(x => x.Id), firstPage.Items.Select(x => x.Id));
        Assert.All(complete.Items, row => Assert.Equal("Personel employee", row.Person));
        Assert.True((await employee.ListAsync(true, LeaveListFilter.All, null)).IsFailure);
    }

    [Fact]
    public async Task OfficeListIncludesOnlyPersonnelWithWorkingStatus()
    {
        await using var data = await TestData.CreateAsync();
        await using (var db = data.CreateDbContext())
        {
            (await db.Personel_Table.FindAsync(2))!.Tipi = PersonelTipi.KadroHarici;
            (await db.IsBilgileri_Table.SingleAsync(x => x.PersonelId == 3)).CalismaDurumu = CalismaDurumu.Ayrildi;
            db.Personel_Table.Add(new Personel { Id = 4, Adi = "İş Bilgisi", Soyadi = "Yok", SicilNo = 4 });
            db.IzinTalep_Table.AddRange(
                StoredRequest(302, LeaveType.Paid, Status.IslemBekliyor, "1", Friday, owner: 2),
                StoredRequest(303, LeaveType.Paid, Status.IslemBekliyor, "1", Friday, owner: 3),
                StoredRequest(304, LeaveType.Paid, Status.IslemBekliyor, "1", Friday, owner: 4),
                StoredRequest(305, LeaveType.Personal, Status.KayitlaraIslendi, "01:00:00", Friday, owner: 2));
            await db.SaveChangesAsync();
        }

        var office = data.Service("office", manage: true);
        var all = Success(await office.ListAsync(true, LeaveListFilter.All, null));
        Assert.Equal(2, all.Total);
        Assert.Equal(new[] { 302, 305 }, all.Items.Select(request => request.Id));

        var page = Success(await office.ListAsync(true, LeaveListFilter.General, Status.IslemBekliyor, 0));
        Assert.Equal(1, page.Total);
        Assert.Equal(302, Assert.Single(page.Items).Id);

        var personal = Success(await data.Service("stranger").ListAsync(false, LeaveListFilter.All, null));
        Assert.Equal(303, Assert.Single(personal.Items).Id);
    }

    [Fact]
    public async Task PendingOverviewAndPaidCalculationExcludeClosedInactiveAndAlreadyAppliedRequests()
    {
        await using var data = await TestData.CreateAsync();
        var input = Request(LeaveType.Paid, Friday, Friday.AddDays(12));
        await using (var db = data.CreateDbContext())
        {
            db.IzinTalep_Table.AddRange(
                StoredRequest(100, LeaveType.Paid, Status.IslemBekliyor, "3", Friday),
                StoredRequest(101, LeaveType.Paid, Status.KontrolEdildi, "4", Friday),
                StoredRequest(102, LeaveType.Paid, Status.DilekceBekleniyor, "5", Friday),
                StoredRequest(103, LeaveType.Paid, Status.Reddedildi, "10", Friday),
                StoredRequest(104, LeaveType.Paid, Status.IptalEdildi, "10", Friday),
                StoredRequest(105, LeaveType.Paid, Status.KayitlaraIslendi, "10", Friday),
                StoredRequest(106, LeaveType.Paid, Status.IslemBekliyor, "10", Friday, active: false),
                StoredRequest(107, LeaveType.Paid, Status.IslemBekliyor, "10", Friday, owner: 3),
                StoredRequest(108, LeaveType.Paid, Status.KontrolEdildi, "10", Friday),
                StoredRequest(109, LeaveType.Marriage, Status.IslemBekliyor, "3", Friday));
            db.IzinHareket_Table.Add(new IzinHareket { PersonelId = 2, IzinTalepId = 108 });
            await db.SaveChangesAsync();
        }

        var employee = data.Service("employee");
        var pending = Success(await employee.ListPendingForEntryAsync(input, false));
        Assert.Equal(new[] { 100, 101, 102, 109 }, pending.Select(x => x.Id).Order());
        Assert.True((await data.Service("stranger").ListPendingForEntryAsync(input, false)).IsFailure);

        var calculation = Success(await employee.CalculateAsync(input, false));
        Assert.Equal("9", calculation.Duration);
        Assert.Equal("20", calculation.Available);
        Assert.Equal("12", calculation.PendingDuration);
        Assert.Equal("8", calculation.AvailableAfterPending);
        Assert.Equal("-1", calculation.Remaining);
        Assert.NotNull(calculation.BalanceWarning);

        var edit = Success(await employee.LoadFormAsync(100, false)).Input;
        Assert.DoesNotContain(Success(await employee.ListPendingForEntryAsync(edit, false)), row => row.Id == 100);
        var editedCalculation = Success(await employee.CalculateAsync(edit, false));
        Assert.Equal("9", editedCalculation.PendingDuration);
        Assert.Equal("10", editedCalculation.Remaining);
        Assert.Null(editedCalculation.BalanceWarning);
        await using var persisted = data.CreateDbContext();
        Assert.Empty(await persisted.IzinDonem_Table.ToListAsync());
    }

    [Fact]
    public async Task PendingPersonalLeaveUsesTheSamePeriodAndCalendarLeaveReportsCombinedDays()
    {
        await using var data = await TestData.CreateAsync();
        var employee = data.Service("employee");
        var personal = Request(LeaveType.Personal, Friday, Friday);
        personal.StartTime = new TimeOnly(11, 30);
        personal.EndTime = new TimeOnly(13, 30);
        Assert.True((await employee.SaveAsync(personal, false)).IsSuccess);
        await using (var db = data.CreateDbContext())
        {
            db.IzinTalep_Table.AddRange(
                StoredRequest(201, LeaveType.Personal, Status.KontrolEdildi, "04:00:00", Friday.AddYears(-1)),
                StoredRequest(202, LeaveType.Personal, Status.KontrolEdildi, "02:00:00", Friday),
                StoredRequest(203, LeaveType.Marriage, Status.IslemBekliyor, "2", Friday));
            await db.SaveChangesAsync();
        }

        var personalCalculation = Success(await employee.CalculateAsync(personal, false));
        Assert.Equal("03:00", personalCalculation.PendingDuration);
        Assert.Equal("24:00", personalCalculation.AvailableAfterPending);
        Assert.Equal("23:00", personalCalculation.Remaining);

        var marriage = Success(await employee.CalculateAsync(Request(LeaveType.Marriage, Friday, Friday), false));
        Assert.Equal("2", marriage.PendingDuration);
        Assert.Equal("3", marriage.CombinedDuration);
        Assert.Null(marriage.AvailableAfterPending);
    }

    [Fact]
    public async Task PaidApprovalWritesOneMovementAndUsesTheSharedBalance()
    {
        await using var data = await TestData.CreateAsync();
        await using (var db = data.CreateDbContext())
        {
            db.ResmiTatil_Table.Add(new()
            {
                Tatil = "Test tatili", Yil = 2026, Ay = 11, Gun = 16,
                BaslamaTarihi = Friday.AddDays(3), BitisTarihi = Friday.AddDays(3).AddHours(17)
            });
            await db.SaveChangesAsync();
        }

        var employee = data.Service("employee");
        var office = data.Service("office", manage: true);
        var save = Success(await employee.SaveAsync(Request(LeaveType.Paid, Friday, Friday.AddDays(3)), false));
        var id = save.Id;
        Assert.False(save.NotificationFailed);
        Assert.Equal(2, data.Email.Messages.Count);
        Assert.True((await office.DecideAsync(id, LeaveDecision.Approve, null, true)).IsFailure);
        Assert.True((await employee.DecideAsync(id, LeaveDecision.Check, null, false)).IsFailure);
        Assert.True((await office.DecideAsync(id, LeaveDecision.Check, null, false)).IsSuccess);
        Assert.Contains("UcretlIzinBelgesiViewer.aspx", Success(await employee.DetailAsync(id, false)).DocumentUrl);
        Assert.True((await office.DecideAsync(id, LeaveDecision.Approve, null, false)).IsFailure);
        Assert.True((await employee.DeleteAsync(id, false)).IsFailure);

        await using (var db = data.CreateDbContext())
        {
            Assert.Empty(await db.IzinHareket_Table.ToListAsync());
            Assert.Equal("20", (await db.IzinDonem_Table.SingleAsync()).KalanIzin);
        }
        Assert.True((await office.DecideAsync(id, LeaveDecision.Approve, null, true)).IsSuccess);
        Assert.True((await office.DecideAsync(id, LeaveDecision.Approve, null, true)).IsFailure);
        await using (var db = data.CreateDbContext())
        {
            var movement = await db.IzinHareket_Table.SingleAsync();
            var period = await db.IzinDonem_Table.SingleAsync();
            Assert.Equal(id, movement.IzinTalepId);
            Assert.Equal("1", movement.Sure);
            Assert.Equal("20", movement.OncekiIzinStr);
            Assert.Equal("19", movement.KalanIzinStr);
            Assert.Equal("1", period.KullanilanIzin);
            Assert.Equal("19", period.KalanIzin);
            Assert.Equal("office", movement.Olusturan);
        }
    }

    [Fact]
    public async Task PersonalLeaveRevalidatesBalanceAndCanBeRejectedAfterControl()
    {
        await using var data = await TestData.CreateAsync();
        var employee = data.Service("employee");
        var office = data.Service("office", manage: true);
        var input = Request(LeaveType.Personal, Friday, Friday);
        input.StartTime = new TimeOnly(11, 30);
        input.EndTime = new TimeOnly(13, 30);
        Assert.Equal("01:00:00", Success(await employee.CalculateAsync(input, false)).Duration);
        Assert.Equal("27:00", Success(await employee.CalculateAsync(input, false)).Available);
        var id = Success(await employee.SaveAsync(input, false)).Id;
        Assert.True((await employee.SaveAsync(input, false)).IsFailure);
        Assert.Empty(Success(await office.ListAsync(true, LeaveListFilter.General, null, 0)).Items);
        Assert.Single(Success(await office.ListAsync(true, LeaveListFilter.Personal, null, 0)).Items);
        Assert.True((await office.DecideAsync(id, LeaveDecision.Check, null, false)).IsSuccess);
        await using (var db = data.CreateDbContext())
        {
            var period = await db.IzinDonem_Table.SingleAsync();
            period.KullanilanIzin = "1.03:00:00";
            period.KalanIzin = "00:00:00";
            await db.SaveChangesAsync();
        }
        Assert.True((await office.DecideAsync(id, LeaveDecision.Approve, null, true)).IsFailure);
        Assert.Equal(Status.KontrolEdildi, Success(await employee.DetailAsync(id, false)).Row.Status);

        data.Email.Fail = true;
        var rejected = Success(await office.DecideAsync(id, LeaveDecision.Reject, "Tarih uygun değil.", false));
        Assert.True(rejected.NotificationFailed);
        var detail = Success(await employee.DetailAsync(id, false));
        Assert.Equal(Status.Reddedildi, detail.Row.Status);
        Assert.Equal("Tarih uygun değil.", detail.Description);
        Assert.Empty(detail.Movements);
    }

    [Fact]
    public async Task OfficeCanEnterLeaveForWorkingPersonnelAcrossStaffTypes()
    {
        await using var data = await TestData.CreateAsync();
        await using (var db = data.CreateDbContext())
        {
            (await db.Personel_Table.FindAsync(2))!.Tipi = PersonelTipi.KadroHarici;
            db.Personel_Table.Add(new Personel
            {
                Id = 4, Adi = "Eski", Soyadi = "Personel", KullaniciAdi = "former", SicilNo = 4, Tipi = PersonelTipi.Kadrolu
            });
            db.IsBilgileri_Table.Add(new IsBilgileri
            {
                PersonelId = 4, BirimId = 1, UnvanId = 1, CalismaDurumu = CalismaDurumu.Ayrildi,
                BaslamaTar = EntitlementStart, IzinDonemiBasTar = EntitlementStart
            });
            await db.SaveChangesAsync();
        }

        var office = data.Service("office", manage: true);
        var employee = data.Service("employee");
        var available = Success(await office.ListPersonnelForEntryAsync());
        Assert.Equal(new[] { 1, 2, 3 }, available.Select(person => person.Id).Order());
        Assert.True((await employee.ListPersonnelForEntryAsync()).IsFailure);
        var form = Success(await office.LoadFormAsync(null, true));
        Assert.Equal(available, form.People);
        Assert.DoesNotContain(form.Signers, person => person.Id == 2);

        var input = Request(LeaveType.Paternity, Friday, Friday);
        Assert.True((await employee.SaveAsync(input, true)).IsFailure);
        var requestId = Success(await office.SaveAsync(input, true)).Id;
        Assert.Equal(requestId, Assert.Single(Success(await employee.ListAsync(false, LeaveListFilter.All, null)).Items).Id);
        Assert.Empty(Success(await office.ListAsync(false, LeaveListFilter.All, null)).Items);

        input.PersonelId = 4;
        Assert.True((await office.SaveAsync(input, true)).IsFailure);
    }

    [Fact]
    public async Task ScopedCrudAndLegacyPetitionUseTheSameWorkflow()
    {
        await using var data = await TestData.CreateAsync();
        var employee = data.Service("employee");
        var stranger = data.Service("stranger");
        var office = data.Service("office", manage: true);
        var id = Success(await employee.SaveAsync(Request(LeaveType.Marriage, Friday, Friday), false)).Id;
        Assert.True((await stranger.DetailAsync(id, false)).IsFailure);
        Assert.True((await stranger.ListAsync(true, LeaveListFilter.All, null, 0)).IsFailure);
        Assert.True((await stranger.SaveAsync(Request(LeaveType.Marriage, Friday, Friday), false)).IsFailure);
        Assert.True((await stranger.DeleteAsync(id, false)).IsFailure);

        var form = Success(await employee.LoadFormAsync(id, false));
        form.Input.EndDate = Friday.AddDays(1);
        Assert.True((await employee.SaveAsync(form.Input, false)).IsSuccess);
        Assert.Equal("2", Success(await employee.DetailAsync(id, false)).Row.Duration);
        Assert.True((await employee.DeleteAsync(id, false)).IsSuccess);

        var legacyId = Success(await employee.SaveAsync(Request(LeaveType.Paternity, Friday, Friday), false)).Id;
        await using (var db = data.CreateDbContext())
        {
            var legacy = await db.IzinTalep_Table.FindAsync(legacyId);
            legacy!.OnayDurumu = Status.DilekceBekleniyor;
            legacy.Sure = "0";
            await db.SaveChangesAsync();
        }
        Assert.True((await office.DecideAsync(legacyId, LeaveDecision.Check, null, false)).IsSuccess);
        Assert.Equal("1", Success(await employee.DetailAsync(legacyId, false)).Row.Duration);
        Assert.True((await office.DecideAsync(legacyId, LeaveDecision.Approve, null, true)).IsSuccess);
    }

    [Theory]
    [InlineData(AskerSivil.Sivil, 3)]
    [InlineData(AskerSivil.Asker, 0)]
    public async Task UnpaidLeaveUpdatesOnlyCivilianEntitlementStart(AskerSivil employment, int expectedDays)
    {
        await using var data = await TestData.CreateAsync();
        await using (var db = data.CreateDbContext())
        {
            (await db.Personel_Table.FindAsync(2))!.AskerSivil = employment;
            await db.SaveChangesAsync();
        }
        var office = data.Service("office", manage: true);
        var id = Success(await office.SaveAsync(Request(LeaveType.Unpaid, Friday, Friday.AddDays(2)), true)).Id;
        Assert.True((await office.DecideAsync(id, LeaveDecision.Check, null, false)).IsSuccess);
        Assert.True((await office.DecideAsync(id, LeaveDecision.Approve, null, true)).IsSuccess);
        await using var persisted = data.CreateDbContext();
        Assert.Equal(EntitlementStart.AddDays(expectedDays), (await persisted.IsBilgileri_Table.SingleAsync(x => x.PersonelId == 2)).IzinDonemiBasTar);
        Assert.Equal("3", (await persisted.IzinHareket_Table.SingleAsync()).Sure);
    }

    private static LeaveRequestInput Request(LeaveType type, DateTime start, DateTime end) => new()
    {
        Type = type, StartDate = start, EndDate = end, PersonelId = 2, ManagerId = 1, SubstituteId = 3,
        Address = "Ankara", Notify = false
    };

    private static IzinTalep StoredRequest(int id, LeaveType type, Status status, string duration, DateTime start,
        int owner = 2, bool active = true) => new()
    {
        Id = id, PersonelId = owner, IzinTipi = (int)type, Aktif = active, OnayDurumu = status, Sure = duration,
        Birim = type == LeaveType.Personal ? "Saat" : "Gün", BaslangicTarihi = start, BitisTarihi = start.AddHours(17)
    };

    private static T Success<T>(Result<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.Error.Message} {result.Error.Exception}");
        return result.Value;
    }

    private sealed class Authorization(string name, bool manage) : IServiceAuthorizationContext
    {
        public Task<bool> HasPermissionAsync(ModuleName module, Operation operation, CancellationToken cancellationToken = default) =>
            Task.FromResult(module == ModuleName.IKYS && (operation != Operation.Manage || manage));
        public Task<string?> GetUserNameAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(name);
        public Task<IReadOnlyList<int>> GetAuthorizedRegionIdsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<int>>([]);
    }

    private sealed class TestEmail : IEmailService
    {
        public List<string> Messages { get; } = [];
        public bool Fail { get; set; }
        public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default) =>
            SendAsync([to], subject, htmlBody, cancellationToken);
        public Task SendAsync(IEnumerable<string> recipients, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            if (Fail) throw new InvalidOperationException("Test bildirimi gönderilemedi.");
            Messages.Add(htmlBody);
            return Task.CompletedTask;
        }
    }

    private sealed class TestData : IDbContextFactory<ApplicationDbContext>, IAsyncDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private readonly IConfiguration _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["LeaveRequests:PortalDocumentBaseUrl"] = "https://portal.example.test"
        }).Build();
        public TestEmail Email { get; } = new();

        public LeaveRequestWorkflow Service(string name, bool manage = false) =>
            new(this, new Authorization(name, manage), NullLogService.Instance, Email, _configuration);

        public ApplicationDbContext CreateDbContext() =>
            new TestContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);

        public static async Task<TestData> CreateAsync()
        {
            var data = new TestData();
            await data._connection.OpenAsync();
            await using var db = data.CreateDbContext();
            await db.Database.EnsureCreatedAsync();
            db.Personel_Table.AddRange(new[] { "office", "employee", "stranger" }.Select((name, i) =>
                new Personel { Id = i + 1, Adi = "Personel", Soyadi = name, SicilNo = i + 1, KullaniciAdi = name, Tipi = PersonelTipi.Kadrolu }));
            db.BirimTanim_Table.Add(new() { Id = 1, Adi = "Birim", KisaAdi = "B", ParentId = 0, AmirId = 1, Aktif = true, Sira = 1, BolgeId = 1 });
            db.UnvanTanim_Table.Add(new() { Id = 1, Adi = "Uzman", GorTipId = 1 });
            db.IzinTanim_Table.AddRange(Enum.GetValues<LeaveType>().Select(type => new IzinTanim { Id = (int)type, Adi = "İzin " + (int)type }));
            await db.SaveChangesAsync();
            db.IsBilgileri_Table.AddRange(Enumerable.Range(1, 3).Select(id => new IsBilgileri
            {
                PersonelId = id, BirimId = 1, UnvanId = 1, CalismaDurumu = CalismaDurumu.Calisiyor,
                BaslamaTar = EntitlementStart, IzinDonemiBasTar = EntitlementStart
            }));
            await db.SaveChangesAsync();
            return data;
        }

        public ValueTask DisposeAsync() => _connection.DisposeAsync();

        private sealed class TestContext(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                var keep = new HashSet<Type>
                {
                    typeof(Personel), typeof(IsBilgileri), typeof(BirimTanim), typeof(UnvanTanim), typeof(GorevTanim),
                    typeof(IletisimBilgileri), typeof(IzinTalep), typeof(IzinTanim), typeof(IzinDonem), typeof(IzinHareket),
                    typeof(ResmiTatil), typeof(Aile)
                };
                foreach (var type in modelBuilder.Model.GetEntityTypes().Select(x => x.ClrType).Where(x => !keep.Contains(x)).ToList())
                    modelBuilder.Ignore(type);
            }
        }
    }
}
