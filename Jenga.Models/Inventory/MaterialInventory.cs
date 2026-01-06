using Jenga.Models.IKYS;
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
        
        [Column("PersonelId")]
        public int? PersonelId { get; set; }

        [Column("Quantity")]
        public int Quantity { get; set; }

        // New: brand/model at inventory level (nullable)
        [Column("BrandId")]
        public int? BrandId { get; set; }

        [Column("ModelId")]
        public int? ModelId { get; set; }

        // Optional navigation properties
        public Material? Material { get; set; }
        public Location? Location { get; set; }
        public Personel? Personel { get; set; }

        public MaterialBrand? Brand { get; set; }
        public MaterialModel? Model { get; set; }
    }
}

