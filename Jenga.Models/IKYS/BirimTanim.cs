using Jenga.Models.Sistem;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Jenga.Models.IKYS;

public class BirimTanim: BaseModel
{
    [Required]
    [DisplayName("Birim Adı")]
    public string? Adi { get; set; }
    [Required]
    [DisplayName("Birim Kısaltması")]
    public string? KisaAdi { get; set; }
    [Required]
    [DisplayName("Üst Birim")]
    public int? ParentId { get; set; }
    [Required]
    [DisplayName("Birim Amiri")]
    public int? AmirId { get; set; }
    [Required]
    [DisplayName("Sıra")]
    public int? Sira { get; set; }
    [Required]
    [DisplayName("Aktif mi")]
    public bool? Aktif { get; set; }
}
