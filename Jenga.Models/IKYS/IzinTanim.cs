using Jenga.Models.Sistem;
using System.ComponentModel;

namespace Jenga.Models.IKYS;

public class IzinTanim : BaseModel
{
    [DisplayName("Adı")]
    public string? Adi { get; set; }
}
