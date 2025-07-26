using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.DYS;
public class MalzemeYeriTanim : BaseModel
{
    [Required]
    [DisplayName("Malzeme Yeri Tanımı")]
    public string Adi { get; set; }

}
