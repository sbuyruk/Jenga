using Jenga.Models.Sistem;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS
{
    [Table("Bagis_Table")]
    public class Bagis : BaseModel
    {
        [Column("BagisciId")]
        public int? BagisciId { get; set; }

        [Column("TasinmazId")]
        public int? TasinmazId { get; set; }

        [Column("BagisYili")]
        public int? BagisYili { get; set; }

        [Column("BagisTarihi")]
        public DateTime? BagisTarihi { get; set; }

        [Column("Envanterde")]
        public bool Envanterde { get; set; }

        [Column("ArmaganId")]
        public string? ArmaganId { get; set; }

        [Column("ArmaganDurumu")]
        public string? ArmaganDurumu { get; set; }

        [Column("ArmaganTarihi")]
        public DateTime? ArmaganTarihi { get; set; }

        [Column("ArmaganAciklama")]
        public string? ArmaganAciklama { get; set; }

    }
}
