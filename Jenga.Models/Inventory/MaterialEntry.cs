using Jenga.Models.IKYS;
using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("MaterialEntry_Table")]
    public class MaterialEntry : BaseModel
    {
        public int MaterialId { get; set; }
        public int Quantity { get; set; }
        public int MaterialUnitId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime EntryDate { get; set; }
        public int? LocationId { get; set; }
        public int? PersonelId { get; set; }

        [Column("BrandId")]
        public int? BrandId { get; set; }

        [Column("ModelId")]
        public int? ModelId { get; set; }

        // Navigation properties (optional)
        public Material? Material { get; set; }
        public Location? Location { get; set; }
        public Personel? Personel { get; set; }

        // Navigation to brand/model
        public MaterialBrand? Brand { get; set; }
        public MaterialModel? Model { get; set; }
    }
}