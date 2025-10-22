using Jenga.Models.Enums;
using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("Location_Table")]
    
    public class Location : BaseModel
    {
        [Column("LocationName")]
        public string LocationName { get; set; } = string.Empty;

        [Column("LocationType")]
        public LocationType LocationType { get; set; }

        [Column("ParentId")]
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public Location? Parent { get; set; }
        public List<Location> Children { get; set; } = new();

        // Bu property ile sorunuz çözülebilir:
        [Column("IsStoragePlace")]
        public bool IsStoragePlace { get; set; }

    }
}