using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public virtual List<MTSKurumGorev>? MTSKurumGorevs { get; set; }
    //public int? MTSKurumTanimId { get; set; }
    //[DisplayName("MTSKurum Id")]
    //[ForeignKey("MTSKurumTanimId")]
    //[ValidateNever]
    //public MTSKurumTanim? MTSKurumTanim { get; set; }

    //public int? MTSGorevTanimId { get; set; }
    //[DisplayName("MTSGorev Id")]
    //[ForeignKey("MTSGorevTanimId")]
    //[ValidateNever]
    //public MTSGorevTanim? MTSGorevTanim { get; set; }

    public int? MTSUnvanTanimId { get; set; }
    [DisplayName("MTSUnvan Id")]
    [ForeignKey("MTSUnvanTanimId")]
    [ValidateNever]
    public MTSUnvanTanim? MTSUnvanTanim { get; set; }

    [ValidateNever]
    [DisplayName("Kurumu")]
    public string? Kurumu { get; set; }
    [ValidateNever]
    [DisplayName("Görevi")]
    public string? Gorevi { get; set; }
    [ValidateNever]
    [DisplayName("Ünvanı")]
    public string? Unvani { get; set; }
    [ValidateNever]
    [DisplayName("EPosta")]
    public string? EPosta { get; set; }
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
