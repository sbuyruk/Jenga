using System.ComponentModel.DataAnnotations.Schema;
using Jenga.Models.Sistem;

namespace Jenga.Models.IKYS;

[Table("Harcirah_Table")]
public sealed class Harcirah : BaseModel
{
    public int KadroGrupId { get; set; }
    public int SeriId { get; set; }
    public int Sira { get; set; }
    public string? KadroGrubu { get; set; }
    public string Ulke { get; set; } = string.Empty;
    public decimal Miktar { get; set; }
    public string ParaBirimi { get; set; } = string.Empty;
    public DateTime BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
}
