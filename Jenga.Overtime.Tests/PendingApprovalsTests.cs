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

public sealed class PendingApprovalsTests
{
    [Fact]
    public async Task ReportLoadsDutyAdvanceVehicleAndRecordedSignatories()
    {
        await using var data = await TestData.CreateAsync();
        await SeedListsAsync(data);
        await using var db = data.CreateDbContext();
        var task = await db.GorevOnay_Table.SingleAsync(x => x.Id == 102);
        task.Avans = "500,00 TL"; task.AracPlakasi = "06 TEST 01";
        task.AracTahsisi = false; task.UlasimAraci = "Vakıf Aracı";
        task.OnayImza = 2; task.PerSubeImza = 1; task.PerSubeVekil = true; task.OnayMakam = 1;
        db.BirimTanim_Table.Add(new() { Id = 2, Adi = "Muhasebe ve Finans Direktörlüğü", KisaAdi = "Muh.ve Fin.Dir.", ParentId = 0, AmirId = 3, Aktif = true, Sira = 2, BolgeId = 1 });
        db.IsBilgileri_Table.Add(new() { PersonelId = 3, BirimId = 2, UnvanId = 1, CalismaDurumu = CalismaDurumu.Calisiyor });
        var duty = new GorevTanim { BirimId = 1, Adi = "Kıdemli Uzman", KisaAdi = "Kd.Uzm.", Aktif = true };
        db.GorevTanim_Table.Add(duty);
        await db.SaveChangesAsync();
        var job = await db.IsBilgileri_Table.FirstAsync(x => x.PersonelId == 2);
        job.GorevId = duty.Id;
        await db.SaveChangesAsync();
        var row = Assert.Single(Success(await data.Service().GetTaskApprovalReportItemsAsync([102], null, true)));
        Assert.Equal("Kd.Uzm.", row.DutyTitle);
        Assert.Equal("500,00 TL", row.AdvanceText);
        Assert.Equal("06 TEST 01", row.VehiclePlate);
        Assert.False(row.VehicleRequested);
        Assert.Equal(2, row.SpecialistSignature.PersonelId);
        Assert.NotEmpty(row.SpecialistSignature.Name);
        Assert.Equal("Kd.Uzm.", row.SpecialistSignature.Title);
        Assert.Equal(1, row.PersonnelDirectorSignature.PersonelId);
        Assert.True(row.PersonnelDirectorSignature.IsDeputy);
        Assert.Equal(3, row.FinanceDirectorSignature.PersonelId);
        Assert.Equal("Personel 3", row.FinanceDirectorSignature.Name);
        Assert.Equal("Muh. ve Fin. Dir.", row.FinanceDirectorSignature.Title);
    }

    [Theory]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public async Task FinanceSignatureDoesNotUseInactiveOrAmbiguousManagers(bool activeUnit, bool activePerson, bool ambiguous)
    {
        await using var data = await TestData.CreateAsync();
        await SeedListsAsync(data);
        await using var db = data.CreateDbContext();
        db.BirimTanim_Table.Add(new() { Id = 2, Adi = "Muhasebe ve Finans Direktörlüğü", KisaAdi = "MFD", ParentId = 0, AmirId = 3, Aktif = activeUnit, Sira = 2, BolgeId = 1 });
        db.IsBilgileri_Table.Add(new() { PersonelId = 3, BirimId = 2, UnvanId = 1, CalismaDurumu = activePerson ? CalismaDurumu.Calisiyor : CalismaDurumu.Ayrildi });
        if (ambiguous)
        {
            db.BirimTanim_Table.Add(new() { Id = 3, Adi = "Muhasebe ve Finans Direktörlüğü", KisaAdi = "MFD2", ParentId = 0, AmirId = 1, Aktif = true, Sira = 3, BolgeId = 1 });
            db.IsBilgileri_Table.Add(new() { PersonelId = 1, BirimId = 3, UnvanId = 1, CalismaDurumu = CalismaDurumu.Calisiyor });
        }
        (await db.GorevOnay_Table.SingleAsync(x => x.Id == 102)).OnayMakam = 2;
        await db.SaveChangesAsync();
        var row = Assert.Single(Success(await data.Service().GetTaskApprovalReportItemsAsync([102], null, true)));
        Assert.Null(row.FinanceDirectorSignature.PersonelId);
        Assert.Empty(row.FinanceDirectorSignature.Name);
        Assert.Equal("Muh. ve Fin. Dir.", row.FinanceDirectorSignature.Title);
    }

