using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.IKYS;

public class GorevTanim : BaseModel
{
    [Required]
    [DisplayName("Birim")]
    public long? BirimId { get; set; }
    [Required]
    [DisplayName("Görev")]
    public string? Adi { get; set; }
    [ValidateNever]
    [DisplayName("Görev Kısaltması")]
    public string? KisaAdi { get; set; }
    [ValidateNever]
    [DisplayName("Personel")]
    public int? PersonelId { get; set; }
    [ValidateNever]
    [DisplayName("Vekil mi")]
    public bool? Vekil { get; set; }
    [ValidateNever]
    [DisplayName("Aktif")]
    public bool? Aktif { get; set; }
}
