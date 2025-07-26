using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.IKYS;
public class UnvanTanim : BaseModel
{
    [Required]
    [DisplayName("Görev Id")]
    public short? GorTipId { get; set; }
    [Required]
    [DisplayName("Ünvan")]
    public string? Adi { get; set; }
    [ValidateNever]
    [DisplayName("Ünvan Kısaltması")]
    public string? KisaAdi { get; set; }

}
