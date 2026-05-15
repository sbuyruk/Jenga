namespace Jenga.Models.Search
{
    /// <summary>
    /// Hangi varlık türünün sonuç olduğunu belirtir.
    /// Yeni modüller buraya eklenerek sisteme dahil edilir.
    /// </summary>
    public enum SearchResultTipi
    {
        // TBYS
        Tasinmaz,
        Kiraci,
        TasinmazBagisci,

        // NBYS
        NakitBagisci,

        // FTK  — bir sonraki adımda aktif edilecek
        FtkKisi,

        // IKYS — bir sonraki adımda aktif edilecek
        Personel,
    }

    /// <summary>
    /// Hangi modüle ait olduğunu belirtir; UI'da rozet ve renk için kullanılır.
    /// </summary>
    public enum SearchModul
    {
        TBYS,
        NBYS,
        FTK,
        IKYS,
    }

    /// <summary>
    /// Tek bir arama sonucu kaydı.
    /// </summary>
    public class SearchResultItem
    {
        public int Id { get; set; }
        public SearchResultTipi Tipi { get; set; }
        public SearchModul Modul { get; set; }

        /// <summary>Ana başlık — genellikle Ad Soyad veya sicil/kimlik bilgisi.</summary>
        public string Baslik { get; set; } = string.Empty;

        /// <summary>İkinci satır — il/ilçe, birim vb.</summary>
        public string AltBaslik { get; set; } = string.Empty;

        /// <summary>Üçüncü satır — meslek, kiralama amacı vb. opsiyonel.</summary>
        public string? EkBilgi { get; set; }

        /// <summary>Tıklandığında gidilecek URL — servis katmanı üretir.</summary>
        public string HedefUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Bir modüle ait gruplandırılmış sonuç listesi.
    /// </summary>
    public class SearchGrubu
    {
        public SearchModul Modul { get; set; }
        public string GrupBasligi { get; set; } = string.Empty;
        public string IkonCss { get; set; } = string.Empty;
        public string RenkCss { get; set; } = string.Empty;
        public List<SearchResultItem> Sonuclar { get; set; } = [];
    }

    /// <summary>
    /// Tüm modüllerin gruplandırılmış arama sonucu.
    /// </summary>
    public class GlobalSearchSonucu
    {
        public List<SearchGrubu> Gruplar { get; set; } = [];
        public int ToplamSonuc => Gruplar.Sum(g => g.Sonuclar.Count);
        public bool HerhangiBirSonuc => ToplamSonuc > 0;
    }
}
