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
        public string? LocationType { get; set; }
    }
}