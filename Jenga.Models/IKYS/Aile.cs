using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;

public class Aile : BaseModel
{
    [ValidateNever]
    public int? PersonelId { get; set; }
    [ForeignKey("PersonelId")]
    [ValidateNever]
    public Personel? Personel { get; set; }
    [DisplayName("Adı")]
    public string? Adi { get; set; }
    [DisplayName("Soyadı")]
    public string? Soyadi { get; set; }
    [DisplayName("TC Kimlik No")]
    public string? TcKimlikNo { get; set; }
    [DisplayName("Yakınlık Derecesi")]
    public short? YakinlikDerecesi { get; set; }
    [DisplayName("Doğum Tarihi")]
    public DateTime? DogumTar { get; set; }
    [DisplayName("Tahsil")]
    public short? Tahsil { get; set; }
    [DisplayName("Okul")]
    public string? Okul { get; set; }
    [DisplayName("Telefon")]
    public string? Telefon { get; set; }
    [DisplayName("Meslek")]
    public short? Meslek { get; set; }
}
