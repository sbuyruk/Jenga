using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS
{
    [Table("BagisciTalepleri_Table")]
    public class BagisciTalepleri : BaseModel
    {
        [Column("Talep")]
        public string? Talep { get; set; }

        [Column("Irtibat")]
        public string? Irtibat { get; set; }

        [Column("Tarih")]
        public string? Tarih { get; set; }

        [Column("BagisciId")]
        public long? BagisciId { get; set; }
    }
}
