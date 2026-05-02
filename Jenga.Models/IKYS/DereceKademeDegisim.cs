using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;

public class DereceKademeDegisim : BaseModel
{
    public int? PersonelId { get; set; }
    [ForeignKey("PersonelId")]
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
