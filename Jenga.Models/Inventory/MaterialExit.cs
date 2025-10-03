using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("MaterialExit_Table")]
    public class MaterialExit : BaseModel
    {
        [Column("MaterialId")]
        public int MaterialId { get; set; }

        [Column("Quantity")]
        public int Quantity { get; set; }

        [Column("ExitDate")]
        public DateTime ExitDate { get; set; }

        [Column("ExitType")]
        public string? ExitType { get; set; }

        [Column("Reason")]
        public string? Reason { get; set; }
    }
}