    [Fact]
    public async Task NullManagerApprovalLoadsAsOtherWithoutGrantingActions()
    {
        await using var data = await TestData.CreateAsync();
        await using var db = data.CreateDbContext();
        var task = new GorevOnay
        {
            PersonelId = 2, AmirOnayi = null, Odendi = false,
            BaslangicTarihi = DateTime.Today.AddHours(8), BitisTarihi = DateTime.Today.AddHours(17)
        };
        db.GorevOnay_Table.Add(task);
        await db.SaveChangesAsync();
        var service = data.Service();
        foreach (var general in new[] { true, false })
        {
            var item = Success(await service.GetTaskApprovalListAsync(2, general)).Single(x => x.TaskApprovalId == task.Id);
            Assert.Equal(ApprovalStatus.Other, item.ManagerApprovalStatus);
            Assert.False(item.CanReport);
            Assert.False(item.CanEditGeneral);
            Assert.False(item.CanEditPersonal(DateTime.Now));
        }
        Assert.True((await service.GetTaskApprovalReportItemsAsync([task.Id], null, true)).IsFailure);
        Assert.True((await service.SetTaskApprovalsPaidAsync([task.Id])).IsFailure);
        Assert.True((await service.GetScopedByIdAsync(task.Id, 2, false)).IsFailure);
        Assert.True((await service.ApproveAsync(task.Id, 1)).IsFailure);
        Assert.Null((await db.GorevOnay_Table.AsNoTracking().SingleAsync(x => x.Id == task.Id)).AmirOnayi);
    }

    [Fact]
    public async Task EmployeeWithoutManagedUnitsSeesOnlyOwnPendingAndFiveLatestApprovedTasks()
    {
        await using var data = await TestData.CreateAsync();
        var result = Success(await data.Service().GetPendingApprovalsAsync(2, false));
        Assert.False(result.HasManagerUnits);
        Assert.Empty(result.ReviewItems);
        Assert.Equal(20, Assert.Single(result.PendingTasks).GorevOnayId);
        Assert.Equal(new[] { 35, 34, 33, 32, 31 }, result.RecentApprovedTasks.Select(x => x.GorevOnayId));
        Assert.All(result.PendingTasks.Concat(result.RecentApprovedTasks), x => Assert.Equal(2, x.PersonelId));
        Assert.Equal("Açıklama", result.PendingTasks[0].Aciklama);
    }

    [Fact]
    public async Task ManagerReviewIsScopedAndPendingTasksComeFirst()
    {
        await using var data = await TestData.CreateAsync();
        var result = Success(await data.Service().GetPendingApprovalsAsync(1, false));
        Assert.True(result.HasManagerUnits);
        Assert.Equal(20, result.ReviewItems[0].GorevOnayId);
        Assert.All(result.ReviewItems, x => Assert.Equal(2, x.PersonelId));
        Assert.Contains(result.ReviewItems, x => x.ApprovalStatus == ApprovalStatus.Rejected && x.OnayRedAciklama == "Gerekçe");
        Assert.Empty(result.PendingTasks);
        Assert.Empty(result.RecentApprovedTasks);
    }

    [Fact]
    public async Task IkysViewIncludesOtherUnitsAndPersonnelWithoutJobInformation()
    {
        await using var data = await TestData.CreateAsync();
        var result = Success(await data.Service().GetPendingApprovalsAsync(null, true));
        Assert.True(result.HasManagerUnits);
        Assert.Equal(new[] { 40, 20 }, result.PendingTasks.Select(x => x.GorevOnayId));
        Assert.Contains(result.ReviewItems, x => x.PersonelId == 3);
        Assert.Equal(5, result.RecentApprovedTasks.Count);
    }

