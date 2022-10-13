using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.MTS;
public class KaynakTanim
{
    [Key]
    public int Id { get; set; }
    [Required]
    [DisplayName("Anı Objesi Kaynağı")]
    public string Adi { get; set; }
    [DisplayName("Açıklama")]
    public string? Aciklama { get; set; }
    public string? Olusturan { get; set; }
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    public string? Degistiren { get; set; }
    public DateTime DegistirmeTarihi { get; set; } = DateTime.Now;
}
