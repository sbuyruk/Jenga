using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("MaterialModel_Table")]
    public class MaterialModel : BaseModel
    {
        [Column("BrandId")]
        public int BrandId { get; set; }

        [Column("ModelName")]
        public string ModelName { get; set; } = string.Empty;
    }
}