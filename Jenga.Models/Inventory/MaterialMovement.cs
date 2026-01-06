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

        [Column("MaterialUnitId")]
        public int? MaterialUnitId { get; set; }

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

        // New: Brand/Model at movement level (nullable)
        [Column("BrandId")]
        public int? BrandId { get; set; }

        [Column("ModelId")]
        public int? ModelId { get; set; }
       
        [Column("Operation")]
        public string? Operation { get; set; }
        // Optional navigation properties
        // public Material? Material { get; set; }
        // public Location? FromLocation { get; set; }
        // public Location? ToLocation { get; set; }
        // public Person? FromPerson { get; set; }
        // public Person? ToPerson { get; set; }
        // public MaterialUnit? MaterialUnit { get; set; }

        //public MaterialBrand? Brand { get; set; }
        //public MaterialModel? Model { get; set; }
    }
}