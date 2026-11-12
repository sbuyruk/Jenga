using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.DataAccess.Services.IKYS;
using Jenga.DataAccess.Services.Search;
using Jenga.Models.Enums;
using Jenga.Models.Search;
using Jenga.Utility.Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Jenga.Overtime.Tests;

public sealed class IkysSearchTests
{
    [Theory]
    [InlineData("user2", false, new int[] { 2 }, new int[] { 2 }, new int[] { 10 })]
    [InlineData("user2", false, new int[] { }, new int[] { }, new int[] { })]
    [InlineData("user1", false, new int[] { 1 }, new int[] { 1, 2, 4 }, new int[] { 10 })]
    [InlineData("user9", true, new int[] { 3, 6, 7, 9 }, new int[] { 1, 2, 3, 4, 5, 6, 7, 9 }, new int[] { 10 })]
    [InlineData("user9", true, new int[] { 3, 6, 7, 9 }, new int[] { 1, 2, 3, 4, 5, 6, 7, 9 }, new int[] { })]
    [InlineData("user6", false, new int[] { 6 }, new int[] { 2, 6 }, new int[] { 10 })]
    [InlineData("unknown", true, new int[] { }, new int[] { }, new int[] { 10 })]
    public async Task IkysSearchRespectsRegionOwnershipManagementAndDelegation(
        string login, bool manage, int[] taskOwners, int[] overtimeOwners, int[] regions)
    {
        var connectionString = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE Personel_Table (Id INTEGER, Adi TEXT, Soyadi TEXT, SicilNo INTEGER, KullaniciAdi TEXT, Tipi INTEGER);
            CREATE TABLE IsBilgileri_Table (
                Id INTEGER, PersonelId INTEGER, BirimId INTEGER, CalismaDurumu INTEGER,
                UnvanId INTEGER, GorevId INTEGER, BaslamaTar TEXT);
            CREATE TABLE UnvanTanim_Table (Id INTEGER, Adi TEXT);
            CREATE TABLE GorevTanim_Table (Id INTEGER, Adi TEXT);
            CREATE TABLE IletisimBilgileri_Table (PersonelId INTEGER, DahiliTelefonu TEXT, IntranetEPosta TEXT);
            CREATE TABLE BirimTanim_Table (
                Id INTEGER, Adi TEXT, KisaAdi TEXT NOT NULL DEFAULT '', ParentId INTEGER, AmirId INTEGER, Sira INTEGER NOT NULL DEFAULT 0,
                Aktif INTEGER, BolgeId INTEGER, Aciklama TEXT, Olusturan TEXT, OlusturmaTarihi TEXT,
                Degistiren TEXT, DegistirmeTarihi TEXT);
            INSERT INTO BirimTanim_Table (Id, Adi, AmirId, ParentId, Aktif, BolgeId) VALUES
                (1, 'Merkez', 1, 0, 1, 10), (2, 'Diger bolge', 9, 0, 1, 20),
                (3, 'Alt birim', 4, 1, 1, 10), (4, 'Baska birim', 9, 0, 1, 10);
            CREATE TABLE GorevOnay_Table (
                Id INTEGER, PersonelId INTEGER, GorevinSebebi TEXT, GorevinYeri TEXT,
                BaslangicTarihi TEXT, BitisTarihi TEXT, AmirOnayi INTEGER);
            CREATE TABLE OvertimeEntry_Table (
                Id INTEGER, PersonelId INTEGER, UnitId INTEGER, FullName TEXT, WorkDate TEXT,
                Reason TEXT, UnitName TEXT, SicilNo INTEGER);
            CREATE TABLE OvertimeDelegation_Table (DelegatePersonelId INTEGER, TargetPersonelId INTEGER, IsActive INTEGER);
            INSERT INTO OvertimeDelegation_Table VALUES (6, 2, 1), (6, 7, 0);
            """;
        await command.ExecuteNonQueryAsync();
        foreach (var (id, unit) in new[] { (1, 1), (2, 1), (3, 2), (4, 3), (5, 3), (6, 4), (7, 4), (9, 4) })
        {
            command.CommandText = """
                INSERT INTO Personel_Table (Id, Adi, Soyadi, SicilNo, KullaniciAdi) VALUES ($id, 'ortak', $surname, $sicil, $login);
                INSERT INTO IsBilgileri_Table (Id, PersonelId, BirimId, CalismaDurumu) VALUES ($id, $id, $unit, 1);
                INSERT INTO IletisimBilgileri_Table VALUES ($id, '123', 'personel@example.test');
                INSERT INTO GorevOnay_Table VALUES ($id, $id, 'ortak gorev', 'Ankara', $date, $date, 1);
                INSERT INTO OvertimeEntry_Table VALUES ($id, $id, $unit, $fullName, $date, 'ortak mesai', 'Birim', $sicil);
                """;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$unit", unit);
            command.Parameters.AddWithValue("$surname", id.ToString());
            command.Parameters.AddWithValue("$sicil", 1000 + id);
            command.Parameters.AddWithValue("$login", $"user{id}");
            command.Parameters.AddWithValue("$fullName", $"ortak {id}");
            command.Parameters.AddWithValue("$date", DateTime.Today);
            await command.ExecuteNonQueryAsync();
        }

        var service = new GlobalSearchService(new SearchFactory(connectionString), new NullLogService(),
            new SearchAuthorization(login, manage, regions));
        var result = await service.SearchAsync("ortak");

        Assert.True(result.IsSuccess, $"{result.Error} {result.Error.Exception}");
        var items = result.Value.Gruplar.SelectMany(g => g.Sonuclar).ToList();
        Assert.All(items, item => Assert.Equal(SearchModul.IKYS, item.Modul));
        Assert.Equal(taskOwners, items.Where(i => i.Tipi == SearchResultTipi.GorevOnay).Select(i => i.Id).OrderBy(id => id));
        Assert.Equal(overtimeOwners, items.Where(i => i.Tipi == SearchResultTipi.FazlaMesai).Select(i => i.Id).OrderBy(id => id));
        Assert.Equal(login == "unknown" || !manage && regions.Length == 0 ? 0 : manage ? 8 : 7,
            items.Count(i => i.Tipi == SearchResultTipi.Personel));
        if (!manage) Assert.DoesNotContain(items, i => i.Id == 3);
        Assert.All(items.Where(i => i.Tipi == SearchResultTipi.FazlaMesai), i =>
            Assert.StartsWith($"/ikys/overtime?date={DateTime.Today:yyyy-MM-dd}&scope=", i.HedefUrl));
        Assert.All(items.Where(i => i.Tipi == SearchResultTipi.GorevOnay), i =>
            Assert.StartsWith($"/ikys/task-approvals?SecilenId={i.Id}", i.HedefUrl));
        Assert.All(items.Where(i => i.Tipi == SearchResultTipi.Personel), i =>
            Assert.Equal($"/ikys/personel/{i.Id}", i.HedefUrl));

        var bySicil = await service.SearchAsync("1002");
        Assert.True(bySicil.IsSuccess, $"{bySicil.Error} {bySicil.Error.Exception}");
        Assert.All(bySicil.Value.Gruplar.SelectMany(g => g.Sonuclar), i => Assert.Equal(2, i.Id));
        if (login != "unknown" && (manage || regions.Length > 0))
            Assert.Contains(bySicil.Value.Gruplar, g => g.Sonuclar.Count > 0);

        var byLocation = await service.SearchAsync("Ankara");
        Assert.True(byLocation.IsSuccess, $"{byLocation.Error} {byLocation.Error.Exception}");
        Assert.Equal(taskOwners, byLocation.Value.Gruplar.SelectMany(g => g.Sonuclar).Select(i => i.Id).OrderBy(id => id));
        Assert.All(byLocation.Value.Gruplar.SelectMany(g => g.Sonuclar), i => Assert.Equal(SearchResultTipi.GorevOnay, i.Tipi));

        var byReason = await service.SearchAsync("mesai");
        Assert.True(byReason.IsSuccess, $"{byReason.Error} {byReason.Error.Exception}");
        Assert.Equal(overtimeOwners, byReason.Value.Gruplar.SelectMany(g => g.Sonuclar).Select(i => i.Id).OrderBy(id => id));
        Assert.All(byReason.Value.Gruplar.SelectMany(g => g.Sonuclar), i => Assert.Equal(SearchResultTipi.FazlaMesai, i.Tipi));

        var summaryService = new PersonelSummaryService(new SearchFactory(connectionString),
            new SearchAuthorization(login, manage, regions), new NullLogService());
        var inRegion = await summaryService.GetAsync(2);
        var hasAccess = login != "unknown" && (manage || regions.Contains(10));
        Assert.Equal(hasAccess, inRegion.IsSuccess);
        if (hasAccess)
        {
            Assert.Equal(2, inRegion.Value.PersonelId);
            Assert.Equal("ortak 2", inRegion.Value.FullName);
            Assert.Equal("personel@example.test", inRegion.Value.WorkEmail);
            Assert.Equal(taskOwners.Contains(2) ? 1 : 0, inRegion.Value.TaskApprovals.Count);
            Assert.Equal(overtimeOwners.Contains(2) ? 1 : 0, inRegion.Value.OvertimeEntries.Count);
            Assert.All(inRegion.Value.TaskApprovals, activity => Assert.Equal(2, activity.Id));
            Assert.All(inRegion.Value.OvertimeEntries, activity => Assert.Equal(2, activity.Id));
        }
        var outsideRegion = await summaryService.GetAsync(3);
        Assert.Equal(login != "unknown" && manage, outsideRegion.IsSuccess);
        var missing = await summaryService.GetAsync(99999);
        Assert.True(missing.IsFailure);
    }

    [Fact]
    public async Task PersonelSummaryRequiresIkysViewBeforeQueryingDatabase()
    {
        var service = new PersonelSummaryService(new SearchFactory("Data Source=:memory:"),
            new SearchAuthorization("user2", false, [10], false), new NullLogService());
        var result = await service.GetAsync(2);
        Assert.True(result.IsFailure);
        Assert.Equal("General.Forbidden", result.Error.Code);
    }

    private sealed class SearchFactory(string connectionString) : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connectionString).Options);
    }

    private sealed class SearchAuthorization(string login, bool manage, int[] regions, bool view = true) : IServiceAuthorizationContext
    {
        public Task<bool> HasPermissionAsync(ModuleName module, Operation operation, CancellationToken cancellationToken = default)
            => Task.FromResult(module == ModuleName.IKYS && (view && operation == Operation.View || manage && operation == Operation.Manage));

        public Task<string?> GetUserNameAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<string?>($"DOMAIN\\{login}");

        public Task<IReadOnlyList<int>> GetAuthorizedRegionIdsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<int>>(regions);
    }
}
