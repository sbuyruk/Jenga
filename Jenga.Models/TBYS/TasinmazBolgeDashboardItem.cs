using System;

namespace Jenga.Models.TBYS
{
    /// <summary>
    /// BolgeDashboard projeksiyonu için hafif taşınmaz veri taşıma nesnesi.
    /// Sadece dashboard'un ihtiyaç duyduğu alanları içerir.
    /// </summary>
    public sealed class TasinmazBolgeDashboardItem
    {
        public string? Ili { get; set; }
        public string? SorumluBolge { get; set; }
        public string? MulkiyetSekli { get; set; }
        public string? KirayaUygunluk { get; set; }
        public string? KullanimSekli { get; set; }
        public decimal? MuhasebeyeKayitliDeger { get; set; }
        public decimal? TahminiRayicDegeri { get; set; }
        public decimal? EmlakBeyanDegeri { get; set; }
        public decimal? YaklasikPiyasaDegeri { get; set; }

        public bool TamMulkiyetMi =>
            !string.IsNullOrEmpty(MulkiyetSekli) &&
            MulkiyetSekli.Equals(Tasinmaz.TamMulkiyetDeger, StringComparison.OrdinalIgnoreCase);

        public bool CiplakMulkiyetMi =>
            !string.IsNullOrEmpty(MulkiyetSekli) &&
            MulkiyetSekli.Equals(Tasinmaz.CiplakMulkiyetDeger, StringComparison.OrdinalIgnoreCase);

        public bool KirayaUygunMu =>
            !string.IsNullOrEmpty(KirayaUygunluk) &&
            KirayaUygunluk.Equals(Tasinmaz.KirayaUygunDeger, StringComparison.OrdinalIgnoreCase);
    }
}
