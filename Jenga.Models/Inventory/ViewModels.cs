namespace Jenga.Models.Inventory
{
    public class MaterialLocationCard
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string UnitName { get; set; } = string.Empty;

        // personel bilgileri (nullable)
        public int? PersonelId { get; set; }
        public string? PersonelName { get; set; }

        // YENİ: Bu kartı temsil eden (gruptaki) bir MaterialInventory satırının Id'si
        // StartTransfer'da modal'a initial selection gönderirken kullanacağız.
        public int InventoryId { get; set; } = 0;
    }

    public class MaterialTransactionView
    {
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string Type { get; set; } = "Giriş";
        public bool IsIncrease { get; set; } = true;

        // Raw movement/exit type from the movement record (e.g. Exit.ExitType)
        public string MovementTypeRaw { get; set; } = string.Empty;
    }
}
