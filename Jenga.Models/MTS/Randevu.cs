using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.MTS;
public class Randevu : BaseModel
{
    [Required]
    [DisplayName("Faaliyet Tipi")]
    public string RandevuTipi { get; set; }
    [Required]
    [DisplayName("Faaliyet Yeri")]
    public int RandevuYeri { get; set; }
    [Required]
    [DisplayName("Faaliyet Konusu")]
    public string RandevuKonusu { get; set; }
    [Required]
    [DisplayName("Faaliyet Amacı")]
    public int RandevuAmaci { get; set; }
    [Required]
    [DisplayName("Faaliyet Amacı")]
    public string? RandevuDurumu { get; set; }
    [ValidateNever]
    [DisplayName("Tüm Gün")]
    public bool TumGun { get; set; }
    [ValidateNever]
    [DisplayName("Açık tarihli")]
    public bool AcikTarih { get; set; }
    [ValidateNever]
    [DisplayName("İç İrtibat")]
    public int IcIrtibatId { get; set; }
    [ValidateNever]
    [DisplayName("Dış İrtibat")]
    public int DisIrtibatId { get; set; }
    [ValidateNever]
    [DisplayName("Başlangıç Tarihi")]
    public DateTime BaslangicTarihi { get; set; }
    [ValidateNever]
    [DisplayName("Bitiş Tarihi")]
    public DateTime BitisTarihi { get; set; }
    [ValidateNever]
    [DisplayName("Başlangıç Saati")]
    public string? BaslangicSaati { get; set; }
    [ValidateNever]
    [DisplayName("Bitiş Saati")]
    public string? BitisSaati { get; set; }
}
