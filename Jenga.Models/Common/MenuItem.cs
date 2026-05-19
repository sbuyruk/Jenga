using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Common
{

    [Table("Auth_MenuItem_Table")]
    public class MenuItem : BaseModel
    {
        [Column("Title")]
        public string? Title { get; set; } = "Menu Başlığı";

        [Column("ParentId")]
        public int? ParentId { get; set; }

        [Column("Url")]
        public string? Url { get; set; } = "#";

        [Column("DisplayOrder")]
        public int? DisplayOrder { get; set; }

        [Column("IsVisible")]
        public bool? IsVisible { get; set; } = true;

        [NotMapped]
        public List<MenuItem>? Children { get; set; } = new List<MenuItem>(); // fixed initialization
        [NotMapped]
        public bool IsExpanded { get; set; }
        [NotMapped]
        public bool IsActive { get; set; }

    }

}