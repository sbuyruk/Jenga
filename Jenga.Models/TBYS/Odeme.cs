using System;
using System.ComponentModel.DataAnnotations.Schema;
using Jenga.Models.Sistem;

namespace Jenga.Models.TBYS
{
    [Table("Odeme_Table")]
    public class Odeme : BaseModel
    {
        [Column("SozlesmeId")]
        public int? SozlesmeId { get; set; }

        [Column("KiraciId")]
        public int? KiraciId { get; set; }

        [Column("OdemeTarihi")]
        public DateTime? OdemeTarihi { get; set; }

        [Column("OdenenTutar", TypeName = "money")]
        public decimal? OdenenTutar { get; set; }

        [Column("OdemePlaniId")]
        public int? OdemePlaniId { get; set; }

        [ForeignKey("KiraciId")]
        public Kiraci? Kiraci { get; set; }
    }
}
