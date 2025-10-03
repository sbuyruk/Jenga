using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Common
{

    [Table("MenuItem_Table")]
    public class MenuItem : BaseModel
    {
        [Column("Adi")]
        public string? Title { get; set; }="Menu Başlığı";

        [Column("UstMenuId")]
        public int? ParentId { get; set; }

        [Column("Url")]
        public string? Url { get; set; } = "#";

        [Column("Sira")]
        public int? DisplayOrder { get; set; }

        [Column("IsVisible")]
        public bool? IsVisible { get; set; } = true;

        [NotMapped]
        public List<MenuItem>? Children { get; set; } = [];//initialized
        [NotMapped]
        public bool IsExpanded { get; set; }
        [NotMapped]
        public bool IsActive { get; set; }  

    }

}