using Jenga.Models.Sistem;
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
    [DisplayName("Görev Kisaltmasi")]
    public string? KisaAdi { get; set; }
    [DisplayName("Personel")]
    public int? PersonelId { get; set; }
    [DisplayName("Vekil mi")]
    public bool? Vekil { get; set; }
    [DisplayName("Aktif")]
    public bool? Aktif { get; set; }
}
