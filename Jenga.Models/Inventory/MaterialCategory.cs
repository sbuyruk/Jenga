using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("MaterialCategory_Table")]
    public class MaterialCategory : BaseModel
    {
        [Column("ParentCategoryId")]
        public int? ParentCategoryId { get; set; }

        [Column("CategoryName")]
        public string CategoryName { get; set; } = string.Empty;
    }
}