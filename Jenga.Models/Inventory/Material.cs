using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("Material_Table")]
    public class Material : BaseModel
    {
        [Column("CategoryId")]
        public int CategoryId { get; set; }

        [Column("BrandId")]
        public int? BrandId { get; set; }

        [Column("ModelId")]
        public int? ModelId { get; set; }

        [Column("MaterialName")]
        public string MaterialName { get; set; } = string.Empty;

        [Column("IsAsset")]
        public bool IsAsset { get; set; }

        [Column("Unit")]
        public string? Unit { get; set; }


        // Navigation properties (isteğe bağlı, ekleyebilirsin)
        // public MaterialCategory? Category { get; set; }
        // public MaterialBrand? Brand { get; set; }
        // public MaterialModel? Model { get; set; }
    }
}