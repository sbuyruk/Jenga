using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS
{
    [Table("TasinmazBagisci_Table")]
    public class TasinmazBagisci : BaseModel
    {
        /// <summary>
        /// Gizli==true ise hassas kişisel alanları (Ad, Soyad, TCKimlikNo, Adres, Telefon, Foto) maskeler.
        /// Servis katmanından döndürülmeden önce çağrılmalıdır.
        /// </summary>
        public TasinmazBagisci Maskele()
        {
            if (Gizli != true) return this;

            const string gizli = "***";
            Adi       = gizli;
            Soyadi    = gizli;
            TCKimlikNo = null;
            Adres     = gizli;
            Telefon1  = gizli;
            Telefon2  = gizli;
            Foto      = null;
            return this;
        }

        [Column("Adi")]
        public string Adi { get; set; } = string.Empty;

        [Column("Soyadi")]
        public string? Soyadi { get; set; } = string.Empty;

        [Column("TCKimlikNo")]
        public long? TCKimlikNo { get; set; }

        [Column("DogumYeri")]
        public string? DogumYeri { get; set; } = string.Empty;

        [Column("DogumTarihi")]
        public DateTime? DogumTarihi { get; set; }

        [Column("Meslegi")]
        public string? Meslegi { get; set; } = string.Empty;

        [Column("SosyalGuvence")]
        public string? SosyalGuvence { get; set; } = string.Empty;

        [Column("Ili")]
        public string? Ili { get; set; } = string.Empty;

        [Column("Ilcesi")]
        public string? Ilcesi { get; set; } = string.Empty;

        [Column("Adres")]
        public string? Adres { get; set; } = string.Empty;

        [Column("Telefon1")]
        public string? Telefon1 { get; set; } = string.Empty;

        [Column("Telefon2")]
        public string? Telefon2 { get; set; } = string.Empty;

        [Column("Foto")]
        public string? Foto { get; set; } = string.Empty;

        [Column("Sag_vefat")]
        public string? Sag_vefat { get; set; } = string.Empty;

        [Column("VefatTarihi")]
        public DateTime? VefatTarihi { get; set; }

        [Column("DergiGonderilmesin")]
        public bool? DergiGonderilmesin { get; set; }

        [Column("Gizli")]
        public bool? Gizli { get; set; }

        [Column("DefinYeri")]
        public string? DefinYeri { get; set; } = string.Empty;

        [Column("DefinIli")]
        public string? DefinIli { get; set; } = string.Empty;

        [Column("DefinIlcesi")]
        public string? DefinIlcesi { get; set; } = string.Empty;

        [Column("DefinAciklama")]
        public string? DefinAciklama { get; set; } = string.Empty;

        [Column("EPosta")]
        public string? EPosta { get; set; } = string.Empty;

        [Column("IlId")]
        public int? IlId { get; set; }

        [Column("IlceId")]
        public int? IlceId { get; set; }

        [Column("Tahsil")]
        public string? Tahsil { get; set; } = string.Empty;
    }
}
