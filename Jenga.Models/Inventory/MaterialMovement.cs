using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("MaterialMovement_Table")]
    public class MaterialMovement : BaseModel
    {
        [Column("MaterialId")]
        public int MaterialId { get; set; }

        [Column("Quantity")]
        public int Quantity { get; set; }

        [Column("FromLocationId")]
        public int? FromLocationId { get; set; }

        [Column("ToLocationId")]
        public int? ToLocationId { get; set; }

        [Column("FromPersonId")]
        public int? FromPersonId { get; set; }

        [Column("ToPersonId")]
        public int? ToPersonId { get; set; }

        [Column("MovementDate")]
        public DateTime MovementDate { get; set; }

        [Column("MovementType")]
        public string? MovementType { get; set; }
    }
}