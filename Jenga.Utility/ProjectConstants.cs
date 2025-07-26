
namespace Jenga.Utility
{
    public static class ProjectConstants
    {
        //ORTAK
        public const int SIFIR = 0;
        public static DateTime ILK_TARIH = new DateTime(1900, 1, 1);
        public const int GENELMUDUR_PERSONELID = 1184;

        public static string BAGISCI_SAG = "Sağ";
        public static string BAGISCI_VEFAT = "Vefat";
        public static string BAGISCI_BILINMIYOR = "Bilinmiyor";
        public static string BAGISCI_MULGA = "Mülga";
        public static string BAGISCI_KURULUS = "Kuruluş";
        public static string BAGISCI_TASFIYE = "Tasfiye";
        public static string BAGISCI_LAGV = "Lağv";

        public static string SOSYALGUVENCE_YOK = "Yok";
        public static string SOSYALGUVENCE_SGK = "SGK";
        public static string SOSYALGUVENCE_EMEKLISANDIGI = "Emekli Sandığı";
        public static string SOSYALGUVENCE_SSK = "SSK";
        public static string SOSYALGUVENCE_BAGKUR = "Bağkur";
        //IKYS
        public static int PER_CALISIYOR_INT = 1;
        public static string PER_CALISIYOR = "Çalışıyor";
        public static int PER_AYRILDI_INT = 0;
        public static string PER_AYRILDI = "Ayrıldı";
        //MTS
        public const string MTS_ANIOBJESISTOKLU = "Stoklu";
        public const string MTS_ANIOBJESISTOKSUZ = "Stoksuz";
        public const string MTS_GIRIS = "Giriş";
        public const string MTS_CIKIS = "Çıkış";
        public const int FAALIYET_KATILIMCI_IC_INT = 1;
        public const int FAALIYET_KATILIMCI_DIS_INT = 2;
        public const int FAALIYET_KATILIMCI_NAKITBAGISCI_INT = 3;
        public const int FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT = 4;
        public const int FAALIYET_KATILIMCI_FTK_INT = 5;
        public const string FAALIYET_KATILIMCI_IC = "Vakıf Personeli";
        public const string FAALIYET_KATILIMCI_IC_ESKI = "Vakıf Personeli (Eski)";
        public const string FAALIYET_KATILIMCI_DIS = "Vakıf Dışı";
        public const string FAALIYET_KATILIMCI_NAKITBAGISCI = "Nakit Bağışçı";
        public const string FAALIYET_KATILIMCI_TASINMAZBAGISCI = "Taşınmaz Bağışçı";
        public const string FAALIYET_KATILIMCI_FTK = "Fahri Tanıtım Kurulu";
        public const string MTSGOREVDURUMU_GOREVDE = "Görevde";
        public const string MTSGOREVDURUMU_AYRILDI = "Ayrıldı";
        public const string MTSAYRILMASEBEBI_BOS = "";
        public const string MTSAYRILMASEBEBI_ATAMA = "Atama";
        public const string MTSAYRILMASEBEBI_EMEKLILIK = "Emeklilik";
        public const string MTSAYRILMASEBEBI_ISTIFA = "İstifa";
        //Unvan
        public const int MTSUNVAN_NAKITBAGISCI_INT = 29;
        public const int MTSUNVAN_TASINMAZBAGISCI_INT = 30;
        public const string MTSUNVAN_NAKITBAGISCI = "Nakit Bağışçı";
        public const string MTSUNVAN_TASINMAZBAGISCI = "Taşınmaz Bağışçı";

        public static string ARAMAGORUSME_GELENTELEFON = "Gelen Telefon Araması";
        public static string ARAMAGORUSME_GIDENTELEFON = "Giden Telefon Araması";
        public static string ARAMAGORUSME_YUZYUZEGORUSME = "Yüzyüze Görüşme";
        public static string ARAMAGORUSME_YONETICIDIREKTIFI = "Yönetici Direktifi";

        public const string FAALIYET_AMACI_TOPLANTI = "Toplantı";
        public const string FAALIYET_AMACI_ZIYARET = "Ziyaret";
        public const string FAALIYET_AMACI_DAVET = "Davet";
        public const string FAALIYET_AMACI_YILDONUMU = "Yildönümü";
        public const string FAALIYET_AMACI_DOGUMGUNU = "Doğum Günü";
        public const string FAALIYET_AMACI_OZELCALISMA = "Özel Çalışma";
        public const string FAALIYET_AMACI_IZIN = "İzin";
        public const string FAALIYET_AMACI_RESMITATIL = "Resmi Tatil";
        public const string FAALIYET_AMACI_SEYAHAT = "Seyahat";
        public const string FAALIYET_AMACI_GORUSME = "Görüşme";
        public const string FAALIYET_AMACI_BILGI = "Bilgi";
        public const string FAALIYET_AMACI_VAKIF_TOPLANISI = "Vakıf Toplantısı";

