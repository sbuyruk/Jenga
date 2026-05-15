using System;
using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS
{
    [Table("SozlesmeTasinmaz_Table")]
    public class SozlesmeTasinmaz : BaseModel
    {
        [NotMapped]
        public new string? Aciklama { get; set; }
        [Column("SozlesmeId")]
        public int? SozlesmeId { get; set; }

        [Column("TasinmazId")]
        public int? TasinmazId { get; set; }

        [Column("BolumId")]
        public int? BolumId { get; set; }

        [Column("KiralamaAmaci")]
        public string? KiralamaAmaci { get; set; }
    }
}
