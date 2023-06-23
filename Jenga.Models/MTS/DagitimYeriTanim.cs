using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.MTS;
public class DagitimYeriTanim : BaseModel
{
    [Required]
    [DisplayName("Dağıtım Yeri")]
    public string Adi { get; set; }
}
