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

        [Column("MaterialUnitId")]
        public int MaterialUnitId { get; set; }

        [Column("CriticalStock")]
        public int? CriticalStock { get; set; } = 0;
    }
}