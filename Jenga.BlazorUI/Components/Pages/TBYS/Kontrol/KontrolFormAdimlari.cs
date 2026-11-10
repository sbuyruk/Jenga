using Jenga.Models.TBYS.Kontrol;

namespace Jenga.BlazorUI.Components.Pages.TBYS.Kontrol;

/// <summary>Sunum grupları; kayıtlı formun alanlarını, kodlarını veya sırasını değiştirmez.</summary>
public sealed record KontrolFormAdimi(string Baslik, string Aciklama, string Ikon, IReadOnlyList<KontrolBolumTanimi> Bolumler);

public static class KontrolFormAdimlari
{
    public static List<KontrolFormAdimi> Olustur(KontrolFormTanimi tanim)
    {
        var gruplar = new (string Baslik, string Aciklama, string Ikon, string[] Kodlar)[]
        {
            ("Taşınmaz ve rapor", "Kontrolün temel bilgilerini ve raporu hazırlayan kişileri belirleyin.", "bi-building", ["ana_tasinmaz", "bagimsiz_bolum"]),
            ("Belgeler ve imar", "İncelenen belgeleri ve güncel imar bilgilerini kaydedin.", "bi-file-earmark-text", ["belgeler", "imar"]),
            ("Yapı ve donatılar", "Bina, bağımsız bölüm ve sosyal donatıların özelliklerini değerlendirin.", "bi-houses", ["bina", "bb_ozellik", "site"]),
            ("Çevre ve emsaller", "Konumu, yakın çevreyi ve karşılaştırılabilir taşınmazları kaydedin.", "bi-geo-alt", ["konum", "yakin_cevre", "emsal", "bolgesel"]),
            ("Kullanım ve kira", "Kullanım, kira, irtibat, bağışçı ve takyidat bilgilerini inceleyin.", "bi-key", ["takyidat", "irtibat", "bagisci", "sozlesme"]),
            ("Fotoğraf ve ekler", "Gözlemlerinizi destekleyen fotoğraf, kroki ve belgeleri ekleyin.", "bi-images", ["kroki", "foto"]),
            ("Değerlendirme", "Genel kanaatinizi, önerilerinizi ve kontrol sıklığını kaydedin.", "bi-clipboard-check", ["siklik", "kanaat"])
        };
        var adimlar = new List<KontrolFormAdimi>();
        var bilinenKodlar = gruplar.SelectMany(g => g.Kodlar).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var g in gruplar)
        {
            var bolumler = tanim.Bolumler.Where(b => g.Kodlar.Contains(b.Kod, StringComparer.OrdinalIgnoreCase)).OrderBy(b => b.Sira).ToList();
            // Rapor bilgileri, eski özel raporlarda da ilk adımda kalır.
            if (bolumler.Count > 0 || adimlar.Count == 0)
                adimlar.Add(new(g.Baslik, g.Aciklama, g.Ikon, bolumler));
        }
        // Geçmiş raporlardaki ek bölümler de kendi adımlarıyla gösterilir.
        foreach (var b in tanim.Bolumler.Where(b => !bilinenKodlar.Contains(b.Kod)).OrderBy(b => b.Sira))
            adimlar.Add(new(b.Baslik, b.Aciklama ?? "Bu bölüme ait kontrol bilgilerini kaydedin.", "bi-ui-checks", [b]));
        return adimlar;
    }

    public static string AlanGenisligi(KontrolAlanTanimi alan) => alan.Tip switch
    {
        _ when alan.Kod is "konum_ulasim" or "konum_cevre" => "kontrol-alan-tam kontrol-konum-metin",
        _ when alan.Kod == "takyidat_aciklama" => "kontrol-alan-tam kontrol-takyidat-aciklama",
        KontrolAlanTipi.UzunMetin or KontrolAlanTipi.Ek => "kontrol-alan-tam",
        _ when alan.Kod == "takyidat_tur" => "kontrol-alan-normal kontrol-takyidat-tur",
        _ when alan.Kod == "takyidat_tarih" => "kontrol-alan-normal kontrol-takyidat-tarih",
        _ when alan.Kod is "konum_gelisme" or "konum_koordinat" => "kontrol-alan-normal kontrol-konum-kisa",
        _ when alan.Kod is "bina_yapi_tarzi" or "bina_dis_cephe" => "kontrol-alan-normal kontrol-bina-secim",
        _ when alan.Kod is "bina_asansor" or "bina_otopark" => "kontrol-alan-kisa kontrol-bina-evethayir",
        _ when alan.Kod is "bbo_oda" or "bbo_salon" => "kontrol-alan-mini kontrol-bb-sayi",
        _ when alan.Kod == "bbo_balkon" => "kontrol-alan-kisa kontrol-bb-kisa",
        _ when alan.Kod is "bbo_cephe" or "bbo_durum" => "kontrol-alan-normal kontrol-bb-kisa",
        _ when alan.Kod == "bbo_zemin" => "kontrol-alan-normal kontrol-bb-zemin",
        _ when alan.Kod is "site_icinde" or "site_aidat" => "kontrol-alan-kisa kontrol-site-kisa",
        _ when alan.Kod == "site_donati" => "kontrol-alan-genis kontrol-site-donati",
        _ when alan.Kod == "tesis_tur" => "kontrol-alan-normal kontrol-tesis-tur",
        _ when alan.Kod == "tesis_ad" => "kontrol-alan-normal kontrol-tesis-ad",
        _ when alan.Kod == "irtibat_ad" => "kontrol-alan-normal kontrol-irtibat-ad",
        _ when alan.Kod is "irtibat_sifat" or "irtibat_tel" => "kontrol-alan-normal kontrol-irtibat-diger",
        _ when alan.Kod is "emsal_tur" or "emsal_kaynak" => "kontrol-alan-normal kontrol-emsal-normal",
        _ when alan.Kod == "emsal_adres" => "kontrol-alan-genis kontrol-emsal-adres",
        _ when alan.Kod is "emsal_alan" or "emsal_uzaklik" => "kontrol-alan-kisa kontrol-emsal-dar",
        _ when alan.Kod is "emsal_bedel" or "emsal_birim" => "kontrol-alan-kisa kontrol-emsal-normal",
        KontrolAlanTipi.TamSayi => "kontrol-alan-mini",
        _ when alan.Kod is "bina_insa_yili" or "bina_kat_sayisi" or "imar_hmax"
            || alan.TasinmazAlani is "InsaYili" or "ToplamKatSayisi" or "BulunduguKat" or "BagimsizBolumNo" => "kontrol-alan-mini",
        KontrolAlanTipi.CokluSecim or KontrolAlanTipi.BolgePersoneli => "kontrol-alan-genis",
        _ when alan.TasinmazAlani == "Adres" || alan.Kod.EndsWith("_adres", StringComparison.OrdinalIgnoreCase) => "kontrol-alan-genis",
        KontrolAlanTipi.OndalikSayi or KontrolAlanTipi.EvetHayir => "kontrol-alan-kisa",
        _ => "kontrol-alan-normal"
    };
}
