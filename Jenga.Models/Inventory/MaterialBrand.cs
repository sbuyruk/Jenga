using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("MaterialBrand_Table")]
    public class MaterialBrand : BaseModel
    {
        public string BrandName { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        // Navigation property (opsiyonel)
        // public MaterialCategory? Category { get; set; }
    }
}