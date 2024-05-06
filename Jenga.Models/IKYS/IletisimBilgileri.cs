using Jenga.Models.MTS;
using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;
public class IletisimBilgileri : BaseModel
{
    [ValidateNever]
    public int PersonelId { get; set; }
    [ForeignKey("PersonelId")]
    [ValidateNever]
    public Personel? Personel { get; set; }
    [ValidateNever]
    [DisplayName("TAdres")]
    public string? Adres { get; set; }
    [ValidateNever]
    [DisplayName("Semt")]
    public string? Semt { get; set; }
    [ValidateNever]
    [DisplayName("İli")]
    public int? Ili { get; set; }
    [ValidateNever]
    [DisplayName("Ilcesi")]
    public int? Ilcesi { get; set; }
    [ValidateNever]
    [DisplayName("Posta Kodu")]
    public string? PostaKodu { get; set; }
    [ValidateNever]
    [DisplayName("Dahili")]
    public string? DahiliTelefonu { get; set; }
    [ValidateNever]
    [DisplayName("Ev Telefonu")]
    public string? EvTelefonu { get; set; }
    [ValidateNever]
    [DisplayName("Cep Telefonu1")]
    public string? CepTelefonu { get; set; }
    [ValidateNever]
    [DisplayName("Cep Telefonu2")]
    public string? CepTelefonu2 { get; set; }
    [ValidateNever]
    [DisplayName("Intranet E-Posta")]
    public string? IntranetEPosta { get; set; }
    [ValidateNever]
    [DisplayName("İnternet E-Posta")]
    public string? InternetEPosta { get; set; }
    [ValidateNever]
    [DisplayName("Özel E-Posta")]
    public string? OzelEPosta { get; set; }

}
