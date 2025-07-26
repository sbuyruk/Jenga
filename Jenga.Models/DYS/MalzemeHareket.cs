using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.DYS
{
    public class MalzemeHareket : BaseModel
    {
        [DisplayName("Malzeme")]
        [ForeignKey("MalzemeId")]
        [Required]
        public int MalzemeId { get; set; }
        [ValidateNever]
        public Malzeme Malzeme { get; set; }
        [DisplayName("Malzemenin Geldiği Yer")]
        [ForeignKey("KaynakYeriId")]
        [ValidateNever]
        public int KaynakYeriId { get; set; }
        [ValidateNever]
        public MalzemeYeriTanim KaynakYeri { get; set; }
        [ValidateNever]
        [ForeignKey("HedefYeriId")]
        [DisplayName("Malzemenin Gittiği Yer")]
        public int HedefYeriId { get; set; }
        [ValidateNever]
        public MalzemeYeriTanim HedefYeri { get; set; }
        [DisplayName("Adet")]
        [Required]
        [Range(1, 1000, ErrorMessage = "Adet 1 ile 1000 arasında olmalı.")]
        public int Adet { get; set; }
        [DisplayName("Giriş/Çıkış")]
        [ValidateNever]
        public string? GirisCikis { get; set; }
        [DisplayName("İşlem Tarihi")]
        [ValidateNever]
        public DateTime IslemTarihi { get; set; }
        [DisplayName("İşlem")]
        [ValidateNever]
        public string? IslemTipi { get; set; }

    }
}
