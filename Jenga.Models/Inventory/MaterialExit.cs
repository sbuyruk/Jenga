using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;
using Jenga.Models.Enums;

namespace Jenga.Models.Inventory
{
    [Table("MaterialExit_Table")]
    public class MaterialExit : BaseModel
    {
        [Column("MaterialId")]
        public int MaterialId { get; set; }

        [Column("Quantity")]
        public int Quantity { get; set; }
        [Column("MaterialUnitId")]
        public int MaterialUnitId { get; set; }
        [Column("LocationId")]
        public int LocationId { get; set; }
        [Column("PersonId")]
        public int? PersonId { get; set; }
        [Column("ExitDate")]
        public DateTime ExitDate { get; set; }

        // Stored in the database as an integer corresponding to MaterialExitType
        [Column("ExitType")]
        public int? ExitType { get; set; }

        // Helper to get enum name (not mapped to DB)
        [NotMapped]
        public string? ExitTypeName => ExitType.HasValue ? Enum.GetName(typeof(MaterialExitType), ExitType.Value) : null;

    }
}
