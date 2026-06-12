namespace Jenga.Models.NBYS
{
    /// <summary>
    /// ArmaganDashboard "Son Armağanlar" tablosu için hafif bağışçı DTO.
    /// </summary>
    public sealed class NakitBagisciArmaganItem
    {
        public int Id { get; set; }
        public string? Adi { get; set; }
        public string? Soyadi { get; set; }
        public int? Ili { get; set; }
    }
}
