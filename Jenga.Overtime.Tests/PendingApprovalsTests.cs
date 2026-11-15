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

    private static T Success<T>(Result<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.Error.Message} {result.Error.Exception}");
        return result.Value;
    }

    private sealed class Authorization(bool manage, bool view) : IServiceAuthorizationContext
    {
        public Task<bool> HasPermissionAsync(ModuleName module, Operation operation, CancellationToken cancellationToken = default)
            => Task.FromResult(module == ModuleName.IKYS && (operation == Operation.View ? view : manage));
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
                    typeof(GorevTanim), typeof(GorevOnay), typeof(IletisimBilgileri)
                };
                foreach (var type in modelBuilder.Model.GetEntityTypes().Select(x => x.ClrType).Where(x => !keep.Contains(x)).ToList())
                    modelBuilder.Ignore(type);
            }
        }
    }
}
