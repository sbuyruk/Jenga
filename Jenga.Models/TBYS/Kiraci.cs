using System;
using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS
{
    [Table("Kiraci_Table")]
    public class Kiraci : BaseModel
    {
        [NotMapped]
        public new string? Aciklama { get; set; }
        [Column("Adi")]
        public string? Adi { get; set; }

        [Column("Soyadi")]
        public string? Soyadi { get; set; }

        [Column("TCKimlikNo")]
        public long? TCKimlikNo { get; set; }

        [Column("VergiDairesi")]
        public string? VergiDairesi { get; set; }

        [Column("VergiNo")]
        public string? VergiNo { get; set; }

        [Column("Ili")]
        public string? Ili { get; set; }

        [Column("Ilcesi")]
        public string? Ilcesi { get; set; }

        [Column("Semt")]
        public string? Semt { get; set; }

        [Column("Adres")]
        public string? Adres { get; set; }

        [Column("Telefon")]
        public string? Telefon { get; set; }

        [Column("Eposta")]
        public string? Eposta { get; set; }

        [Column("KiralamaAmaci")]
        public string? KiralamaAmaci { get; set; }

        [Column("IlId")]
        public int? IlId { get; set; }

        [Column("IlceId")]
        public int? IlceId { get; set; }
    }
}
