using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;

public class YabanciDil : BaseModel
{
    [ValidateNever]
    public int? PersonelId { get; set; }
    [ForeignKey("PersonelId")]
    [ValidateNever]
    public Personel? Personel { get; set; }
    [DisplayName("Dil")]
    public string? Dil { get; set; }
    [DisplayName("Sınav Adı")]
    public string? SinavAdi { get; set; }
    [DisplayName("Sınav Notu")]
    public string? SinavNotu { get; set; }
    [DisplayName("Sınav Tarihi")]
    public DateTime? SinavTarihi { get; set; }
}
