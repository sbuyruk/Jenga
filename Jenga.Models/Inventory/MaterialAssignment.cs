using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("MaterialAssignment_Table")]
    public class MaterialAssignment : BaseModel
    {
        [Column("MaterialId")]
        public int MaterialId { get; set; }

        [Column("PersonId")]
        public int PersonId { get; set; }

        [Column("Quantity")]
        public int Quantity { get; set; }

        [Column("AssignmentDate")]
        public DateTime AssignmentDate { get; set; }

        [Column("AssignmentType")]
        public string? AssignmentType { get; set; }

        [Column("LocationId")]
        public int LocationId { get; set; }
    }
}