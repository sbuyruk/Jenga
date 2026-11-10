namespace Jenga.Models.TBYS.Kontrol
{
    public enum KontrolDurumu
    {
        Taslak = 0,
        Tamamlandi = 1,
        IptalEdildi = 2,
        /// <summary>Yerine gerekçeli yeni bir kontrol (revizyon) geçmiştir; içerik salt okunur korunur.</summary>
        RevizeEdildi = 3
    }

    public enum KontrolAlanTipi
    {
        KisaMetin = 0,
        UzunMetin = 1,
        TamSayi = 2,
        OndalikSayi = 3,
        Tarih = 4,
        EvetHayir = 5,
        TekliSecim = 6,
        CokluSecim = 7,
        Ek = 8,
        /// <summary>Taşınmazın bölgesindeki personelden seçim; cevap "Adı Soyadı" metni olarak saklanır.</summary>
        BolgePersoneli = 9
    }

    /// <summary>Alanın cevabı nereden gelir.</summary>
    public enum KontrolVeriKaynagiTipi
    {
        Manuel = 0,
        /// <summary>Taşınmazdan okunur, değiştirilemez.</summary>
        TasinmazSaltOkunur = 1,
        /// <summary>Taşınmazdan başlangıç değeri alınır, kullanıcı düzenleyebilir.</summary>
        TasinmazBaslangic = 2
    }

    public enum PeriyotBirimi
    {
        Gun = 0,
        Hafta = 1,
        Ay = 2,
        Yil = 3
    }

    /// <summary>Evet/Hayır alanı için üç anlamlı değer; boş cevap ayrıca null ile temsil edilir.</summary>
    public enum EvetHayirCevabi
    {
        Hayir = 0,
        Evet = 1,
        Uygulanamaz = 2
    }
}
