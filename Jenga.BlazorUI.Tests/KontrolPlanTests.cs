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
using Jenga.DataAccess.Services.Common;
using Jenga.Models.Enums;

namespace Jenga.BlazorUI.Tests;

public class KontrolPlanTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public async Task KiraBilgileri_TasinmazinAktifSozlesmesiYoksaBosKalir(bool? aktif)
    {
        await using var factory = await Database.AcAsync();
        var baslangic = DateTime.Today.AddMonths(-1);
        await using (var db = factory.CreateDbContext())
        {
            db.Kiraci_Table.Add(new() { Id = 1, Adi = "Ali", Soyadi = "Demir" });
            db.KiraSozlesme_Table.Add(new() { Id = 1, KiraciId = 1, Aktif = aktif, KiraBedeli = 9876.54m,
                IlkSozlesmeTar = baslangic.AddYears(-2), SozBasTar = baslangic, SozBitTar = baslangic.AddYears(1), OdemeSekli = "Peşin" });
            db.SozlesmeTasinmaz_Table.Add(new() { TasinmazId = 1, SozlesmeId = 1 });
            // Aynı tablodaki başka sözleşme bu taşınmaza bağlı değildir.
            db.KiraSozlesme_Table.Add(new() { Id = 2, KiraciId = 1, Aktif = true, SozBasTar = DateTime.Today, KiraBedeli = 99999m });
            await db.SaveChangesAsync();
        }
        var servis = Servis(factory);
        var sonuc = await servis.GetByIdAsync(1);
        Assert.True(sonuc.IsSuccess, sonuc.IsFailure ? sonuc.Error.Message : "");
        var cevaplar = KontrolCevaplari.FromJson(sonuc.Value.CevaplarJson);
        Assert.Empty(cevaplar.Alanlar);
    }

    [Fact]
    public async Task KiraBilgileri_SadeceBedelBossaDoldurur_DigerCevaplariKorur()
    {
        await using var factory = await Database.AcAsync();
        await KiraKayitlariniEkleAsync(factory);
        var cevaplar = new KontrolCevaplari();
        cevaplar.Koy("kira_kiraci_adi", "Kayıtlı Kiracı");
        cevaplar.Koy("kira_baslangic", "2020-01-01");
        cevaplar.Koy("kira_sozlesme_baslangic", "2026-01-01");
        cevaplar.Koy("kira_bitis", "2027-01-01");
        cevaplar.Koy("kira_odeme_duzeni", "Kayıtlı ödeme şekli");
        await using (var db = factory.CreateDbContext())
        {
            (await db.TasinmazKontrol_Table.SingleAsync()).CevaplarJson = cevaplar.ToJson();
            await db.SaveChangesAsync();
        }
        var sonuc = await Servis(factory).GetByIdAsync(1);
        Assert.True(sonuc.IsSuccess, sonuc.IsFailure ? sonuc.Error.Message : "");
        var dolu = KontrolCevaplari.FromJson(sonuc.Value.CevaplarJson);
        Assert.Equal("12345.67", dolu.Al("kira_bedel"));
        foreach (var alan in cevaplar.Alanlar) Assert.Equal(alan.Value, dolu.Al(alan.Key));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task KiraBilgileri_GuncelSozlesmedenDolar_KaydetVeYenidenAcmadaKorunur(bool yeniKontrol)
    {
        await using var factory = await Database.AcAsync();
        await KiraKayitlariniEkleAsync(factory);
        var servis = Servis(factory);
        var id = 1;
        if (yeniKontrol)
        {
            await using (var db = factory.CreateDbContext())
            {
                (await db.TasinmazKontrol_Table.SingleAsync()).Durum = KontrolDurumu.Tamamlandi;
                await db.SaveChangesAsync();
            }
            var baslat = await servis.BaslatAsync(new() { TasinmazId = 1, KontrolTarihi = DateTime.Today });
            Assert.True(baslat.IsSuccess, baslat.IsFailure ? baslat.Error.Message : "");
            id = baslat.Value;
        }
        var sonuc = await servis.GetByIdAsync(id);
        Assert.True(sonuc.IsSuccess, sonuc.IsFailure ? sonuc.Error.Message : "");
        var cevaplar = KontrolCevaplari.FromJson(sonuc.Value.CevaplarJson);
        Assert.Equal("12345.67", cevaplar.Al("kira_bedel"));
        Assert.Equal("Ayşe Yılmaz", cevaplar.Al("kira_kiraci_adi"));
        Assert.Equal(DateTime.Today.AddYears(-3).ToString("yyyy-MM-dd"), cevaplar.Al("kira_baslangic"));
        Assert.Equal(DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd"), cevaplar.Al("kira_sozlesme_baslangic"));
        Assert.Equal(DateTime.Today.AddYears(1).ToString("yyyy-MM-dd"), cevaplar.Al("kira_bitis"));
        Assert.Equal("Aylık", cevaplar.Al("kira_odeme_duzeni"));
        cevaplar.Koy("kira_kiraci_adi", "Elle güncellenen isim");
        var kaydet = await servis.TaslakKaydetAsync(id, sonuc.Value.KayitSurumu, cevaplar, null, DateTime.Today);
        Assert.True(kaydet.IsSuccess, kaydet.IsFailure ? kaydet.Error.Message : "");
        Assert.Equal("Elle güncellenen isim", KontrolCevaplari.FromJson((await servis.GetByIdAsync(id)).Value.CevaplarJson).Al("kira_kiraci_adi"));
    }

    [Fact]
    public async Task KiraBilgileri_SozlesmeYokkenBosKalir_TamamlanmisRaporuDegistirmez()
    {
        await using var factory = await Database.AcAsync();
        var servis = Servis(factory);
        var bos = await servis.GetByIdAsync(1);
        Assert.True(bos.IsSuccess);
        Assert.Null(KontrolCevaplari.FromJson(bos.Value.CevaplarJson).Al("kira_kiraci_adi"));
        await KiraKayitlariniEkleAsync(factory);
        await using (var db = factory.CreateDbContext())
        {
            var kontrol = await db.TasinmazKontrol_Table.SingleAsync();
            kontrol.Durum = KontrolDurumu.Tamamlandi;
            kontrol.CevaplarJson = "{}";
            await db.SaveChangesAsync();
        }
        Assert.Equal("{}", (await servis.GetByIdAsync(1)).Value.CevaplarJson);
    }

    private static async Task KiraKayitlariniEkleAsync(Database factory)
    {
        await using var db = factory.CreateDbContext();
        db.Kiraci_Table.Add(new() { Id = 1, Adi = "Ayşe", Soyadi = "Yılmaz" });
        db.KiraSozlesme_Table.AddRange(
            new KiraSozlesme { Id = 1, KiraciId = 1, Aktif = true, SozBasTar = DateTime.Today.AddYears(-2), OdemeSekli = "Eski" },
            new KiraSozlesme { Id = 2, KiraciId = 1, Aktif = true, KiraBedeli = 12345.67m, SozBasTar = DateTime.Today.AddMonths(-1), IlkSozlesmeTar = DateTime.Today.AddYears(-3), SozBitTar = DateTime.Today.AddYears(1), OdemeSekli = "Aylık" },
            new KiraSozlesme { Id = 3, KiraciId = 1, Aktif = true, SozBasTar = DateTime.Today.AddMonths(1), OdemeSekli = "Gelecek" },
            new KiraSozlesme { Id = 4, KiraciId = 1, Aktif = false, SozBasTar = DateTime.Today, OdemeSekli = "Pasif" });
        for (int i = 1; i <= 4; i++) db.SozlesmeTasinmaz_Table.Add(new() { SozlesmeId = i, TasinmazId = 1 });
        await db.SaveChangesAsync();
    }

    [Theory]
    [InlineData("TM")]
    [InlineData("ÇM")]
    public async Task ListeVePlan_BolgeKisaAdiMulkiyetVeBagisciyiGetirir(string mulkiyet)
    {
        await using var factory = await Database.AcAsync();
        await using (var db = factory.CreateDbContext())
        {
            db.Bolge_Table.Add(new() { Id = 1, Adi = "Merkez Bölge", KisaAdi = "MRK" });
            db.Il_Table.Add(new() { Id = 1, IlAdi = "Ankara", BolgeId = 1 });
            var tasinmaz = await db.Tasinmaz_Table.SingleAsync();
            tasinmaz.IlId = 1; tasinmaz.EnvanterdeMi = 1;
            tasinmaz.MulkiyetSekli = mulkiyet; tasinmaz.Bagisci = "Ayşe Yılmaz";
            var kontrol = await db.TasinmazKontrol_Table.SingleAsync();
            kontrol.BolgeId = 1; kontrol.BolgeAdi = "Merkez Bölge";
            await db.SaveChangesAsync();
        }
        var servis = Servis(factory, new HaritaYetkisi(true, [1]));
        var planSonucu = await servis.GetPlanListesiAsync();
        Assert.True(planSonucu.IsSuccess, planSonucu.IsFailure ? planSonucu.Error.Message : "");
        var plan = Assert.Single(planSonucu.Value);
        Assert.Equal("MRK", plan.BolgeKisaAdi);
        Assert.Equal("Merkez Bölge", plan.BolgeAdi);
        Assert.Equal(mulkiyet, plan.MulkiyetSekli);
        Assert.Equal("Ayşe Yılmaz", plan.Bagisci);
        var listeSonucu = await servis.GetListeAsync();
        Assert.True(listeSonucu.IsSuccess, listeSonucu.IsFailure ? listeSonucu.Error.Message : "");
        var kayit = Assert.Single(listeSonucu.Value);
        Assert.Equal("MRK", kayit.BolgeKisaAdi);
        Assert.Equal("Merkez Bölge", kayit.BolgeAdi);
        Assert.Equal(mulkiyet, kayit.MulkiyetSekli);
        Assert.Equal("Ayşe Yılmaz", kayit.Bagisci);
    }

    [Fact]
    public async Task Harita_YetkiliTasinmazlarinSonGecerliKoordinatiniVeParselSiniriniGetirir()
    {
        const string geometri = """{"type":"Polygon","coordinates":[[[32,39],[33,39],[33,40],[32,39]]]}""";
        await using var factory = await Database.AcAsync();
        await using (var db = factory.CreateDbContext())
        {
            db.Bolge_Table.AddRange(new Bolge { Id = 1, Adi = "Yetkili", KisaAdi = "YT" }, new Bolge { Id = 2, Adi = "Diğer", KisaAdi = "DG" });
            db.Il_Table.AddRange(new Il { Id = 1, BolgeId = 1, IlAdi = "Ankara" }, new Il { Id = 2, BolgeId = 2, IlAdi = "İzmir" });
            var t = await db.Tasinmaz_Table.SingleAsync();
            t.IlId = 1; t.EmlakSicilNo = "S-1"; t.Adres = "Test adresi"; t.Cinsi = "Mesken"; t.Ili = "Ankara"; t.Ilcesi = "Çankaya";
            t.ParselGeometrisiGeoJson = geometri;
            db.Tasinmaz_Table.AddRange(
                new Tasinmaz { Id = 2, IlId = 1 },
                new Tasinmaz { Id = 3, IlId = 2, ParselGeometrisiGeoJson = geometri },
                new Tasinmaz { Id = 4, IlId = 1 },
                new Tasinmaz { Id = 5, IlId = 1, ParselGeometrisiGeoJson = geometri });
            var ilk = await db.TasinmazKontrol_Table.SingleAsync();
            ilk.KontrolTarihi = new(2020, 1, 1); ilk.CevaplarJson = KoordinatJson("38, 31");
            db.TasinmazKontrol_Table.AddRange(
                Rapor(2, 1, "39, 32", KontrolDurumu.Tamamlandi),
                Rapor(3, 1, "40, 33"),
                Rapor(4, 1, "91, 34"),
                Rapor(5, 1, "41, 34", KontrolDurumu.IptalEdildi),
                Rapor(6, 1, "42, 35", KontrolDurumu.RevizeEdildi),
                Rapor(7, 2, "40, 33"),
                Rapor(8, 3, "38, 27"),
                Rapor(9, 4, "koordinat yok"));
            var bozuk = Rapor(10, 1, ""); bozuk.CevaplarJson = "bozuk JSON";
            db.TasinmazKontrol_Table.Add(bozuk);
            await db.SaveChangesAsync();
        }
        var servis = Servis(factory, new HaritaYetkisi(true, [1]));
        var sonuc = await servis.GetHaritaNoktalariAsync();
        Assert.True(sonuc.IsSuccess, sonuc.IsFailure ? sonuc.Error.Message : "");
        Assert.Equal(new[] { 1, 2, 5 }, sonuc.Value.Select(n => n.TasinmazId).OrderBy(id => id));
        var nokta = Assert.Single(sonuc.Value, n => n.TasinmazId == 1);
        Assert.Equal(3, nokta.KontrolId);
        Assert.Equal(40m, nokta.Enlem); Assert.Equal(33m, nokta.Boylam);
        Assert.Equal("S-1", nokta.EmlakSicilNo); Assert.Equal("Test adresi", nokta.Adres);
        Assert.Equal("Mesken", nokta.Cinsi); Assert.Equal("Ankara", nokta.Ili); Assert.Equal("Çankaya", nokta.Ilcesi);
        Assert.True(nokta.Taslak);
        Assert.Equal(geometri, nokta.ParselGeometrisiGeoJson);
        var sadeceParsel = Assert.Single(sonuc.Value, n => n.TasinmazId == 5);
        Assert.Equal(geometri, sadeceParsel.ParselGeometrisiGeoJson);
        Assert.Null(sadeceParsel.KontrolId);
        Assert.Null(sadeceParsel.Enlem);
        Assert.Null(sadeceParsel.Boylam);
        Assert.Empty((await Servis(factory, new HaritaYetkisi(true, [])).GetHaritaNoktalariAsync()).Value);
        Assert.True((await Servis(factory, new HaritaYetkisi(false, [1, 2])).GetHaritaNoktalariAsync()).IsFailure);
    }

    private static string KoordinatJson(string deger)
    {
        var cevaplar = new KontrolCevaplari(); cevaplar.Koy("konum_koordinat", deger); return cevaplar.ToJson();
    }
    private static TasinmazKontrol Rapor(int id, int tasinmazId, string koordinat, KontrolDurumu durum = KontrolDurumu.Taslak)
        => new() { Id = id, TasinmazId = tasinmazId, KontrolTarihi = new(2026, 1, 1), Durum = durum, CevaplarJson = KoordinatJson(koordinat) };
    private sealed class HaritaYetkisi(bool izin, IReadOnlyList<int> bolgeler) : IServiceAuthorizationContext
    {
        public Task<bool> HasPermissionAsync(ModuleName module, Operation operation, CancellationToken cancellationToken = default) => Task.FromResult(izin);
        public Task<string?> GetUserNameAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>("Test");
        public Task<IReadOnlyList<int>> GetAuthorizedRegionIdsAsync(CancellationToken cancellationToken = default) => Task.FromResult(bolgeler);
    }

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

    private static TasinmazKontrolService Servis(Database factory, IServiceAuthorizationContext? auth = null) => new(factory, new DbContextScopeFactory(factory),
        new DosyaSistemiKontrolEkDeposu(new ConfigurationBuilder().Build()), new NullLogService(), auth);

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
            var types = new[] { typeof(Tasinmaz), typeof(TasinmazKontrol), typeof(TasinmazKontrolPlani), typeof(TasinmazKontrolEk), typeof(Il), typeof(Bolge), typeof(KiraSozlesme), typeof(Kiraci), typeof(SozlesmeTasinmaz) };
            foreach (var type in builder.Model.GetEntityTypes().Select(x => x.ClrType).Where(t => !types.Contains(t)).ToArray())
                builder.Ignore(type);
            builder.Entity<KiraSozlesme>().Property(s => s.TeminatAciklama).HasColumnType("TEXT");
        }
    }
}
