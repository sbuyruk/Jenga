using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.IKYS;
public class Personel : BaseModel
{
    [Required]
    [DisplayName("Adı")]
    public string Adi { get; set; }
    [Required]
    [DisplayName("Soyadı")]
    public string Soyadi { get; set; }
    [Required]
    [DisplayName("Sicil No")]
    public int SicilNo { get; set; }
    [ValidateNever]
    [DisplayName("Tahsili")]
    public int? Tahsili { get; set; }
    [ValidateNever]
    [DisplayName("Kullanıcı Adı")]
    public string? KullaniciAdi { get; set; }
    [ValidateNever]
    [DisplayName("Asker/Sivil")]
    public string? Asker_sivil { get; set; }
}
