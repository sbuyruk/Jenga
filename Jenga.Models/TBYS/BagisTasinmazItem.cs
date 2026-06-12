namespace Jenga.Models.TBYS
{
    /// <summary>
    /// Taşınmaz bağış sayısı için ArmaganDashboard'un ihtiyaç duyduğu minimal DTO.
    /// </summary>
    public sealed class BagisTasinmazItem
    {
        public int? BagisciId { get; set; }
        public DateTime? ArmaganTarihi { get; set; }
    }
}
