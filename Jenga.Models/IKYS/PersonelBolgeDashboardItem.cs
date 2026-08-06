namespace Jenga.Models.IKYS
{
    /// <summary>
    /// BolgeDashboard projeksiyonu için hafif personel veri taşıma nesnesi.
    /// </summary>
    public sealed class PersonelBolgeDashboardItem
    {
        public int PersonelId { get; set; }
        public string? Adi { get; set; }
        public string? Soyadi { get; set; }
        public int SicilNo { get; set; }
        public string? UnvanAdi { get; set; }
        public string? BirimAdi { get; set; }
        public string? CalismaDurumu { get; set; }
        public int? ProtokolSiraNo { get; set; }
    }
}
