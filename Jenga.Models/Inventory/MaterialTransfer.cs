using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Inventory
{
    [Table("MaterialTransfer_Table")]
    public class MaterialTransfer : BaseModel
    {
        public int MaterialId { get; set; }
        public int Quantity { get; set; }
        public int MaterialUnitId { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public int? FromPersonId { get; set; }
        public int? ToPersonId { get; set; }
        public DateTime TransferDate { get; set; } = DateTime.Now;
    }
}