using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;
public class IsBilgileri : BaseModel
{
    [Required]
    public int PersonelId { get; set; }
    [DisplayName("Personel")]
    [ForeignKey("PersonelId")]
    [ValidateNever]
    public Personel Personel { get; set; }
    
    //[Required]
    public int? UnvanId { get; set; }
    [DisplayName("Ünvan")]
    [ForeignKey("UnvanId")]
    [ValidateNever]
    public UnvanTanim?  UnvanTanim { get; set; }
    //[Required]
    public int? GorevId { get; set; }
    [DisplayName("Görev")]
    [ForeignKey("GorevId")]
    [ValidateNever]
    public GorevTanim? GorevTanim { get; set; }
    //[Required]
    public int? BirimId { get; set; }
    [DisplayName("Birim")]
    [ForeignKey("BirimId")]
    public BirimTanim BirimTanim { get; set; }
    //[Required]
    [DisplayName("Başlama Tarihi")]
    public DateTime? BaslamaTar { get; set; }
    [Required]
    [DisplayName("Çalışma Durumu")]
    public string? CalismaDurumu { get; set; }
    [ValidateNever]
    [DisplayName("Ayrılma Tarihi")]
    public DateTime? AyrilmaTar { get; set; }
    [ValidateNever]
    [DisplayName("Ayrilma Sebebi")]
    public string? AyrilmaSebebi { get; set; }
    //[Required]
    [DisplayName("SGK Sicil No")]
    public string? SGKSicilNo { get; set; }
    [ValidateNever]
    [DisplayName("SGK Başlama Tarihi")]
    public DateTime? SGKBasTar { get; set; }
    [ValidateNever]
    [DisplayName("Vakıf Öncesi Prim GünSayısı")]
    public int VakifOncesiPrimGunSayisi { get; set; }
    [ValidateNever]
    [DisplayName("Emeklilik Tarihi")] 
    public DateTime? EmeklilikTarihi { get; set; }
    //[Required]
    [DisplayName("İzin Dönemi Başlama Tarihi")]
    public DateTime? IzinDonemiBasTar { get; set; }
}
