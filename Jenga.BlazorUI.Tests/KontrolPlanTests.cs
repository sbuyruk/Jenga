using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.TBYS;
using Jenga.Models.Common;
using Jenga.Models.TBYS;
using Jenga.Models.TBYS.Kontrol;
using Jenga.Utility.Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public class KontrolPlanTests
{
    [Theory]
    [InlineData("1Yil", 1)]
    [InlineData("3Yil", 3)]
    [InlineData("5Yil", 5)]
    [InlineData("10Yil", 10)]
    public async Task Tamamla_RapordakiSikliktanPlaniVeTarihiOlusturur(string kod, int yil)
    {
        await using var factory = await Database.AcAsync();
        var servis = Servis(factory);
        var istek = Istek(kod);
        var sonuc = await servis.TamamlaAsync(istek);
        Assert.True(sonuc.IsSuccess, sonuc.IsFailure ? sonuc.Error.Message : "");
        await using var db = factory.CreateDbContext();
        var plan = await db.TasinmazKontrolPlani_Table.SingleAsync();
        var kontrol = await db.TasinmazKontrol_Table.SingleAsync();
        Assert.Equal(yil, plan.PeriyotSure);
        Assert.Equal(PeriyotBirimi.Yil, plan.PeriyotBirim);
        Assert.Equal(istek.KontrolTarihi.AddYears(yil), plan.SonrakiKontrolTarihi);
        Assert.Equal(plan.SonrakiKontrolTarihi, kontrol.HesaplananSonrakiTarih);
        Assert.Equal(kontrol.Id, plan.KaynakKontrolId);
        Assert.True(kontrol.PlanGuncellendi);
    }

    [Fact]
    public async Task Tamamla_SiklikDegisimindeGerekceIster_EskiPlaniKorur()
    {
        await using var factory = await Database.AcAsync();
        await using (var db = factory.CreateDbContext())
        {
            db.TasinmazKontrolPlani_Table.Add(new() { TasinmazId = 1, PeriyotSure = 1, PeriyotBirim = PeriyotBirimi.Yil });
            await db.SaveChangesAsync();
        }
        var servis = Servis(factory);
        var istek = Istek("3Yil");
        var sonuc = await servis.TamamlaAsync(istek);
        Assert.True(sonuc.IsFailure);
        Assert.Contains("gerekçe", sonuc.Error.Message);
        await using (var db = factory.CreateDbContext())
        {
            Assert.Equal(1, (await db.TasinmazKontrolPlani_Table.SingleAsync()).PeriyotSure);
            Assert.Equal(KontrolDurumu.Taslak, (await db.TasinmazKontrol_Table.SingleAsync()).Durum);
        }
        istek.PeriyotDegisiklikGerekcesi = "Taşınmazın mevcut durumu uygundur.";
        Assert.True((await servis.TamamlaAsync(istek)).IsSuccess);
    }

    private static TasinmazKontrolService Servis(Database factory) => new(factory, new DbContextScopeFactory(factory),
        new DosyaSistemiKontrolEkDeposu(new ConfigurationBuilder().Build()), new NullLogService());

    private static KontrolTamamlaIstegi Istek(string kod)
    {
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy("siklik_oneri", kod);
        cevaplar.Koy("kanaat_ozet", "Taşınmaz iyi durumdadır.");
        cevaplar.Koy("kanaat_oneri", "Mevcut kullanım sürdürülebilir.");
        return new() { KontrolId = 1, KontrolTarihi = new DateTime(2020, 11, 17), ImzalayanMakam = "Test", Cevaplar = cevaplar };
    }

    private sealed class Database(string cs, SqliteConnection anchor) : IDbContextFactory<ApplicationDbContext>, IAsyncDisposable
    {
        public ApplicationDbContext CreateDbContext() => new Context(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(cs).Options);
        public static async Task<Database> AcAsync()
        {
            var cs = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared";
            var anchor = new SqliteConnection(cs);
            await anchor.OpenAsync();
            var factory = new Database(cs, anchor);
            await using var db = factory.CreateDbContext();
            await db.Database.EnsureCreatedAsync();
            db.Tasinmaz_Table.Add(new() { Id = 1 });
            db.TasinmazKontrol_Table.Add(new() { Id = 1, TasinmazId = 1 });
            await db.SaveChangesAsync();
            return factory;
        }
        public ValueTask DisposeAsync() => anchor.DisposeAsync();
    }

    private sealed class Context(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var types = new[] { typeof(Tasinmaz), typeof(TasinmazKontrol), typeof(TasinmazKontrolPlani), typeof(TasinmazKontrolEk), typeof(Il), typeof(Bolge) };
            foreach (var type in builder.Model.GetEntityTypes().Select(x => x.ClrType).Where(t => !types.Contains(t)).ToArray())
                builder.Ignore(type);
        }
    }
}
