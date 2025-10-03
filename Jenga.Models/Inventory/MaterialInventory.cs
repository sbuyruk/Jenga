using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("MaterialInventory_Table")]
    public class MaterialInventory : BaseModel
    {
        [Column("MaterialId")]
        public int MaterialId { get; set; }

        [Column("Quantity")]
        public int Quantity { get; set; }

        [Column("EntryDate")]
        public DateTime EntryDate { get; set; }

        [Column("EntryType")]
        public string? EntryType { get; set; }

        [Column("Source")]
        public string? Source { get; set; }

        [Column("LocationId")]
        public int LocationId { get; set; }
    }
}