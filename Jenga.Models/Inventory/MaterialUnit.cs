using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("MaterialUnit_Table")]
    public class MaterialUnit : BaseModel
    {
        public string Name { get; set; } = string.Empty;  // "Kilogram", "Adet", "Litre"
        public string? Symbol { get; set; }               // "kg", "adet", "lt"
    }
}