    [Fact]
    public async Task GlobalViewAndDecisionsRequireManagePermission()
    {
        await using var data = await TestData.CreateAsync();
        var service = data.Service(manage: false);
        Assert.True((await service.GetPendingApprovalsAsync(2, true)).IsFailure);
        Assert.True((await service.ApproveAsync(40, 1, authorizedIkysView: true)).IsFailure);
        Assert.True((await service.RejectAsync(20, 1, "Gerekçe")).IsFailure);
        Assert.True((await data.Service(view: false).GetPendingApprovalsAsync(2, false)).IsFailure);
        Assert.True((await data.Service().GetPendingApprovalsAsync(null, false)).IsFailure);
        Assert.Equal(20, Assert.Single(Success(await service.GetPendingApprovalsAsync(2, false)).PendingTasks).GorevOnayId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DecisionRespectsUnitScopeAndRefreshesReadOnlyLists(bool reject)
    {
        await using var data = await TestData.CreateAsync();
        var service = data.Service();
        var denied = reject ? await service.RejectAsync(40, 1, "Gerekçe") : await service.ApproveAsync(40, 1);
        Assert.True(denied.IsFailure);
        Assert.True((await service.RejectAsync(20, 1, " ")).IsFailure);
        var result = reject
            ? await service.RejectAsync(20, 1, "Uygun değil", "manager")
            : await service.ApproveAsync(20, 1, "manager");
        Assert.True(result.IsSuccess, result.Error.Message);
        var overview = Success(await service.GetPendingApprovalsAsync(2, false));
        Assert.Empty(overview.PendingTasks);
        Assert.Equal(!reject, overview.RecentApprovedTasks.Any(x => x.GorevOnayId == 20));
        await using var db = data.CreateDbContext();
        var record = await db.GorevOnay_Table.SingleAsync(x => x.Id == 20);
        Assert.Equal(reject ? 2 : 1, record.AmirOnayi);
        Assert.Equal("manager", record.Degistiren);
        if (reject) Assert.Equal("Uygun değil", record.OnayRedAciklama);
        Assert.Single(data.Email.Messages);
        Assert.True((await service.ApproveAsync(20, 1)).IsFailure);
    }

    [Fact]
    public async Task AuthorizedIkysUserCanApproveOutsideManagedUnits()
    {
        await using var data = await TestData.CreateAsync();
        Assert.True((await data.Service().ApproveAsync(40, 0, "hr", authorizedIkysView: true)).IsSuccess);
        Assert.DoesNotContain(Success(await data.Service().GetPendingApprovalsAsync(null, true)).PendingTasks,
            x => x.GorevOnayId == 40);
    }

    [Fact]
    public async Task PersonalListIncludesEntireOwnHistoryWhileOrganizationIncludesAllUnitsForOneYear()
    {
        await using var data = await TestData.CreateAsync();
        await SeedListsAsync(data);
        var personal = Success(await data.Service(manage: false).GetTaskApprovalListAsync(2, false));
        Assert.All(personal, x => Assert.Equal(2, x.PersonelId));
        Assert.Contains(personal, x => x.TaskApprovalId == 110);
        Assert.Equal(personal.OrderByDescending(x => x.EndDate).Select(x => x.TaskApprovalId), personal.Select(x => x.TaskApprovalId));
        var general = Success(await data.Service().GetTaskApprovalListAsync(null, true));
        Assert.Contains(general, x => x.PersonelId == 3);
        Assert.Contains(general, x => x.IsRejected);
        Assert.Contains(general, x => x.IsPaid);
        Assert.DoesNotContain(general, x => x.TaskApprovalId == 110);
        Assert.True((await data.Service(manage: false).GetTaskApprovalListAsync(2, true)).IsFailure);
        Assert.True((await data.Service().GetTaskApprovalListAsync(null, false)).IsFailure);
    }

    [Theory]
    [InlineData(101, true)]
    [InlineData(102, false)]
    [InlineData(103, false)]
    [InlineData(104, false)]
    [InlineData(105, false)]
    [InlineData(106, false)]
    [InlineData(107, true)]
    [InlineData(108, false)]
    [InlineData(109, false)]
    [InlineData(110, false)]
    [InlineData(112, true)]
    [InlineData(113, false)]
    public async Task PersonalEditChecksOwnerPaymentApprovalAndFourteenDayEndDate(int id, bool allowed)
    {
        await using var data = await TestData.CreateAsync();
        await SeedListsAsync(data);
        var service = data.Service(manage: false);
        var loaded = await service.GetScopedByIdAsync(id, 2, false);
        Assert.Equal(allowed, loaded.IsSuccess);
        var listItem = Success(await service.GetTaskApprovalListAsync(2, false)).SingleOrDefault(x => x.TaskApprovalId == id);
        if (id == 104) Assert.Null(listItem);
        else Assert.Equal(allowed, listItem!.CanEditPersonal(DateTime.Now));
        await using var db = data.CreateDbContext();
        var update = await db.GorevOnay_Table.AsNoTracking().SingleAsync(x => x.Id == id);
        update.GorevinSebebi = "Düzenlendi";
        update.Odendi = true;
        update.AmirOnayi = (int)ApprovalStatus.Approved;
        var result = await service.UpdateScopedAsync(update, 2, false);
        Assert.Equal(allowed, result.IsSuccess);
        if (allowed)
        {
            var saved = await db.GorevOnay_Table.AsNoTracking().SingleAsync(x => x.Id == id);
            Assert.Equal("Düzenlendi", saved.GorevinSebebi);
            Assert.False(saved.Odendi);
            Assert.Equal((int)ApprovalStatus.PendingApproval, saved.AmirOnayi);
        }
    }

    [Fact]
    public async Task ReportsAndPaymentsExcludePendingRejectedPaidAndUnknownStatuses()
    {
        await using var data = await TestData.CreateAsync();
        await SeedListsAsync(data);
        var service = data.Service();
        Assert.Equal(2, Success(await service.GetTaskApprovalReportItemsAsync([102, 112], null, true)).Count);
        foreach (var id in new[] { 101, 105, 106, 113 })
        {
            Assert.True((await service.GetTaskApprovalReportItemsAsync([102, id], 1, true)).IsFailure);
            Assert.True((await service.SetTaskApprovalsPaidAsync([104, id])).IsFailure);
        }
        await using var db = data.CreateDbContext();
        Assert.False((await db.GorevOnay_Table.AsNoTracking().SingleAsync(x => x.Id == 104)).Odendi);
        Assert.True((await data.Service(manage: false).SetTaskApprovalsPaidAsync([104])).IsFailure);
        Assert.True((await service.GetTaskApprovalReportItemsAsync([102], 2, false)).IsFailure);
        Assert.True((await service.SetTaskApprovalsPaidAsync([102, 104, 104])).IsSuccess);
        Assert.All(await db.GorevOnay_Table.AsNoTracking().Where(x => x.Id == 102 || x.Id == 104).ToListAsync(), x => Assert.True(x.Odendi));
        Assert.True((await service.GetTaskApprovalReportItemsAsync([102], null, true)).IsFailure);
        Assert.True((await service.GetScopedByIdAsync(104, null, true)).IsFailure);
    }

    [Fact]
    public async Task DirectEntryAndDeleteCannotBypassPersonalRestrictions()
    {
        await using var data = await TestData.CreateAsync();
        await SeedListsAsync(data);
        var service = data.Service(manage: false);
        var entry = new TaskApprovalEntryService(data, NullLogService.Instance, service, new Authorization(false, true));
        Assert.True((await entry.LoadAsync(new() { GorevOnayId = 101, CurrentPersonelId = 2 })).IsSuccess);
        Assert.True((await entry.LoadAsync(new() { GorevOnayId = 104, CurrentPersonelId = 2 })).IsFailure);
        Assert.True((await entry.LoadAsync(new() { GorevOnayId = 104, CurrentPersonelId = 2, CanManage = true, Auth = "IKYS" })).IsFailure);
        Assert.True((await service.DeleteScopedAsync(104, 2, false)).IsFailure);
        Assert.True((await service.DeleteScopedAsync(108, 2, false)).IsFailure);
        Assert.True((await service.DeleteScopedAsync(101, 2, false)).IsSuccess);
        Assert.True((await data.Service().GetScopedByIdAsync(104, null, true)).IsSuccess);
    }

    [Fact]
    public async Task PersonalCreationCannotAssignAnotherOwnerOrForgePaidAndApprovedState()
    {
        await using var data = await TestData.CreateAsync();
        var service = data.Service(manage: false);
        Assert.True((await service.AddScopedAsync(new() { PersonelId = 3 }, 2, false)).IsFailure);
        var entity = new GorevOnay
        {
            PersonelId = 2, Odendi = true, Secildi = true, AmirOnayi = 1,
            BaslangicTarihi = DateTime.Now, BitisTarihi = DateTime.Now.AddDays(1)
        };
        Assert.True((await service.AddScopedAsync(entity, 2, false)).IsSuccess);
        await using var db = data.CreateDbContext();
        var saved = await db.GorevOnay_Table.AsNoTracking().SingleAsync(x => x.Id == entity.Id);
        Assert.False(saved.Odendi);
        Assert.False(saved.Secildi);
        Assert.Equal(0, saved.AmirOnayi);
        Assert.True((await service.GetScopedByIdAsync(entity.Id, 2, false)).IsSuccess);
    }

    private static async Task SeedListsAsync(TestData data)
    {
        await using var db = data.CreateDbContext();
        await db.GorevOnay_Table.ExecuteDeleteAsync();
        var now = DateTime.Now;
        var rows = new (int Id, int Owner, int Status, bool? Paid, DateTime End)[]
        {
            (101, 2, 0, false, now.AddDays(-1)), (102, 2, 1, false, now.AddDays(-5)),
            (103, 2, 3, false, now.AddDays(-15)), (104, 3, 1, false, now),
            (105, 2, 3, true, now), (106, 2, 2, false, now),
            (107, 2, 0, false, now.AddDays(-14).AddMinutes(5)),
            (108, 2, 0, false, now.AddDays(-14).AddMinutes(-5)),
            (109, 2, 0, null, now), (110, 2, 0, false, now.AddYears(-2)),
            (111, 3, 0, false, now), (112, 2, 3, false, now.AddDays(1)), (113, 2, 4, false, now)
        };
        db.GorevOnay_Table.AddRange(rows.Select(x => new GorevOnay
        {
            Id = x.Id, PersonelId = x.Owner, AmirOnayi = x.Status, Odendi = x.Paid,
            BaslangicTarihi = x.Id == 101 ? now.AddDays(-30) : x.End.AddDays(-1), BitisTarihi = x.End,
            GorevinSebebi = "Görev", GorevinYeri = "Ankara"
        }));
        await db.SaveChangesAsync();
    }

    private static T Success<T>(Result<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.Error.Message} {result.Error.Exception}");
        return result.Value;
    }

    private sealed class Authorization(bool manage, bool view) : IServiceAuthorizationContext
    {
        public Task<bool> HasPermissionAsync(ModuleName module, Operation operation, CancellationToken cancellationToken = default)
            => Task.FromResult(module == ModuleName.IKYS && (operation == Operation.Manage ? manage : view));
        public Task<string?> GetUserNameAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>("manager");
        public Task<IReadOnlyList<int>> GetAuthorizedRegionIdsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<int>>([]);
    }

    private sealed class Email : IEmailService
    {
        public List<string> Messages { get; } = [];
        public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
            => SendAsync([to], subject, htmlBody, cancellationToken);
        public Task SendAsync(IEnumerable<string> recipients, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            Messages.Add(htmlBody);
            return Task.CompletedTask;
        }
    }

    private sealed class TestData : IDbContextFactory<ApplicationDbContext>, IAsyncDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        public Email Email { get; } = new();
        public ApplicationDbContext CreateDbContext() => new TestContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        public GorevOnayService Service(bool manage = true, bool view = true)
            => new(this, NullLogService.Instance, Email, new Authorization(manage, view));

        public static async Task<TestData> CreateAsync()
        {
            var data = new TestData();
            await data._connection.OpenAsync();
            await using var db = data.CreateDbContext();
            await db.Database.EnsureCreatedAsync();
            db.Personel_Table.AddRange(Enumerable.Range(1, 3).Select(i => new Personel
            {
                Id = i, Adi = "Personel", Soyadi = i.ToString(), SicilNo = i
            }));
            db.BirimTanim_Table.Add(new() { Id = 1, Adi = "Birim", KisaAdi = "B", ParentId = 0, AmirId = 1, Aktif = true, Sira = 1, BolgeId = 1 });
            db.UnvanTanim_Table.Add(new() { Id = 1, Adi = "Uzman", GorTipId = 1 });
            await db.SaveChangesAsync();
            db.IsBilgileri_Table.Add(new() { PersonelId = 2, BirimId = 1, UnvanId = 1, CalismaDurumu = CalismaDurumu.Calisiyor });
            db.IletisimBilgileri_Table.Add(new() { PersonelId = 2, InternetEPosta = "employee@example.test" });
            var date = new DateTime(2026, 11, 15, 9, 30, 0);
            db.GorevOnay_Table.AddRange(
                new GorevOnay { Id = 20, PersonelId = 2, AmirOnayi = 0, BaslangicTarihi = date.AddDays(10), BitisTarihi = date.AddDays(10).AddHours(8), Aciklama = "Açıklama" },
                new GorevOnay { Id = 21, PersonelId = 2, AmirOnayi = 2, BaslangicTarihi = date.AddDays(30), OnayRedAciklama = "Gerekçe" },
                new GorevOnay { Id = 40, PersonelId = 3, AmirOnayi = 0, BaslangicTarihi = date.AddDays(40) });
            db.GorevOnay_Table.AddRange(Enumerable.Range(30, 6).Select(i => new GorevOnay
            {
                Id = i, PersonelId = 2, AmirOnayi = 1, BaslangicTarihi = date.AddDays(i - 30), BitisTarihi = date.AddDays(i)
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
                    typeof(Personel), typeof(IsBilgileri), typeof(BirimTanim), typeof(UnvanTanim),
                    typeof(GorevTanim), typeof(GorevOnay), typeof(IletisimBilgileri), typeof(Harcirah)
                };
                foreach (var type in modelBuilder.Model.GetEntityTypes().Select(x => x.ClrType).Where(x => !keep.Contains(x)).ToList())
                    modelBuilder.Ignore(type);
            }
        }
    }
}
