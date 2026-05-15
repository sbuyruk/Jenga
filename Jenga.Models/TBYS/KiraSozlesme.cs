using System;
using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS
{
    [Table("KiraSozlesme_Table")]
    public class KiraSozlesme : BaseModel
    {
        [Column("KiraciId")]
        public int? KiraciId { get; set; }

        [Column("IlkSozlesmeTar", TypeName = "date")]
        public DateTime? IlkSozlesmeTar { get; set; }

        [Column("SozBasTar", TypeName = "date")]
        public DateTime? SozBasTar { get; set; }

        [Column("SozBitTar", TypeName = "date")]
        public DateTime? SozBitTar { get; set; }

        [Column("KiraBedeli", TypeName = "money")]
        public decimal? KiraBedeli { get; set; }

        [Column("OdemeSekli")]
        public string? OdemeSekli { get; set; }

        [Column("ArtisAyi")]
        public string? ArtisAyi { get; set; }

        [Column("TasinmazAdedi")]
        public int? TasinmazAdedi { get; set; }

        [Column("KiraciDurumu")]
        public int? KiraciDurumu { get; set; }

        [Column("KefilAdiSoyadi")]
        public string? KefilAdiSoyadi { get; set; }

        [Column("KefilTCKimlikNo")]
        public string? KefilTCKimlikNo { get; set; }

        [Column("KefilAdresi")]
        public string? KefilAdresi { get; set; }

        [Column("KefilTel")]
        public string? KefilTel { get; set; }

        [Column("TeminatCinsi")]
        public string? TeminatCinsi { get; set; }

        [Column("TeminatTutari", TypeName = "money")]
        public decimal? TeminatTutari { get; set; }

        [Column("TeminatOdemeTarihi", TypeName = "date")]
        public DateTime? TeminatOdemeTarihi { get; set; }

        [Column("TeminatIadeTarihi", TypeName = "date")]
        public DateTime? TeminatIadeTarihi { get; set; }

        [Column("TeminatAciklama", TypeName = "nvarchar(max)")]
        public string? TeminatAciklama { get; set; }

        [Column("TaksitSayisi")]
        public int? TaksitSayisi { get; set; }

        [Column("Aktif")]
        public bool? Aktif { get; set; }

        [Column("DosyaNo")]
        public int? DosyaNo { get; set; }

        [Column("DevirAnaPara", TypeName = "money")]
        public decimal? DevirAnaPara { get; set; }

        [Column("DevirFaizTutari", TypeName = "money")]
        public decimal? DevirFaizTutari { get; set; }

        [Column("DevirFaizliBakiye", TypeName = "money")]
        public decimal? DevirFaizliBakiye { get; set; }

        [Column("SozlesmeDurumu")]
        public string? SozlesmeDurumu { get; set; }

        [Column("DurumDegismeTar", TypeName = "datetime")]
        public DateTime? DurumDegismeTar { get; set; }

        [Column("GecikmeZammiTipi")]
        public string? GecikmeZammiTipi { get; set; }

        [Column("OdenenTeminatTutari", TypeName = "money")]
        public decimal? OdenenTeminatTutari { get; set; }

        [Column("KalanTeminatTutari", TypeName = "money")]
        public decimal? KalanTeminatTutari { get; set; }

        [Column("IadeTeminatTutari", TypeName = "money")]
        public decimal? IadeTeminatTutari { get; set; }

        [Column("SozlesmePDFDosyasi")]
        public string? SozlesmePDFDosyasi { get; set; }

        [Column("BolgeId")]
        public int? BolgeId { get; set; }

        [ForeignKey("BolgeId")]
        public Common.Bolge? Bolge { get; set; }
    }
}
