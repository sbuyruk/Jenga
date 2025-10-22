using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("MaterialInventory_Table")]
    public class MaterialInventory : BaseModel
    {
        [Column("MaterialId")]
        public int MaterialId { get; set; }

        [Column("LocationId")]
        public int? LocationId { get; set; }
        [Column("MaterialUnitId")]
        public int MaterialUnitId { get; set; }
        [Column("Quantity")]
        public int Quantity { get; set; }

        // Opsiyonel navigation property
        // public Material? Material { get; set; }
        // public Location? Location { get; set; }
        // public MaterialUnit? MaterialUnit { get; set; }
    }
}

