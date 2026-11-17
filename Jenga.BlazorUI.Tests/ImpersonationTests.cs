using System.Security.Claims;
using Jenga.BlazorUI.Services.Common;
using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class ImpersonationTests
{
    [Theory]
    [InlineData(false, "2099-01-01T00:00:00Z", false)]
    [InlineData(true, null, false)]
    [InlineData(true, "invalid", false)]
    [InlineData(true, "2026-11-17T12:00:00Z", false)]
    [InlineData(true, "2026-11-17T11:59:59Z", false)]
    [InlineData(true, "2026-11-17T12:00:01Z", true)]
    public void ProductionRequiresEnabledAndFutureExpiry(bool enabled, string? expiry, bool expected)
    {
        var policy = new ImpersonationPolicy(Config(enabled, expiry), new EnvironmentStub());
        Assert.Equal(expected, policy.IsEnabledAt(DateTimeOffset.Parse("2026-11-17T12:00:00Z")));
    }

    [Fact]
    public void DevelopmentDoesNotRequireProductionOptIn()
        => Assert.True(new ImpersonationPolicy(Config(false, null), new EnvironmentStub { EnvironmentName = Environments.Development }).IsEnabled);

    [Fact]
    public async Task RealAdministratorCanSwitchToNonAdminAndReset()
    {
        await using var db = await Database.CreateAsync();
        var js = new StorageStub();
        var config = Config(true, "2099-01-01T00:00:00Z");
        using var service = Service(db, js, config, "DOMAIN\\admin");
        Assert.True(await service.SetImpersonationOverrideAsync("target"));
        Assert.Equal("target", (await service.GetCurrentPersonelAsync())?.KullaniciAdi);
        Assert.True(await service.CanImpersonateAsync()); // Gerçek hesap hâlâ yönetici.
        Assert.True(await service.SetImpersonationOverrideAsync(null));
        Assert.Null(js.Value);
        Assert.Equal("admin", (await service.GetCurrentPersonelAsync())?.KullaniciAdi);
    }

    [Theory]
    [InlineData("reader")]
    [InlineData("target")]
    [InlineData(null)]
    public async Task NonAdministratorCannotSetOrReadForgedOverride(string? realUser)
    {
        await using var db = await Database.CreateAsync();
        var js = new StorageStub { Value = "admin" };
        using var service = Service(db, js, Config(true, "2099-01-01T00:00:00Z"), realUser);
        Assert.False(await service.CanImpersonateAsync());
        Assert.False(await service.SetImpersonationOverrideAsync("admin"));
        Assert.Equal(realUser, (await service.GetCurrentPersonelAsync())?.KullaniciAdi);
    }

    [Fact]
    public async Task ExpiryOrDisableDiscardsCachedIdentityAndPermissions()
    {
        await using var db = await Database.CreateAsync();
        var js = new StorageStub();
        var config = Config(true, "2099-01-01T00:00:00Z");
        using var service = Service(db, js, config, "admin");
        Assert.True(await service.SetImpersonationOverrideAsync("target"));
        Assert.False(await service.HasPermissionAsync(ModuleName.Admin, Operation.Manage));
        config["Auth:Impersonation:ExpiresAtUtc"] = "2000-01-01T00:00:00Z";
        Assert.True(await service.HasPermissionAsync(ModuleName.Admin, Operation.Manage));
        Assert.Equal("admin", (await service.GetCurrentPersonelAsync())?.KullaniciAdi);
        Assert.True(await service.SetImpersonationOverrideAsync(null));
        Assert.Null(js.Value);
    }

    [Fact]
    public async Task ReloadAppliesAuthorizedOverrideAndRejectsUnknownOrWildcardTarget()
    {
        await using var db = await Database.CreateAsync();
        var js = new StorageStub { Value = "target" };
        var config = Config(true, "2099-01-01T00:00:00Z");
        using var service = Service(db, js, config, "admin@domain");
        Assert.Equal("target", (await service.GetCurrentPersonelAsync())?.KullaniciAdi);
        Assert.False(await service.SetImpersonationOverrideAsync("%"));
        Assert.False(await service.SetImpersonationOverrideAsync("missing"));
        Assert.Equal("target", js.Value);
        config["Auth:Impersonation:Enabled"] = "false";
        Assert.Equal("admin", (await service.GetCurrentPersonelAsync())?.KullaniciAdi);
    }

    private static IConfigurationRoot Config(bool enabled, string? expiry) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> {
            ["Auth:Impersonation:Enabled"] = enabled.ToString(), ["Auth:Impersonation:ExpiresAtUtc"] = expiry }).Build();

    private static CurrentUserService Service(Database db, StorageStub js, IConfiguration config, string? user)
    {
        var principal = new ClaimsPrincipal(user is null ? new ClaimsIdentity() :
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user) }, "Test"));
        return new(db, new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = principal } },
            new AuthStub(principal), new ImpersonationService(js), new ImpersonationPolicy(config, new EnvironmentStub()),
            NullLogger<CurrentUserService>.Instance);
    }

    private sealed class AuthStub(ClaimsPrincipal principal) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(principal));
    }
    private sealed class EnvironmentStub : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
    private sealed class StorageStub : IJSRuntime
    {
        public string? Value { get; set; }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            if (identifier == "sessionStorage.setItem") Value = (string?)args![1];
            if (identifier == "sessionStorage.removeItem") Value = null;
            return ValueTask.FromResult(identifier == "sessionStorage.getItem" ? (TValue)(object?)Value! : default!);
        }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken token, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }
    private sealed class Database(SqliteConnection connection) : IDbContextFactory<ApplicationDbContext>, IAsyncDisposable
    {
        public ApplicationDbContext CreateDbContext() => new TestContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        public static async Task<Database> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var factory = new Database(connection);
            await using var db = factory.CreateDbContext();
            await db.Database.EnsureCreatedAsync();
            db.Personel_Table.AddRange(new Personel { Id = 1, KullaniciAdi = "admin", Adi = "Admin", Soyadi = "Test" },
                new Personel { Id = 2, KullaniciAdi = "target", Adi = "Target", Soyadi = "Test" },
                new Personel { Id = 3, KullaniciAdi = "reader", Adi = "Reader", Soyadi = "Test" });
            db.PersonelRol_Table.AddRange(new PersonelRole { PersonelId = 1, RoleId = 1 }, new PersonelRole { PersonelId = 3, RoleId = 2 });
            db.ModulePermission_Table.AddRange(new ModulePermission { Id = 1, Module = ModuleName.Admin, Operation = Operation.Manage },
                new ModulePermission { Id = 2, Module = ModuleName.Admin, Operation = Operation.View });
            db.RoleModulePermission_Table.AddRange(new RoleModulePermission { RoleId = 1, ModulePermissionId = 1 },
                new RoleModulePermission { RoleId = 2, ModulePermissionId = 2 });
            await db.SaveChangesAsync();
            return factory;
        }
        public ValueTask DisposeAsync() => connection.DisposeAsync();
    }
    private sealed class TestContext(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var types = new[] { typeof(Personel), typeof(PersonelRole), typeof(ModulePermission), typeof(RoleModulePermission) };
            foreach (var type in builder.Model.GetEntityTypes().Select(x => x.ClrType).Where(t => !types.Contains(t)).ToArray()) builder.Ignore(type);
            builder.Entity<Personel>().Ignore(p => p.Kimlik).Ignore(p => p.IletisimBilgileri).Ignore(p => p.IsBilgileri);
            builder.Entity<PersonelRole>().Ignore(p => p.Role).Ignore(p => p.Personel);
            builder.Entity<RoleModulePermission>().Ignore(p => p.Role).Ignore(p => p.ModulePermission);
            builder.Entity<ModulePermission>().Ignore(p => p.RoleModulePermissions);
        }
    }
}
