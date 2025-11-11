namespace Jenga.Models.Inventory
{
    public class MaterialLocationCard
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string UnitName { get; set; } = string.Empty;
    }

    public class MaterialTransactionView
    {
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string Type { get; set; } = "Giriş";
        public bool IsIncrease { get; set; } = true;
    }
}
