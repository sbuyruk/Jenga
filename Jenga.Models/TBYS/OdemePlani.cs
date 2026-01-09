using System;
using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS
{
    [Table("OdemePlani_Table")]
    public class OdemePlani : BaseModel
    {
        [Column("SozlesmeId")]
        public int? SozlesmeId { get; set; }

        [Column("Yil")]
        public int? Yil { get; set; }

        [Column("Ay")]
        public string? Ay { get; set; }

        [Column("VadeBasTar", TypeName = "date")]
        public DateTime? VadeBasTar { get; set; }

        [Column("KiraBedeli", TypeName = "money")]
        public decimal? KiraBedeli { get; set; }

        [Column("OdenenTutar", TypeName = "money")]
        public decimal? OdenenTutar { get; set; }

        [Column("AnaPara", TypeName = "money")]
        public decimal? AnaPara { get; set; }

        [Column("FaizliBakiye", TypeName = "money")]
        public decimal? FaizliBakiye { get; set; }

        [Column("FaizOrani", TypeName = "decimal(7,4)")]
        public decimal? FaizOrani { get; set; }

        [Column("FaizTutari", TypeName = "money")]
        public decimal? FaizTutari { get; set; }

        [Column("OdemeBasTar", TypeName = "date")]
        public DateTime? OdemeBasTar { get; set; }

        [Column("OdemeBitTar", TypeName = "date")]
        public DateTime? OdemeBitTar { get; set; }

        [Column("Aciklama")]
        public string? Aciklama { get; set; }

        [Column("Sira")]
        public int? Sira { get; set; }

        [Column("VadeBitTar", TypeName = "date")]
        public DateTime? VadeBitTar { get; set; }
    }
}
