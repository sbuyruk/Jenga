using Jenga.Models.Sistem;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Jenga.Models.IKYS;
public class UnvanTanim: BaseModel
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
