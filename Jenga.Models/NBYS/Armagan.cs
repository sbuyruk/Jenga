using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Models.NBYS
{
    [Table("Armagan_Table")]
    public class Armagan : BaseModel
    {
        [Column("BagisciId")]
        [DisplayName("Bağışçı Id")]
        public int? BagisciId { get; set; }

        [Column("ArmaganTanimId")]
        [DisplayName("Armağan Tanım Id")]
        public int? ArmaganTanimId { get; set; }

        [Column("Tarih", TypeName = "date")]
        [DisplayName("Tarih")]
        public DateTime? Tarih { get; set; }

        [Column("Durum")]
        [DisplayName("Durum")]
        public string? Durum { get; set; }

        // BaseModel already contains: OlusturmaTarihi, Olusturan, DegistirmeTarihi, Degistiren, Aciklama

        [Column("BagisMiktari", TypeName = "money")]
        [DisplayName("Bağış Miktarı")]
        public decimal? BagisMiktari { get; set; }

        [Column("DovizCinsi")]
        [DisplayName("Döviz Cinsi")]
        public string? DovizCinsi { get; set; }

        [Column("BelgedeYazanIsim")]
        [DisplayName("Belgede Yazan İsim")]
        public string? BelgedeYazanIsim { get; set; }

        [Column("BelgeGecersizMi")]
        [DisplayName("Belge Geçersiz Mi")]
        public bool? BelgeGecersizMi { get; set; }

        [Column("GecersizNBHareketId")]
        [DisplayName("Geçersiz NB Hareket Id")]
        public int? GecersizNBHareketId { get; set; }

        [Column("GecersizYapan")]
        [DisplayName("Geçersiz Yapan")]
        public string? GecersizYapan { get; set; }

        [Column("GecersizYapmaTarihi")]
        [DisplayName("Geçersiz Yapma Tarihi")]
        public DateTime? GecersizYapmaTarihi { get; set; }

        [Column("IadeMiktari", TypeName = "money")]
        [DisplayName("İade Miktarı")]
        public decimal? IadeMiktari { get; set; }

        [Column("ArmaganBagisMiktari", TypeName = "money")]
        [DisplayName("Armağan Bağış Miktarı")]
        public decimal? ArmaganBagisMiktari { get; set; }

        [Column("BagisMiktariYazmasin")]
        [DisplayName("Bağış Miktarı Yazmasın")]
        public bool? BagisMiktariYazmasin { get; set; } = false;

        [Column("CokluBagis")]
        [DisplayName("Çoklu Bağış")]
        public bool? CokluBagis { get; set; } = false;

        [Column("DuzenliBagis")]
        [DisplayName("Düzenli Bağış")]
        public bool? DuzenliBagis { get; set; }
    }
}
