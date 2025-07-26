using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.NBYS
{
    public class NakitBagisHareket : BaseModel
    {
        [ValidateNever]
        [DisplayName("Bağış Tarihi")]
        public DateTime? BagisTarihi { get; set; }
        [Required]
        [DisplayName("Bağışçı Id")]
        public int BagisciId { get; set; }
        [ValidateNever]
        [DisplayName("Bağış Mikarı")]
        public decimal? BagisMiktari { get; set; } = 0;
        [ValidateNever]
        [DisplayName("Döviz Cinsi")]
        public string? DovizCinsi { get; set; } = "TL";
        [ValidateNever]
        [DisplayName("Banka Id")]
        public int BankaId { get; set; }
        [ValidateNever]
        [DisplayName("İli")]
        public int? Ili { get; set; }
        [ValidateNever]
        [DisplayName("İlçesi")]
        public int? Ilcesi { get; set; }
        [ValidateNever]
        [DisplayName("Adresi")]
        public string? Adresi { get; set; }
        [ValidateNever]
        [DisplayName("Telefon")]
        public string? Telefon { get; set; }
        [ValidateNever]
        [DisplayName("Armağan Id")]
        public int ArmaganId { get; set; } = 0;
        [ValidateNever]
        [DisplayName("İade Edildi mi")]
        public bool IadeEdildiMi { get; set; } = false;
        [ValidateNever]
        [DisplayName("İade Miktarı")]
        public decimal? IadeMiktari { get; set; } = 0;
        [ValidateNever]
        [DisplayName("İade Tarihi")]
        public DateTime? IadeTarihi { get; set; }
        [ValidateNever]
        [DisplayName("İade Sebebi")]
        public string? IadeSebebi { get; set; }
        [ValidateNever]
        [DisplayName("İade Eden")]
        public string? IadeEden { get; set; }
        [ValidateNever]
        [DisplayName("Döviz Tutarı")]
        public decimal? DovizTutari { get; set; } = 0;
        [ValidateNever]
        [DisplayName("Döviz Kuru")]
        public decimal? DovizKuru { get; set; } = 0;
        [ValidateNever]
        [DisplayName("Kur Tarihi")]
        public DateTime? KurTarihi { get; set; }

    }
}
