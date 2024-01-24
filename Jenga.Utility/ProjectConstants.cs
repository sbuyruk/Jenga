
namespace Jenga.Utility
{
    public static class ProjectConstants
    {
        //ORTAK
        public const int SIFIR = 0;
        public static DateTime ILK_TARIH = new DateTime(1900,1,1);

        //MTS
        public const string MTS_ANIOBJESISTOKLU= "Stoklu";
        public const string MTS_ANIOBJESISTOKSUZ = "Stoksuz";
        public const string MTS_GIRIS = "Giriş";
        public const string MTS_CIKIS = "Çıkış";
        public const int RANDEVU_KATILIMCI_IC_INT = 1;
        public const int RANDEVU_KATILIMCI_DIS_INT = 2;
        public const int RANDEVU_KATILIMCI_NAKITBAGISCI_INT = 3;
        public const int RANDEVU_KATILIMCI_TASINMAZBAGISCI_INT = 4;
        public const string RANDEVU_KATILIMCI_IC = "Vakıf Personeli";
        public const string RANDEVU_KATILIMCI_DIS = "Vakıf Dışı";
        public const string RANDEVU_KATILIMCI_NAKITBAGISCI = "Nakit Bağışçı";
        public const string RANDEVU_KATILIMCI_TASINMAZBAGISCI="Taşınmaz Bağışçı";
        public const string MTSGOREVDURUMU_GOREVDE="Görevde";
        public const string MTSGOREVDURUMU_AYRILDI="Ayrıldı";
        public const string MTSAYRILMASEBEBI_BOS = "";
        public const string MTSAYRILMASEBEBI_ATAMA = "Atama";
        public const string MTSAYRILMASEBEBI_EMEKLILIK = "Emeklilik";
        public const string MTSAYRILMASEBEBI_ISTIFA = "İstifa";
    }
}
