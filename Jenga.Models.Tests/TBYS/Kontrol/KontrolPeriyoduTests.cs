using Jenga.Models.TBYS;
using Jenga.Models.TBYS.Kontrol;
using Xunit;

namespace Jenga.Models.Tests.TBYS.Kontrol
{
    public class KontrolPeriyoduTests
    {
        [Theory]
        [InlineData("2025-01-31", 1, PeriyotBirimi.Ay, "2025-02-28")]   // ay sonu kısaltma
        [InlineData("2024-02-29", 1, PeriyotBirimi.Yil, "2025-02-28")]  // artık yıl
        [InlineData("2025-01-01", 2, PeriyotBirimi.Hafta, "2025-01-15")]
        [InlineData("2025-01-01", 10, PeriyotBirimi.Gun, "2025-01-11")]
        public void Ekle_TakvimKurallariniUygular(string baslangic, int sure, PeriyotBirimi birim, string beklenen)
        {
            var sonuc = KontrolPeriyodu.Ekle(DateTime.Parse(baslangic), sure, birim);
            Assert.Equal(DateTime.Parse(beklenen), sonuc);
        }

        [Fact]
        public void Ekle_SaatBilesenineBakmaz()
        {
            var sonuc = KontrolPeriyodu.Ekle(new DateTime(2025, 3, 10, 17, 45, 0), 1, PeriyotBirimi.Gun);
            Assert.Equal(new DateTime(2025, 3, 11), sonuc);
        }

        [Fact]
        public void Ekle_SifirVeyaNegatifSureReddedilir()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => KontrolPeriyodu.Ekle(DateTime.Today, 0, PeriyotBirimi.Ay));
            Assert.Throws<ArgumentOutOfRangeException>(() => KontrolPeriyodu.Ekle(DateTime.Today, -1, PeriyotBirimi.Ay));
        }

        [Fact]
        public void Gecerli_VeAyni()
        {
            Assert.True(KontrolPeriyodu.Gecerli(6, PeriyotBirimi.Ay));
            Assert.False(KontrolPeriyodu.Gecerli(0, PeriyotBirimi.Ay));
            Assert.False(KontrolPeriyodu.Gecerli(6, null));
            Assert.True(KontrolPeriyodu.Ayni(6, PeriyotBirimi.Ay, 6, PeriyotBirimi.Ay));
            Assert.False(KontrolPeriyodu.Ayni(6, PeriyotBirimi.Ay, 6, PeriyotBirimi.Hafta));
        }
    }

    public class TasinmazVeriKaynagiTests
    {
        [Fact]
        public void Snapshot_SadeceFormunReferansladigiKaynakAlanlariniAlir()
        {
            var t = new Tasinmaz { EmlakSicilNo = "ES-1", Cinsi = "ARSA", Ili = "ANKARA" };
            var tanim = new KontrolFormTanimi();
            var b = new KontrolBolumTanimi { Kod = "B1", Baslik = "B" };
            b.Alanlar.Add(new KontrolAlanTanimi { Kod = "sicil", Baslik = "Sicil", Tip = KontrolAlanTipi.KisaMetin, VeriKaynagi = KontrolVeriKaynagiTipi.TasinmazSaltOkunur, TasinmazAlani = "EmlakSicilNo" });
            b.Alanlar.Add(new KontrolAlanTanimi { Kod = "not", Baslik = "Not", Tip = KontrolAlanTipi.KisaMetin });
            tanim.Bolumler.Add(b);

            var snap = TasinmazVeriKaynagi.Snapshot(t, tanim);

            Assert.Single(snap);
            Assert.Equal("ES-1", snap["EmlakSicilNo"]);
        }

        [Fact]
        public void Gecerli_BilinmeyenAnahtarReddedilir()
        {
            Assert.True(TasinmazVeriKaynagi.Gecerli("EmlakSicilNo"));
            Assert.False(TasinmazVeriKaynagi.Gecerli("DROP TABLE"));
            Assert.False(TasinmazVeriKaynagi.Gecerli(null));
        }
    }
}
