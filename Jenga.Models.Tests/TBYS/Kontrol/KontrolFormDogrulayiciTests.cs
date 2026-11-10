using Jenga.Models.TBYS.Kontrol;
using Xunit;

namespace Jenga.Models.Tests.TBYS.Kontrol
{
    public class KontrolFormDogrulayiciTests
    {
        private static KontrolFormTanimi Form(params KontrolAlanTanimi[] alanlar)
        {
            var t = new KontrolFormTanimi();
            var b = new KontrolBolumTanimi { Kod = "B1", Baslik = "Bölüm" };
            b.Alanlar.AddRange(alanlar);
            t.Bolumler.Add(b);
            return t;
        }

        private static int YokEk(string _, int? __) => 0;

        [Fact]
        public void CevaplariDogrula_TaslakZorunluluguDenetlemez_TamamlamaDenetler()
        {
            var f = Form(new KontrolAlanTanimi { Kod = "z", Baslik = "Zorunlu", Tip = KontrolAlanTipi.KisaMetin, Zorunlu = true });
            var c = new KontrolCevaplari();
            Assert.Empty(KontrolFormDogrulayici.CevaplariDogrula(f, c, YokEk, zorunluKontrol: false));
            Assert.Contains(KontrolFormDogrulayici.CevaplariDogrula(f, c, YokEk, zorunluKontrol: true), h => h.Contains("zorunludur"));
        }

        [Fact]
        public void CevaplariDogrula_TipHatalariTaslaktaDaYakalanir()
        {
            var f = Form(
                new KontrolAlanTanimi { Kod = "n", Baslik = "Sayı", Tip = KontrolAlanTipi.OndalikSayi },
                new KontrolAlanTanimi { Kod = "t", Baslik = "Tarih", Tip = KontrolAlanTipi.Tarih },
                new KontrolAlanTanimi { Kod = "e", Baslik = "EH", Tip = KontrolAlanTipi.EvetHayir },
                new KontrolAlanTanimi { Kod = "c", Baslik = "Çoklu", Tip = KontrolAlanTipi.CokluSecim, Secenekler = { new() { Kod = "x", Etiket = "X" } } });
            var c = new KontrolCevaplari();
            c.Koy("n", "abc"); c.Koy("t", "31/12/2025"); c.Koy("e", "Belki"); c.Koy("c", "x;y");
            var h = KontrolFormDogrulayici.CevaplariDogrula(f, c, YokEk, zorunluKontrol: false);
            Assert.Equal(4, h.Count);
        }

        [Fact]
        public void CevaplariDogrula_TekrarlanabilirGrupEnAzSatir_VeZorunluEk()
        {
            var t = new KontrolFormTanimi();
            var b = new KontrolBolumTanimi { Kod = "B", Baslik = "B" };
            b.Alanlar.Add(new KontrolAlanTanimi { Kod = "foto", Baslik = "Foto", Tip = KontrolAlanTipi.Ek, Zorunlu = true });
            b.Gruplar.Add(new KontrolGrupTanimi
            {
                Kod = "G", Baslik = "Emsaller", Tekrarlanabilir = true, EnAzSatir = 2,
                Alanlar = { new KontrolAlanTanimi { Kod = "fiyat", Baslik = "Fiyat", Tip = KontrolAlanTipi.OndalikSayi, Zorunlu = true } }
            });
            t.Bolumler.Add(b);

            var c = new KontrolCevaplari();
            c.GrupSatirlari("G").Add(new(StringComparer.OrdinalIgnoreCase) { ["fiyat"] = "100" });

            var h = KontrolFormDogrulayici.CevaplariDogrula(t, c, YokEk, zorunluKontrol: true);
            Assert.Contains(h, x => x.Contains("en az 2 satır"));
            Assert.Contains(h, x => x.Contains("en az bir ek"));

            var h2 = KontrolFormDogrulayici.CevaplariDogrula(t, c, (kod, satir) => kod == "foto" ? 1 : 0, zorunluKontrol: true);
            Assert.DoesNotContain(h2, x => x.Contains("en az bir ek"));
        }
    }

    public class KontrolRaporuTests
    {
        [Fact]
        public void SabitRapor_AlanKodlariBenzersizVe19Bolum()
        {
            var f = KontrolRaporu.Olustur();
            Assert.Equal(19, f.Bolumler.Count);
            var kodlar = f.TumAlanlar().Select(a => a.Kod).ToList();
            Assert.Equal(kodlar.Count, kodlar.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        }

        [Fact]
        public void VarsayilanForm_JsonGidisDonusuKayipsiz()
        {
            var f = KontrolRaporu.Olustur();
            var json = f.ToJson();
            var geri = KontrolFormTanimi.FromJson(json);
            Assert.Equal(json, geri.ToJson());
            Assert.Equal(f.TumAlanlar().Count(), geri.TumAlanlar().Count());
        }

        [Fact]
        public void VarsayilanForm_TasinmazKaynakliAlanlarWhitelistte()
        {
            var f = KontrolRaporu.Olustur();
            var kaynakli = f.TumAlanlar().Where(a => a.VeriKaynagi != KontrolVeriKaynagiTipi.Manuel).ToList();
            Assert.NotEmpty(kaynakli);
            Assert.All(kaynakli, a => Assert.True(TasinmazVeriKaynagi.Gecerli(a.TasinmazAlani), a.Kod));
        }

    }
}
