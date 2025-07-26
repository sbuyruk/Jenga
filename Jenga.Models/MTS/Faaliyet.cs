using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.MTS;

public class Faaliyet : BaseModel
{

    [ValidateNever]
    [DisplayName("Unique Id")]
    public Guid? UniqueId { get; set; }
    [Required]
    [DisplayName("Faaliyet Tipi")]
    public string FaaliyetTipi { get; set; }
    [Required]
    [DisplayName("Faaliyet Amacı Id")]
    [ForeignKey("FaaliyetAmaciId")]
    public int FaaliyetAmaciId { get; set; }
    [ValidateNever]
    public FaaliyetAmaci FaaliyetAmaci { get; set; }
    [Required]
    [DisplayName("Faaliyet Konusu")]
    public string FaaliyetKonusu { get; set; }
    [ValidateNever]
    [DisplayName("Faaliyet Yeri")]
    public string FaaliyetYeriStr { get; set; }
    [Required]
    [DisplayName("Faaliyet Durumu")]
    public int FaaliyetDurumu { get; set; }
    [ValidateNever]
    [DisplayName("Tüm Gün")]
    public bool? TumGun { get; set; }
    [ValidateNever]
    [DisplayName("Açık Tarihli")]
    public bool? AcikTarih { get; set; }
    [ValidateNever]
    [DisplayName("İç İrtibat Id")]
    public int? IcIrtibatId { get; set; }
    [ValidateNever]
    [DisplayName("Dış İrtibat Id")]
    public int? DisIrtibatId { get; set; }
    [ValidateNever]
    [DisplayName("Başlangıç Tarihi")]
    public DateTime? BaslangicTarihi { get; set; }
    [ValidateNever]
    [DisplayName("Bitiş Tarihi")]
    public DateTime? BitisTarihi { get; set; }
    [ValidateNever]
    [DisplayName("Başlangıç Saati")]
    public string? BaslangicSaati { get; set; }
    [ValidateNever]
    [DisplayName("Bitiş Saati")]
    public string? BitisSaati { get; set; }
    [ValidateNever]
    [DisplayName("Yönetici Notu")]
    public string? YoneticiNotu { get; set; }
    [ValidateNever]
    [DisplayName("Özel Kalem Takvimine İşlensin")]
    public bool? TakvimeIslendi { get; set; }
    public virtual List<FaaliyetKatilim>? FaaliyetKatilims { get; set; }

}
