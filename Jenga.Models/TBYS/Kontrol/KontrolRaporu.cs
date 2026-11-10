using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jenga.Models.TBYS.Kontrol
{
    /// <summary>Sabit raporun alan metaverisi ve eski raporlardaki alanların okunması için kullanılır.</summary>
    public sealed class KontrolFormTanimi
    {
        public List<KontrolBolumTanimi> Bolumler { get; set; } = new();

        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Converters = { new JsonStringEnumConverter() }
        };

        public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

        public static KontrolFormTanimi FromJson(string? json)
            => string.IsNullOrWhiteSpace(json)
                ? new KontrolFormTanimi()
                : JsonSerializer.Deserialize<KontrolFormTanimi>(json, JsonOptions) ?? new KontrolFormTanimi();

        /// <summary>Tüm alanları (grup içindekiler dâhil) düz liste olarak döner.</summary>
        public IEnumerable<KontrolAlanTanimi> TumAlanlar()
        {
            foreach (var b in Bolumler)
            {
                foreach (var a in b.Alanlar) yield return a;
                foreach (var g in b.Gruplar)
                    foreach (var a in g.Alanlar) yield return a;
            }
        }

        public KontrolAlanTanimi? AlanBul(string kod)
            => TumAlanlar().FirstOrDefault(a => string.Equals(a.Kod, kod, StringComparison.OrdinalIgnoreCase));

    }

    public sealed class KontrolBolumTanimi
    {
        public string Kod { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public int Sira { get; set; }
        public List<KontrolAlanTanimi> Alanlar { get; set; } = new();
        public List<KontrolGrupTanimi> Gruplar { get; set; } = new();
    }

    /// <summary>
    /// Alt alanları olan grup. <see cref="Tekrarlanabilir"/> ise kullanıcı birden fazla satır girebilir
    /// (ör. emsaller, irtibat kişileri); değilse tek satırlık alt alan kümesidir (ör. incelenen belge).
    /// </summary>
    public sealed class KontrolGrupTanimi
    {
        public string Kod { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public int Sira { get; set; }
        public bool Tekrarlanabilir { get; set; }
        public int? EnAzSatir { get; set; }
        public List<KontrolAlanTanimi> Alanlar { get; set; } = new();
    }

    public sealed class KontrolAlanTanimi
    {
        /// <summary>Cevapların kalıcı alan kodu. Anlamı değişen soruya yeni kod verilir.</summary>
        public string Kod { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public int Sira { get; set; }
        public KontrolAlanTipi Tip { get; set; }
        public bool Zorunlu { get; set; }
        public string? Birim { get; set; }
        public List<KontrolSecenekTanimi> Secenekler { get; set; } = new();

        public KontrolVeriKaynagiTipi VeriKaynagi { get; set; } = KontrolVeriKaynagiTipi.Manuel;

        /// <summary><see cref="TasinmazVeriKaynagi"/> içinde izin verilen anahtarlardan biri.</summary>
        public string? TasinmazAlani { get; set; }

        public bool SecenekliMi => Tip is KontrolAlanTipi.TekliSecim or KontrolAlanTipi.CokluSecim;
    }

    public sealed class KontrolSecenekTanimi
    {
        public string Kod { get; set; } = string.Empty;
        public string Etiket { get; set; } = string.Empty;
        public int Sira { get; set; }
    }

    /// <summary>
    /// Bir kontrol kaydının cevapları. <see cref="TasinmazKontrol.CevaplarJson"/> içinde saklanır.
    /// Anahtar: alan kodu. Grup cevapları satır listesi olarak tutulur.
    /// Boş cevap = anahtar yok ya da null; EvetHayır alanında Hayır ve Uygulanamaz ayrı değerlerdir.
    /// </summary>
    public sealed class KontrolCevaplari
    {
        public Dictionary<string, string?> Alanlar { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Grup kodu → satırlar; her satır alan kodu → değer.</summary>
        public Dictionary<string, List<Dictionary<string, string?>>> Gruplar { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public string ToJson() => JsonSerializer.Serialize(this, KontrolFormTanimi.JsonOptions);

        public static KontrolCevaplari FromJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new KontrolCevaplari();
            var c = JsonSerializer.Deserialize<KontrolCevaplari>(json, KontrolFormTanimi.JsonOptions) ?? new KontrolCevaplari();
            c.Alanlar = new Dictionary<string, string?>(c.Alanlar, StringComparer.OrdinalIgnoreCase);
            c.Gruplar = new Dictionary<string, List<Dictionary<string, string?>>>(c.Gruplar, StringComparer.OrdinalIgnoreCase);
            return c;
        }

        public string? Al(string kod) => Alanlar.TryGetValue(kod, out var v) ? v : null;

        public void Koy(string kod, string? deger)
        {
            if (string.IsNullOrWhiteSpace(deger)) Alanlar.Remove(kod);
            else Alanlar[kod] = deger;
        }

        public List<Dictionary<string, string?>> GrupSatirlari(string grupKodu)
        {
            if (!Gruplar.TryGetValue(grupKodu, out var satirlar))
            {
                satirlar = new List<Dictionary<string, string?>>();
                Gruplar[grupKodu] = satirlar;
            }
            return satirlar;
        }

        /// <summary>Çoklu seçim cevapları ';' ile ayrılmış seçenek kodlarıdır.</summary>
        public const char CokluSecimAyirici = ';';
    }
}

namespace Jenga.Models.TBYS.Kontrol
{
    /// <summary>Uygulamanın sabit taşınmaz kontrol raporu. Kullanıcı tarafından şablon tanımlanmaz.</summary>
    public static class KontrolRaporu
    {
        public const int VarsayilanPeriyotSure = 1;
        public const PeriyotBirimi VarsayilanPeriyotBirim = PeriyotBirimi.Yil;
        public const string EskiAlanlarAnahtari = "_kontrolAlanlari";

        /// <summary>Geçişten önceki raporların alanları kendi kayıtlarından, yenilerin alanları sabit formdan okunur.</summary>
        public static KontrolFormTanimi TanimAl(string? snapshotJson)
        {
            var snapshot = SnapshotOku(snapshotJson);
            return snapshot.TryGetValue(EskiAlanlarAnahtari, out var json) && !string.IsNullOrWhiteSpace(json)
                ? KontrolFormTanimi.FromJson(json)
                : Olustur();
        }

        public static (int? Sure, PeriyotBirimi? Birim) PeriyotAl(string? snapshotJson)
        {
            var snapshot = SnapshotOku(snapshotJson);
            if (!snapshot.ContainsKey(EskiAlanlarAnahtari)) return (VarsayilanPeriyotSure, VarsayilanPeriyotBirim);
            int? sure = int.TryParse(snapshot.GetValueOrDefault("_kontrolPeriyotSure"), out var s) ? s : null;
            PeriyotBirimi? birim = Enum.TryParse<PeriyotBirimi>(snapshot.GetValueOrDefault("_kontrolPeriyotBirim"), out var b) && Enum.IsDefined(b) ? b : null;
            return (sure, birim);
        }

        /// <summary>Eski raporun revizyonunda alanlar ve periyot başlangıcı korunur.</summary>
        public static void GecmisBilgileriniKoru(Dictionary<string, string?> snapshot, string? oncekiSnapshotJson)
        {
            var onceki = SnapshotOku(oncekiSnapshotJson);
            foreach (var key in new[] { EskiAlanlarAnahtari, "_kontrolPeriyotSure", "_kontrolPeriyotBirim" })
                if (onceki.TryGetValue(key, out var value)) snapshot[key] = value;
        }

        private static Dictionary<string, string?> SnapshotOku(string? json) => string.IsNullOrWhiteSpace(json)
            ? new()
            : JsonSerializer.Deserialize<Dictionary<string, string?>>(json, KontrolFormTanimi.JsonOptions) ?? new();

        public static KontrolFormTanimi Olustur()
        {
            var t = new KontrolFormTanimi();
            int bs = 0;

            KontrolBolumTanimi Bolum(string kod, string baslik)
            {
                var b = new KontrolBolumTanimi { Kod = kod, Baslik = baslik, Sira = ++bs };
                t.Bolumler.Add(b);
                return b;
            }

            // Raporu imzalayan makam, kontrolü yapanlar, kontrol tarihi ve rapor no form dışında, kontrol kaydının kendi alanlarıdır.
            // Rapor no tamamlanma anında sunucuda üretilir (Bölge-Plaka-Yıl-Sıra).

            // 3. Ana taşınmaz (taşınmazdan)
            var b3 = Bolum("ana_tasinmaz", "Ana Taşınmaz Bilgileri");
            Kaynak(b3, "t_sicil", "Emlak Sicil No", "EmlakSicilNo");
            Kaynak(b3, "t_cins", "Cinsi", "Cinsi");
            Kaynak(b3, "t_il", "İli", "Ili");
            Kaynak(b3, "t_ilce", "İlçesi", "Ilcesi");
            Kaynak(b3, "t_mahalle", "Mahalle / Köy", "Mahalle");
            Kaynak(b3, "t_adres", "Adres", "Adres", KontrolVeriKaynagiTipi.TasinmazBaslangic, KontrolAlanTipi.UzunMetin);
            Kaynak(b3, "t_pafta", "Pafta", "PaftaNo");
            Kaynak(b3, "t_ada", "Ada", "AdaNo");
            Kaynak(b3, "t_parsel", "Parsel", "ParselNo");
            Kaynak(b3, "t_yuzolcumu", "Parsel Alanı", "Yuzolcumu");
            Kaynak(b3, "t_ana_nitelik", "Ana Taşınmaz Niteliği", "AnaTasinmazNitelik");
            Kaynak(b3, "t_kat_mulkiyeti", "Kat Mülkiyeti", "KatMulkiyeti");
            Kaynak(b3, "t_kat_irtifaki", "Kat İrtifakı", "KatIrtifaki");
            Kaynak(b3, "t_tapu_tarihi", "Tapu Tarihi", "TapuTarihi");
            Kaynak(b3, "t_yevmiye", "Yevmiye No", "YevmiyeNo");

            // 4. Bağımsız bölüm (taşınmazdan)
            var b4 = Bolum("bagimsiz_bolum", "Bağımsız Bölüm Bilgileri");
            Kaynak(b4, "bb_blok", "Blok", "Blok");
            Kaynak(b4, "bb_giris", "Giriş", "Giris");
            Kaynak(b4, "bb_kat", "Bulunduğu Kat", "BulunduguKat");
            Kaynak(b4, "bb_no", "Bağımsız Bölüm No", "BagimsizBolumNo");
            Kaynak(b4, "bb_nitelik", "Niteliği", "BBNitelik");
            Kaynak(b4, "bb_arsa_payi", "Arsa Payı", "ArsaPayi");
            Kaynak(b4, "bb_brut", "Brüt Alan (m²)", "BBBrutAlan", KontrolVeriKaynagiTipi.TasinmazBaslangic, KontrolAlanTipi.OndalikSayi);
            Kaynak(b4, "bb_net", "Net Alan (m²)", "BBNetAlan", KontrolVeriKaynagiTipi.TasinmazBaslangic, KontrolAlanTipi.OndalikSayi);

            // 5. İncelenen belgeler
            var b5 = Bolum("belgeler", "İncelenen Belgeler");
            foreach (var (k, e) in new[] { ("tapu", "Tapu Kaydı"), ("imar", "İmar Durumu"), ("ruhsat", "Yapı Ruhsatı"), ("iskan", "İskan Belgesi"), ("proje", "Mimari Proje"), ("kira", "Kira Sözleşmesi") })
                Alan(b5, "belge_" + k, e, KontrolAlanTipi.EvetHayir);
            Alan(b5, "belge_aciklama", "Belgelere İlişkin Açıklama", KontrolAlanTipi.UzunMetin);

            // 6. İmar bilgileri
            var b6 = Bolum("imar", "İmar Bilgileri");
            Alan(b6, "imar_plan", "İmar Planı Durumu", KontrolAlanTipi.KisaMetin);
            Alan(b6, "imar_lejant", "Lejant / Kullanım Kararı", KontrolAlanTipi.KisaMetin);
            Alan(b6, "imar_taks", "TAKS", KontrolAlanTipi.OndalikSayi);
            Alan(b6, "imar_emsal", "Emsal (E)", KontrolAlanTipi.OndalikSayi);
            Alan(b6, "imar_hmax", "Hmax", KontrolAlanTipi.KisaMetin);
            Alan(b6, "imar_aciklama", "İmar Açıklaması", KontrolAlanTipi.UzunMetin);

            // 7. Bina özellikleri
            var b7 = Bolum("bina", "Bina Özellikleri");
            Kaynak(b7, "bina_insa_yili", "İnşa Yılı", "InsaYili", KontrolVeriKaynagiTipi.TasinmazBaslangic);
            Kaynak(b7, "bina_kat_sayisi", "Toplam Kat Sayısı", "ToplamKatSayisi", KontrolVeriKaynagiTipi.TasinmazBaslangic);
            Secim(b7, "bina_yapi_tarzi", "Yapı Tarzı", false, "BAK|Betonarme Karkas", "Yigma|Yığma", "Celik|Çelik", "Prefabrik|Prefabrik", "Diger|Diğer");
            Secim(b7, "bina_dis_cephe", "Dış Cephe", false, "Siva|Sıva/Boya", "Mantolama|Mantolama", "Kaplama|Kaplama", "Diger|Diğer");
            Alan(b7, "bina_asansor", "Asansör", KontrolAlanTipi.EvetHayir);
            Alan(b7, "bina_otopark", "Otopark", KontrolAlanTipi.EvetHayir);
            Alan(b7, "bina_isitma", "Isıtma Sistemi", KontrolAlanTipi.KisaMetin);
            Alan(b7, "bina_aciklama", "Bina Açıklaması", KontrolAlanTipi.UzunMetin);

            // 8. Bağımsız bölüm özellikleri
            var b8 = Bolum("bb_ozellik", "Bağımsız Bölüm Özellikleri");
            Alan(b8, "bbo_oda", "Oda Sayısı", KontrolAlanTipi.KisaMetin);
            Alan(b8, "bbo_salon", "Salon Sayısı", KontrolAlanTipi.TamSayi);
            Alan(b8, "bbo_banyo", "Banyo/WC Sayısı", KontrolAlanTipi.TamSayi);
            Alan(b8, "bbo_balkon", "Balkon", KontrolAlanTipi.EvetHayir);
            Alan(b8, "bbo_cephe", "Cephe", KontrolAlanTipi.KisaMetin);
            Alan(b8, "bbo_zemin", "Zemin Kaplaması", KontrolAlanTipi.KisaMetin);
            Secim(b8, "bbo_durum", "Genel Durum", false, "Iyi|İyi", "Orta|Orta", "Kotu|Kötü");
            Alan(b8, "bbo_aciklama", "Açıklama", KontrolAlanTipi.UzunMetin);

            // 9. Site / sosyal donatı
            var b9 = Bolum("site", "Site ve Sosyal Donatılar");
            Alan(b9, "site_icinde", "Site İçinde mi?", KontrolAlanTipi.EvetHayir);
            Alan(b9, "site_adi", "Site Adı", KontrolAlanTipi.KisaMetin);
            Secim(b9, "site_donati", "Sosyal Donatılar", true, "Guvenlik|Güvenlik", "Havuz|Havuz", "Spor|Spor Alanı", "Cocuk|Çocuk Oyun Alanı", "Yesil|Yeşil Alan", "Kapali_Otopark|Kapalı Otopark");
            Alan(b9, "site_aidat", "Aidat (TL)", KontrolAlanTipi.OndalikSayi);

            // 10. Konum / çevre
            var b10 = Bolum("konum", "Konum ve Çevre Bilgileri");
            Alan(b10, "konum_ulasim", "Ulaşım İmkânları", KontrolAlanTipi.UzunMetin);
            Alan(b10, "konum_cevre", "Çevre Yapılaşma", KontrolAlanTipi.UzunMetin);
            Secim(b10, "konum_gelisme", "Bölgenin Gelişme Eğilimi", false, "Gelisen|Gelişen", "Durgun|Durgun", "Gerileyen|Gerileyen");
            Alan(b10, "konum_koordinat", "Koordinat", KontrolAlanTipi.KisaMetin);

            // 11. Yakın çevre tesisleri
            var b11 = Bolum("yakin_cevre", "Yakın Çevre Tesisleri");
            var g11 = Grup(b11, "yakin_tesis", "Tesisler", tekrar: true);
            Alan(g11, "tesis_tur", "Tesis Türü", KontrolAlanTipi.KisaMetin);
            Alan(g11, "tesis_ad", "Adı", KontrolAlanTipi.KisaMetin);
            Alan(g11, "tesis_uzaklik", "Uzaklığı (m)", KontrolAlanTipi.OndalikSayi, birim: "m");

            // 12. Emsaller
            var b12 = Bolum("emsal", "Emsal Bilgileri");
            var g12 = Grup(b12, "emsaller", "Emsaller", tekrar: true);
            Secim(g12, "emsal_tur", "Türü", false, "Satilik|Satılık", "Kiralik|Kiralık");
            Alan(g12, "emsal_adres", "Adres / Konum", KontrolAlanTipi.KisaMetin);
            Alan(g12, "emsal_alan", "Alan (m²)", KontrolAlanTipi.OndalikSayi, birim: "m²");
            Alan(g12, "emsal_bedel", "Bedel (TL)", KontrolAlanTipi.OndalikSayi, birim: "TL");
            Alan(g12, "emsal_birim", "Birim Bedel (TL/m²)", KontrolAlanTipi.OndalikSayi, birim: "TL/m²");
            Alan(g12, "emsal_uzaklik", "Konu Taşınmaza Uzaklığı (m)", KontrolAlanTipi.OndalikSayi, birim: "m");
            Alan(g12, "emsal_kaynak", "Kaynak / İrtibat", KontrolAlanTipi.KisaMetin);
            Alan(g12, "emsal_aciklama", "Açıklama", KontrolAlanTipi.UzunMetin);

            // 13. Kroki
            var b13 = Bolum("kroki", "Kroki");
            Alan(b13, "kroki_ek", "Kroki / Vaziyet Planı", KontrolAlanTipi.Ek);

            // 14. Fotoğraflar
            var b14 = Bolum("foto", "Fotoğraflar");
            Alan(b14, "foto_dis", "Dış Cephe Fotoğrafları", KontrolAlanTipi.Ek);
            Alan(b14, "foto_ic", "İç Mekân Fotoğrafları", KontrolAlanTipi.Ek);
            Alan(b14, "foto_cevre", "Çevre Fotoğrafları", KontrolAlanTipi.Ek);

            // 15. Takyidat
            var b15 = Bolum("takyidat", "Takyidat Bilgileri");
            Alan(b15, "takyidat_var", "Takyidat Var mı?", KontrolAlanTipi.EvetHayir);
            var g15 = Grup(b15, "takyidatlar", "Takyidatlar", tekrar: true);
            Secim(g15, "takyidat_tur", "Türü", false, "Ipotek|İpotek", "Haciz|Haciz", "Serh|Şerh", "Irtifak|İrtifak", "Beyan|Beyan", "Diger|Diğer");
            Alan(g15, "takyidat_lehtar", "Lehtar", KontrolAlanTipi.KisaMetin);
            Alan(g15, "takyidat_tarih", "Tarih", KontrolAlanTipi.Tarih);
            Alan(g15, "takyidat_aciklama", "Açıklama", KontrolAlanTipi.UzunMetin);

            // 16. Kontrol sıklığı / taşınmaz nitelikleri
            var b16 = Bolum("siklik", "Kontrol Sıklığı ve Taşınmaz Nitelikleri");
            Kaynak(b16, "siklik_kullanim", "Kullanım Şekli", "KullanimSekli", KontrolVeriKaynagiTipi.TasinmazBaslangic);
            Kaynak(b16, "siklik_kira", "Kira Durumu", "KiraDurumu", KontrolVeriKaynagiTipi.TasinmazBaslangic);
            Secim(b16, "siklik_oneri", "Önerilen Kontrol Sıklığı", false, "3Ay|3 Ay", "6Ay|6 Ay", "1Yil|1 Yıl", "2Yil|2 Yıl");
            Alan(b16, "siklik_gerekce", "Gerekçe", KontrolAlanTipi.UzunMetin);

            // 17. İrtibat / kira
            var b17 = Bolum("irtibat", "İrtibat Kişileri ve Kira Bilgileri");
            var g17 = Grup(b17, "irtibat_kisiler", "İrtibat Kişileri", tekrar: true);
            Alan(g17, "irtibat_ad", "Adı Soyadı", KontrolAlanTipi.KisaMetin);
            Alan(g17, "irtibat_sifat", "Sıfatı", KontrolAlanTipi.KisaMetin);
            Alan(g17, "irtibat_tel", "Telefon", KontrolAlanTipi.KisaMetin);
            Alan(b17, "kira_bedel", "Aylık Kira Bedeli (TL)", KontrolAlanTipi.OndalikSayi, birim: "TL");
            Alan(b17, "kira_baslangic", "Kira Başlangıç Tarihi", KontrolAlanTipi.Tarih);
            Alan(b17, "kira_bitis", "Kira Bitiş Tarihi", KontrolAlanTipi.Tarih);
            Alan(b17, "kira_odeme_duzeni", "Ödeme Düzeni", KontrolAlanTipi.KisaMetin);

            // 18. Bölgesel gelişmeler
            var b18 = Bolum("bolgesel", "Bölgesel Gelişmeler");
            Alan(b18, "bolgesel_aciklama", "Bölgedeki Gelişmeler / Projeler", KontrolAlanTipi.UzunMetin);

            // 19. Kanaat / özet
            var b19 = Bolum("kanaat", "Kanaat ve Özet");
            Alan(b19, "kanaat_ozet", "Genel Değerlendirme", KontrolAlanTipi.UzunMetin, zorunlu: true);
            Alan(b19, "kanaat_oneri", "Öneriler", KontrolAlanTipi.UzunMetin);
            Alan(b19, "kanaat_rayic", "Tahmini Rayiç Değer (TL)", KontrolAlanTipi.OndalikSayi, birim: "TL");

            // 20. Bağışçı verileri
            var b20 = Bolum("bagisci", "Bağışçı Verileri");
            Kaynak(b20, "bagisci_ad", "Bağışçı", "Bagisci");
            Alan(b20, "bagisci_not", "Bağışçıya İlişkin Notlar", KontrolAlanTipi.UzunMetin);

            // 21. Kullanan kira sözleşmesini imzaladı mı
            var b21 = Bolum("sozlesme", "Kira Sözleşmesi");
            Alan(b21, "sozlesme_imza", "Kullanan Kira Sözleşmesini İmzaladı mı?", KontrolAlanTipi.EvetHayir);
            Alan(b21, "sozlesme_aciklama", "Açıklama", KontrolAlanTipi.UzunMetin);

            return t;
        }

        private static KontrolAlanTanimi Alan(KontrolBolumTanimi b, string kod, string baslik, KontrolAlanTipi tip, bool zorunlu = false, string? birim = null)
        {
            var a = new KontrolAlanTanimi { Kod = kod, Baslik = baslik, Tip = tip, Zorunlu = zorunlu, Birim = birim, Sira = b.Alanlar.Count + 1 };
            b.Alanlar.Add(a);
            return a;
        }

        private static KontrolAlanTanimi Alan(KontrolGrupTanimi g, string kod, string baslik, KontrolAlanTipi tip, bool zorunlu = false, string? birim = null)
        {
            var a = new KontrolAlanTanimi { Kod = kod, Baslik = baslik, Tip = tip, Zorunlu = zorunlu, Birim = birim, Sira = g.Alanlar.Count + 1 };
            g.Alanlar.Add(a);
            return a;
        }

        private static void Kaynak(KontrolBolumTanimi b, string kod, string baslik, string tasinmazAlani,
            KontrolVeriKaynagiTipi kaynak = KontrolVeriKaynagiTipi.TasinmazSaltOkunur, KontrolAlanTipi tip = KontrolAlanTipi.KisaMetin)
        {
            var a = Alan(b, kod, baslik, tip);
            a.VeriKaynagi = kaynak;
            a.TasinmazAlani = tasinmazAlani;
        }

        private static void Secim(KontrolBolumTanimi b, string kod, string baslik, bool coklu, params string[] secenekler)
            => Secenekleri(Alan(b, kod, baslik, coklu ? KontrolAlanTipi.CokluSecim : KontrolAlanTipi.TekliSecim), secenekler);

        private static void Secim(KontrolGrupTanimi g, string kod, string baslik, bool coklu, params string[] secenekler)
            => Secenekleri(Alan(g, kod, baslik, coklu ? KontrolAlanTipi.CokluSecim : KontrolAlanTipi.TekliSecim), secenekler);

        private static void Secenekleri(KontrolAlanTanimi a, string[] secenekler)
        {
            int i = 0;
            foreach (var s in secenekler)
            {
                var p = s.Split('|', 2);
                a.Secenekler.Add(new KontrolSecenekTanimi { Kod = p[0], Etiket = p.Length > 1 ? p[1] : p[0], Sira = ++i });
            }
        }

        private static KontrolGrupTanimi Grup(KontrolBolumTanimi b, string kod, string baslik, bool tekrar)
        {
            var g = new KontrolGrupTanimi { Kod = kod, Baslik = baslik, Tekrarlanabilir = tekrar, Sira = b.Gruplar.Count + 1 };
            b.Gruplar.Add(g);
            return g;
        }
    }
}
