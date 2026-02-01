using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Jenga.Models.NBYS
{
    [Table("NakitBagisHareket_Table")]
    public class NakitBagisHareket : BaseModel
    {
        [Column("BagisTarihi", TypeName = "date")]
        [DisplayName("Bağış Tarihi")]
        public DateTime? BagisTarihi { get; set; }

        [Column("BagisMiktari", TypeName = "money")]
        [DisplayName("Bağış Miktarı")]
        public decimal? BagisMiktari { get; set; }

        [Column("DovizCinsi")]
        [DisplayName("Döviz Cinsi")]
        public string? DovizCinsi { get; set; }

        [Column("BankaId")]
        [DisplayName("Banka Id")]
        public int? BankaId { get; set; }

        [Column("Ili")]
        [DisplayName("İli")]
        public int? Ili { get; set; }

        [Column("Ilcesi")]
        [DisplayName("İlçesi")]
        public int? Ilcesi { get; set; }

        [Column("Adresi")]
        [DisplayName("Adresi")]
        public string? Adresi { get; set; }

        [Column("Telefon")]
        [DisplayName("Telefon")]
        public string? Telefon { get; set; }

        [Column("BagisciId")]
        [DisplayName("Bağışçı Id")]
        public int? BagisciId { get; set; }

        [Column("ArmaganId")]
        [DisplayName("Armağan Id")]
        public int? ArmaganId { get; set; }

        [Column("IadeEdildiMi")]
        [DisplayName("İade Edildi Mi")]
        public bool? IadeEdildiMi { get; set; }

        [Column("IadeMiktari", TypeName = "money")]
        [DisplayName("İade Miktarı")]
        public decimal? IadeMiktari { get; set; }

        [Column("IadeTarihi")]
        [DisplayName("İade Tarihi")]
        public DateTime? IadeTarihi { get; set; }

        [Column("IadeSebebi")]
        [DisplayName("İade Sebebi")]
        public string? IadeSebebi { get; set; }

        [Column("IadeEden")]
        [DisplayName("İade Eden")]
        public string? IadeEden { get; set; }

        [Column("DovizTutari", TypeName = "money")]
        [DisplayName("Döviz Tutarı")]
        public decimal? DovizTutari { get; set; }

        [Column("DovizKuru", TypeName = "money")]
        [DisplayName("Döviz Kuru")]
        public decimal? DovizKuru { get; set; }

        [Column("KurTarihi", TypeName = "date")]
        [DisplayName("Kur Tarihi")]
        public DateTime? KurTarihi { get; set; }

        [Column("EkstreAktarmaId")]
        [DisplayName("Ekstre Aktarma Id")]
        public int? EkstreAktarmaId { get; set; }

        [Column("BagisTipi")]
        [DisplayName("Bağış Tipi")]
        public string? BagisTipi { get; set; }
    }
}
