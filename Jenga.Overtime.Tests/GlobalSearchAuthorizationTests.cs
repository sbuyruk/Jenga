using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.DataAccess.Services.Search;
using Jenga.Models.Enums;
using Jenga.Models.Search;
using Jenga.Utility.Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Jenga.Overtime.Tests;

public sealed class GlobalSearchAuthorizationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task SearchQueriesOnlyAuthorizedModules(bool tbys, bool nbys)
    {
        var connectionString = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE Il_Table (Id INTEGER, BolgeId INTEGER); INSERT INTO Il_Table VALUES (1, 10);";
        await command.ExecuteNonQueryAsync();

        // Unauthorized modules have no tables, so querying them also fails the test.
        if (tbys)
        {
            command.CommandText = """
                CREATE TABLE Tasinmaz_Table (
                    Id INTEGER, EnvanterdeMi INTEGER, Ili TEXT, Ilcesi TEXT, Adres TEXT,
                    Cinsi TEXT, EmlakSicilNo TEXT, Nitelik TEXT, IlId INTEGER);
                INSERT INTO Tasinmaz_Table VALUES (1, 1, 'ortak', '', '', 'Arsa', '1', '', 1);
                CREATE TABLE Kiraci_Table (
                    Id INTEGER, Adi TEXT, Soyadi TEXT, Ili TEXT, Ilcesi TEXT, KiralamaAmaci TEXT, IlId INTEGER);
                INSERT INTO Kiraci_Table VALUES (1, 'ortak', '', '', '', '', 1);
                CREATE TABLE TasinmazBagisci_Table (
                    Id INTEGER, Gizli INTEGER, Adi TEXT, Soyadi TEXT, Ili TEXT, Ilcesi TEXT, Meslegi TEXT, IlId INTEGER);
                INSERT INTO TasinmazBagisci_Table VALUES (1, 0, 'ortak', '', '', '', '', 1);
                """;
            await command.ExecuteNonQueryAsync();
        }

        if (nbys)
        {
            command.CommandText = """
                CREATE TABLE NakitBagisci_Table (Id INTEGER, Adi TEXT, Soyadi TEXT, Ili INTEGER);
                INSERT INTO NakitBagisci_Table VALUES (1, 'ortak', '', 1);
                CREATE TABLE FTKKisi_Table (Id INTEGER, Adi TEXT, Soyadi TEXT, Ili INTEGER);
                INSERT INTO FTKKisi_Table VALUES (1, 'ortak', '', 1);
                """;
            await command.ExecuteNonQueryAsync();
        }

        var factory = new SearchDbFactory(connectionString);
        var service = new GlobalSearchService(factory, new NullLogService(), new TestAuthorization(tbys, nbys));
        var result = await service.SearchAsync("ortak");

        Assert.Equal((tbys ? 1 : 0) + (nbys ? 2 : 0), factory.CreatedContexts);
        if (!tbys && !nbys)
        {
            Assert.True(result.IsFailure);
            Assert.Equal("Search.View.Forbidden", result.Error.Code);
            return;
        }

        Assert.True(result.IsSuccess, $"{result.Error} {result.Error.Exception}");
        var expectedModules = new List<SearchModul>();
        if (tbys) expectedModules.AddRange([SearchModul.TBYS, SearchModul.TBYS, SearchModul.TBYS]);
        if (nbys) expectedModules.AddRange([SearchModul.NBYS, SearchModul.FTK]);
        Assert.Equal(expectedModules, result.Value.Gruplar.Select(g => g.Modul));
        Assert.All(result.Value.Gruplar, group => Assert.Single(group.Sonuclar));
    }

    private sealed class SearchDbFactory(string connectionString) : IDbContextFactory<ApplicationDbContext>
    {
        public int CreatedContexts { get; private set; }

        public ApplicationDbContext CreateDbContext()
        {
            CreatedContexts++;
            return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connectionString).Options);
        }
    }

    [Theory]
    [InlineData(new int[] { 10 }, new int[] { 1 }, false)]
    [InlineData(new int[] { 20 }, new int[] { 2 }, false)]
    [InlineData(new int[] { 10, 20 }, new int[] { 1, 2 }, false)]
    [InlineData(new int[] { 999 }, new int[] { }, false)]
    [InlineData(new int[] { }, new int[] { }, false)]
    [InlineData(new int[] { 10 }, new int[] { 1 }, true)]
    [InlineData(new int[] { }, new int[] { }, true)]
    public async Task SearchFiltersOtherModulesByRegion(int[] regions, int[] expectedIds, bool ikysManage)
    {
        var connectionString = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE Il_Table (Id INTEGER, BolgeId INTEGER);
            CREATE TABLE Personel_Table (Id INTEGER, KullaniciAdi TEXT);
            INSERT INTO Il_Table VALUES (1, 10), (2, 20), (3, NULL);
            CREATE TABLE Tasinmaz_Table (
                Id INTEGER, EnvanterdeMi INTEGER, Ili TEXT, Ilcesi TEXT, Adres TEXT,
                Cinsi TEXT, EmlakSicilNo TEXT, Nitelik TEXT, IlId INTEGER);
            CREATE TABLE Kiraci_Table (
                Id INTEGER, Adi TEXT, Soyadi TEXT, Ili TEXT, Ilcesi TEXT, KiralamaAmaci TEXT, IlId INTEGER);
            CREATE TABLE TasinmazBagisci_Table (
                Id INTEGER, Gizli INTEGER, Adi TEXT, Soyadi TEXT, Ili TEXT, Ilcesi TEXT, Meslegi TEXT, IlId INTEGER);
            CREATE TABLE NakitBagisci_Table (Id INTEGER, Adi TEXT, Soyadi TEXT, Ili INTEGER);
            CREATE TABLE FTKKisi_Table (Id INTEGER, Adi TEXT, Soyadi TEXT, Ili INTEGER);
            CREATE TEMP TABLE SearchSeed (Id INTEGER, ProvinceId INTEGER);
            INSERT INTO SearchSeed VALUES (1, 1), (2, 2), (3, NULL), (4, 999), (5, 3);
            INSERT INTO Tasinmaz_Table (Id, EnvanterdeMi, Ili, IlId)
                SELECT Id, 1, 'ortak', ProvinceId FROM SearchSeed;
            INSERT INTO Kiraci_Table (Id, Adi, IlId)
                SELECT Id, 'ortak', ProvinceId FROM SearchSeed;
            INSERT INTO TasinmazBagisci_Table (Id, Gizli, Adi, IlId)
                SELECT Id, 0, 'ortak', ProvinceId FROM SearchSeed;
            INSERT INTO TasinmazBagisci_Table (Id, Gizli, Adi, IlId) VALUES (6, 1, 'ortak', 1);
            INSERT INTO NakitBagisci_Table (Id, Adi, Ili)
                SELECT Id, 'ortak', ProvinceId FROM SearchSeed;
            INSERT INTO FTKKisi_Table (Id, Adi, Ili)
                SELECT Id, 'ortak', ProvinceId FROM SearchSeed;
            """;
        await command.ExecuteNonQueryAsync();

        var factory = new SearchDbFactory(connectionString);
        var service = new GlobalSearchService(factory, new NullLogService(), new TestAuthorization(true, true, regions, ikysManage));
        var result = await service.SearchAsync("ortak");

        Assert.True(result.IsSuccess, $"{result.Error} {result.Error.Exception}");
        Assert.Equal((regions.Length == 0 ? 0 : 3) + (ikysManage ? 1 : 0), factory.CreatedContexts);
        Assert.Equal(expectedIds.Length * 5, result.Value.ToplamSonuc);
        Assert.Equal(expectedIds.Length == 0 ? 0 : 5, result.Value.Gruplar.Count);
        Assert.All(result.Value.Gruplar, group =>
            Assert.Equal(expectedIds, group.Sonuclar.Select(item => item.Id).OrderBy(id => id)));
    }

    private sealed class TestAuthorization(bool tbys, bool nbys, int[]? regions = null, bool ikysManage = false) : IServiceAuthorizationContext
    {
        public Task<bool> HasPermissionAsync(ModuleName module, Operation operation, CancellationToken cancellationToken = default)
            => Task.FromResult(operation == Operation.View &&
                (module == ModuleName.TBYS && tbys || module == ModuleName.NBYS && nbys) ||
                module == ModuleName.IKYS && ikysManage && operation is Operation.View or Operation.Manage);

        public Task<string?> GetUserNameAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<string?>("search-test");

        public Task<IReadOnlyList<int>> GetAuthorizedRegionIdsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<int>>(regions ?? [10]);
    }
}
