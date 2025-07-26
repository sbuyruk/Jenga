using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.DYS;
public class MarkaTanim : BaseModel
{
    [Required]
    [DisplayName("Marka Tanımı")]
    public string Adi { get; set; }
    public bool Aktif { get; set; }

}
