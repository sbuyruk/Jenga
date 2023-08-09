using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.MTS;
public class Kisi : BaseModel
{
    [Required]
    [DisplayName("Adı")]
    public string? Adi { get; set; }
    [Required]
    [DisplayName("Soyadı")]
    public string? Soyadi { get; set; }
    [ValidateNever]
    [DisplayName("TC Kimlik No")]
    public long? TCKimlikNo { get; set; }
    [ValidateNever]
    [DisplayName("Kurumu")]
    public string? Kurumu { get; set; }
    [ValidateNever]
    [DisplayName("Ünvanı")]
    public string? Unvani { get; set; }
    [ValidateNever]
    [DisplayName("Görevi")]
    public string? Gorevi { get; set; }
    [ValidateNever]
    [DisplayName("Telefon 1")]
    public string? Telefon1 { get; set; }
    [ValidateNever]
    [DisplayName("Telefon 2")]
    public string? Telefon2 { get; set; }
    [ValidateNever]
    [DisplayName("Telefon 3")]
    public string? Telefon3 { get; set; }
    [ValidateNever]
    [DisplayName("Açıklama 1")]
    public string? TelAciklama1 { get; set; }
    [ValidateNever]
    [DisplayName("Açıklama 2")]
    public string? TelAciklama2 { get; set; }
    [ValidateNever]
    [DisplayName("Açıklama 3")]
    public string? TelAciklama3 { get; set; }
    [ValidateNever]
    [DisplayName("Adres")]
    public string? Adres { get; set; }
    [ValidateNever]
    [DisplayName("İl")]
    public int? Ili { get; set; }
    [ValidateNever]
    [DisplayName("İlçe")]
    public int? Ilcesi { get; set; }
    [ValidateNever]
    [DisplayName("Dahili Telefon 1")]
    public string? Dahili1 { get; set; }
    [ValidateNever]
    [DisplayName("Dahili Telefon 2")]
    public string? Dahili2 { get; set; }
    [ValidateNever]
    [DisplayName("Dahili Telefon 3")]
    public string? Dahili3 { get; set; }
    [ValidateNever]
    [DisplayName("Doğum Tarihi")]
    public DateTime? DogumTarihi { get; set; }
    [ValidateNever]
    [DisplayName("Kutlama")]
    public bool? Kutlama { get; set; }=false;
}
