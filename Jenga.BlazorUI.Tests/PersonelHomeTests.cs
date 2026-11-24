using System.Net;
using System.Reflection;
using Jenga.BlazorUI.Components.IKYS;
using Jenga.BlazorUI.Services.Common;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.IKYS;
using Jenga.Models.Inventory;
using Jenga.Utility.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class PersonelHomeTests
{
    [Fact]
    public async Task SelectedPersonUsesHomeLayoutWithoutChangingIdentityOrReadingRestrictedSections()
    {
        var requestedId = 0;
        var factory = new UnusedDatabaseFactory();
        var summary = new PersonelSummary
        {
            PersonelId = 2, FullName = "Seçilen Personel", SicilNo = 186, UnitName = "Test Birimi", Title = "Uzman",
            WorkEmail = "selected@example.test",
            TaskApprovals = [new(5, "Görev Onayı", DateTime.Today, "<p><strong>Görev açıklaması</strong></p>",
                "/ikys/task-approvals?SecilenId=5", 0, DateTime.Today, "Ankara")]
        };
        var summaryService = Fake<IPersonelSummaryService>((method, args) =>
        {
            Assert.Equal("GetAsync", method);
            requestedId = (int)args![0]!;
            return Task.FromResult(Result.Success(summary));
        });
        var currentUser = Fake<ICurrentUserService>((method, _) => method switch
        {
            "GetCurrentPersonelAsync" => Task.FromResult<Personel?>(new() { Id = 1, Adi = "Oturum", Soyadi = "Kullanıcısı" }),
            "HasPermissionAsync" => Task.FromResult(false),
            _ => throw new InvalidOperationException(method)
        });

        var html = await RenderAsync(factory, currentUser, summaryService, 2);
        Assert.Equal(2, requestedId);
        Assert.Contains("workspace-hero", html);
        Assert.Contains("Seçilen Personel", html);
        Assert.Contains("Test Birimi", html);
        Assert.Contains("Görev açıklaması", html);
        Assert.Contains("Onay Bekliyor", html);
        Assert.DoesNotContain("<strong>Görev açıklaması</strong>", html);
        Assert.DoesNotContain("Oturum Kullanıcısı", html);
        Assert.Contains("Bu bilgileri görüntüleme yetkiniz bulunmuyor.", html);
        Assert.DoesNotContain("yüklenemedi", html);
        Assert.Equal(1, factory.CreatedContexts);
    }

    [Fact]
    public async Task DeniedProfileNeverReadsDashboardData()
    {
        var factory = new UnusedDatabaseFactory();
        var current = Fake<ICurrentUserService>((method, _) => method == "GetCurrentPersonelAsync"
            ? Task.FromResult<Personel?>(new() { Id = 1, Adi = "Oturum", Soyadi = "Kullanıcısı" })
            : throw new InvalidOperationException(method));
        var denied = Fake<IPersonelSummaryService>((_, _) => Task.FromResult(
            Result.Failure<PersonelSummary>(Error.Forbidden("Bu personele erişiminiz yok."))));
        var html = await RenderAsync(factory, current, denied, 2);
        Assert.Contains("Bu personele erişiminiz yok.", html);
        Assert.DoesNotContain("workspace-hero", html);
        Assert.Equal(0, factory.CreatedContexts);
    }

    [Fact]
    public async Task DefaultHomeDoesNotRequireAnotherPersonnelProfile()
    {
        var factory = new UnusedDatabaseFactory();
        var current = Fake<ICurrentUserService>((method, _) => method == "GetCurrentPersonelAsync"
            ? Task.FromResult<Personel?>(null) : throw new InvalidOperationException(method));
        var summary = Fake<IPersonelSummaryService>((method, _) => throw new InvalidOperationException(method));
        var html = await RenderAsync(factory, current, summary, null);
        Assert.Contains("Merhaba, Kullanıcı", html);
        Assert.Contains("İzin Taleplerim", html);
        Assert.Equal(0, factory.CreatedContexts);
    }

    [Fact]
    public async Task SignedInHomeReadsLeaveRequestsOnTheSchemaBeforeFutureOffset()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var factory = new LegacyLeaveDatabaseFactory(connection);
        var today = DateTime.Today;
        var person = new Personel { Id = 1, Adi = "Oturum", Soyadi = "Kullanıcısı" };
        await using (var db = factory.CreateDbContext())
        {
            await db.Database.EnsureCreatedAsync();
            db.Personel_Table.Add(person);
            db.IzinTanim_Table.Add(new IzinTanim { Id = (int)LeaveType.Paid, Adi = "Yıllık Ücretli İzin" });
            db.IzinTalep_Table.Add(new IzinTalep
            {
                PersonelId = person.Id, IzinTipi = (int)LeaveType.Paid, Aktif = true,
                BaslangicTarihi = today, BitisTarihi = today, OnayDurumu = IzinTalep.OnayDurumuTipi.IslemBekliyor
            });
            db.IzinDonem_Table.AddRange(
                new IzinDonem { PersonelId = person.Id, BaslangicTarihi = today, IzinHakki = "20", KalanIzin = "12" },
                new IzinDonem { PersonelId = person.Id, BaslangicTarihi = today.AddYears(1), IzinHakki = "26", KalanIzin = "25" });
            await db.SaveChangesAsync();
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE IzinTalep_Table DROP COLUMN GelecekDonemMahsubu");
        }
        var current = Fake<ICurrentUserService>((method, _) => method == "GetCurrentPersonelAsync"
            ? Task.FromResult<Personel?>(person) : throw new InvalidOperationException(method));
        var summary = Fake<IPersonelSummaryService>((method, _) => throw new InvalidOperationException(method));
        var approvals = Fake<IGorevOnayService>((method, _) => method == "GetTaskApprovalListAsync"
            ? Task.FromResult(Result.Success(new List<TaskApprovalListItem>())) : throw new InvalidOperationException(method));

        var html = await RenderAsync(factory, current, summary, null, approvals);

        Assert.Contains("Merhaba, Oturum Kullanıcısı", html);
        Assert.Contains("Yıllık Ücretli İzin", html);
        Assert.Contains("İşlem Bekliyor", html);
        Assert.Contains("12 gün", html);
        Assert.DoesNotContain("25 gün", html);
        Assert.DoesNotContain("yüklenemedi", html);
    }

    private static async Task<string> RenderAsync(IDbContextFactory<ApplicationDbContext> factory, ICurrentUserService current,
        IPersonelSummaryService summary, int? personelId, IGorevOnayService? approvals = null)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(factory);
        services.AddSingleton(current);
        services.AddSingleton(summary);
        services.AddSingleton(approvals ?? Fake<IGorevOnayService>((method, _) => throw new InvalidOperationException(method)));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?> { ["PersonelId"] = personelId });
            var root = await renderer.RenderComponentAsync<PersonelHome>(parameters);
            return WebUtility.HtmlDecode(root.ToHtmlString());
        });
    }

    private sealed class LegacyLeaveDatabaseFactory(SqliteConnection connection) : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() =>
            new LegacyLeaveContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
    }

    private sealed class LegacyLeaveContext(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var dashboardTypes = new HashSet<Type>
            {
                typeof(Personel), typeof(IsBilgileri), typeof(BirimTanim), typeof(UnvanTanim), typeof(DereceKademeDegisim),
                typeof(IletisimBilgileri), typeof(IzinTalep), typeof(IzinTanim), typeof(IzinDonem),
                typeof(MaterialAsset), typeof(MaterialAssetLog), typeof(Material), typeof(Location)
            };
            foreach (var type in modelBuilder.Model.GetEntityTypes().Select(x => x.ClrType).Where(x => !dashboardTypes.Contains(x)).ToList())
                modelBuilder.Ignore(type);
        }
    }

    private sealed class UnusedDatabaseFactory : IDbContextFactory<ApplicationDbContext>
    {
        public int CreatedContexts { get; private set; }
        public ApplicationDbContext CreateDbContext()
        {
            CreatedContexts++;
            return new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=invalid;Database=Unused;Integrated Security=True;Connect Timeout=1").Options);
        }
    }

    private static T Fake<T>(Func<string, object?[]?, object?> invoke) where T : class
    {
        var instance = DispatchProxy.Create<T, ServiceStub>();
        ((ServiceStub)(object)instance).InvokeMethod = invoke;
        return instance;
    }

    public class ServiceStub : DispatchProxy
    {
        public Func<string, object?[]?, object?> InvokeMethod { get; set; } = null!;
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => InvokeMethod(targetMethod!.Name, args);
    }
}
