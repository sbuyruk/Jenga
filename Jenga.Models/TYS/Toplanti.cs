using Jenga.Models.MTS;
using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.TYS;

public class Toplanti : BaseModel
{
    [ValidateNever]
    [DisplayName("Unique Id")]
    public Guid? UniqueId { get; set; }
    [Required]
    [DisplayName("Toplantı Konusu")]
    public string ToplantiKonusu { get; set; }
    [Required]
    [DisplayName("Toplantı Yeri")]
    public int ToplantiYeri { get; set; }
    [ValidateNever]
    [DisplayName("Toplantı Yeri Diğer")]
    public string? ToplantiYeriDiger { get; set; }
    [ValidateNever]
    [DisplayName("Toplantı Yetkilisi")]
    public int ToplantiYetkilisi { get; set; }
    [Required]
    [DisplayName("Koordinatör")]
    public int Koordinator { get; set; }
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
    [DisplayName("Dış Katılımcılar")]
    public string? DisKatilimcilar { get; set; }
    [ValidateNever]
    [DisplayName("Çevrim İçi")]
    public bool? CevrimIci { get; set; }
    [ValidateNever]
    [DisplayName("İkram")]
    public bool? IkramOnayi { get; set; }
    [ValidateNever]
    [DisplayName("İkram Malzemesi")]
    public string? IkramMalzemesi { get; set; }
    public virtual List<ToplantiKatilim>? ToplantiKatilims { get; set; }
}
