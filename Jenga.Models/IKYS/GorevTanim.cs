using Jenga.Models.Sistem;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