        public const string FAALIYET_AMACI_TOPLANTI_CLASS = "toplanti";
        public const string FAALIYET_AMACI_ZIYARET_CLASS = "ziyaret";
        public const string FAALIYET_AMACI_DAVET_CLASS = "davet";
        public const string FAALIYET_AMACI_YILDONUMU_CLASS = "yildonumu";
        public const string FAALIYET_AMACI_DOGUMGUNU_CLASS = "dogumGunu";
        public const string FAALIYET_AMACI_OZELCALISMA_CLASS = "ozelCalisma";
        public const string FAALIYET_AMACI_IZIN_CLASS = "izin";
        public const string FAALIYET_AMACI_RESMITATIL_CLASS = "resmiTatil";
        public const string FAALIYET_AMACI_SEYAHAT_CLASS = "seyahat";
        public const string FAALIYET_AMACI_GORUSME_CLASS = "gorusme";
        public const string FAALIYET_AMACI_BILGI_CLASS = "bilgi";
        public const string FAALIYET_AMACI_VAKIF_TOPLANISI_CLASS = "vakifToplantisi";

        public const string FAALIYET_AMACI_TOPLANTI_INT = "1";
        public const string FAALIYET_AMACI_ZIYARET_INT = "2";
        public const string FAALIYET_AMACI_DAVET_INT = "3";
        public const string FAALIYET_AMACI_YILDONUMU_INT = "4";
        public const string FAALIYET_AMACI_DOGUMGUNU_INT = "5";
        public const string FAALIYET_AMACI_OZELCALISMA_INT = "6";
        public const string FAALIYET_AMACI_IZIN_INT = "7";
        public const string FAALIYET_AMACI_RESMITATIL_INT = "8";
        public const string FAALIYET_AMACI_GORUSME_INT = "9";
        public const string FAALIYET_AMACI_SEYAHAT_INT = "10";
        public const string FAALIYET_AMACI_BILGI_INT = "11";
        public const string FAALIYET_AMACI_VAKIF_TOPLANISI_INT = "12";

        public const string FAALIYET_YERI_GMMAKAMI = "G.M. Makamı";
        public const string FAALIYET_YERI_ZEHRAURGA = "Zehra Urga";
        public const string FAALIYET_YERI_ILHANBASAR = "İlhan Başar";
        public const string FAALIYET_YERI_MAKAMTOPLANTIODASI = "Makam Toplantı Odası";
        public const string FAALIYET_YERI_ASELSAN = "ASELSAN";
        public const string FAALIYET_YERI_TUSAS = "TUSAŞ";
        public const string FAALIYET_YERI_HAVELSAN = "HAVELSAN";
        public const string FAALIYET_YERI_ROKETSAN = "ROKETSAN";
        public const string FAALIYET_YERI_ASPILSAN = "ASPILSAN";
        public const string FAALIYET_YERI_ISBIR = "ISBIR";
        public const string FAALIYET_YERI_DIGER = "Diğer";
        public static string RANDEVU_VERILEN = "Verilen Randevu";
        public static string RANDEVU_ALINAN = "Alınan Randevu";

        public static string FAALIYET_DURUMU_PLANLANDI = "Planlandı";
        public static string FAALIYET_DURUMU_ONAYLANDI = "Onaylandı";
        public static string FAALIYET_DURUMU_IPTALEDILDI = "İptal Edildi";
        public const int FAALIYET_DURUMU_PLANLANDI_INT = 1;
        public const int FAALIYET_DURUMU_ONAYLANDI_INT = 2;
        public const int FAALIYET_DURUMU_IPTALEDILDI_INT = 3;
        public static string FAALIYET_DURUMU_PLANLANDI_CLASS = "planlandi";
        public static string FAALIYET_DURUMU_ONAYLANDI_CLASS = "onaylandi";
        public static string FAALIYET_DURUMU_IPTALEDILDI_CLASS = "iptalEdildi";

        //Bolgeler
        public const string BOLGE_ANKARA = "Ankara";
        public const string BOLGE_ISTANBUL = "İstanbul";
        public const string BOLGE_IZMIR = "İzmir";
        public const string BOLGE_MERSIN = "Mersin";
        public const string BOLGE_YURTDISI = "Yurtdışı";
        public const string BOLGE_HEPSI = "Hepsi";
        public const string BOLGE_GENELMUDURLUK = "Genel Md.lük";
        public const string BOLGE_TEMSILCILIGI = "Bölge Temsilciliği";

        public const int BOLGE_ANKARA_INT = 1;
        public const int BOLGE_ISTANBUL_INT = 2;
        public const int BOLGE_IZMIR_INT = 3;
        public const int BOLGE_MERSIN_INT = 4;
        public const int BOLGE_HEPSI_INT = 5;
        public const int BOLGE_GENELMUDURLUK_INT = 8;
        //Malzeme
        public const string MALZEMEHAREKETI_GIRIS = "Giriş";
        public const string MALZEMEHAREKETI_CIKIS = "Çıkış";
        public const string MALZEMEHAREKETI_AKTARMA = "Aktarma";

        public const int MALZEMENINGELDIGI_YER_BOS_INT = 1;
        public const int MALZEMENINGITTIGI_YER_BOS_INT = 1;
    }
}
