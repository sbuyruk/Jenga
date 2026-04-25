using Jenga.Models.Sistem;
using System.ComponentModel;

namespace Jenga.Models.IKYS;

public class TahsilTanim : BaseModel
{
    [DisplayName("Tahsil Durumu")]
    public string? TahsilDurumu { get; set; }
}
