using System;
using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS
{
    [Table("KiraSozlesme_Table")]
    public class KiraSozlesme : BaseModel
    {
        [Column("SozlesmeNo")]
        public string? SozlesmeNo { get; set; }

        [Column("KiraciId")]
        public int? KiraciId { get; set; }

        [Column("TasinmazId")]
        public int? TasinmazId { get; set; }

        [Column("BaslangicTarihi")]
        public DateTime? BaslangicTarihi { get; set; }

        [Column("BitisTarihi")]
        public DateTime? BitisTarihi { get; set; }

        [Column("KiraUcreti", TypeName = "money")]
        public decimal? KiraUcreti { get; set; }

        [Column("Depozito", TypeName = "money")]
        public decimal? Depozito { get; set; }

        [Column("KiraPeriyodu")]
        public string? KiraPeriyodu { get; set; }

        [Column("Notlar")]
        public string? Notlar { get; set; }

        [Column("Aktif")]
        public bool? Aktif { get; set; }
    }
}
