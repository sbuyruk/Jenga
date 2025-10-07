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
        public int LocationId { get; set; }
        // Navigation properties (opsiyonel)
        // public Material? Material { get; set; }
        // public Location? Location { get; set; }
        // public MaterialUnit? MaterialUnit { get; set; }
    }
}