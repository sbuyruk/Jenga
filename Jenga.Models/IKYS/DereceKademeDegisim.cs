using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;

public class DereceKademeDegisim : BaseModel
{
    [ValidateNever]
    public int? PersonelId { get; set; }
    [ForeignKey("PersonelId")]
    [ValidateNever]
    public Personel? Personel { get; set; }
    [DisplayName("Değişim")]
    public string? Degisim { get; set; }
    [DisplayName("Değişim Tarihi")]
    public DateTime? DegisimTarihi { get; set; }
    [DisplayName("Derece")]
    public int? Derece { get; set; }
    [DisplayName("Kademe")]
    public int? Kademe { get; set; }
}
