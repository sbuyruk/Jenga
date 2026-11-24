using Jenga.Models.Sistem;

namespace Jenga.Models.IKYS;

public sealed class Mahsup : BaseModel
{
    public int PersonelId { get; set; }
    public int IzinHareketId { get; set; }
    public IzinHareket IzinHareket { get; set; } = null!;
    public int IzinTipi { get; set; }
    public int KullanildigiDonemId { get; set; }
    public int MahsupDonemId { get; set; }
    public IzinDonem MahsupDonem { get; set; } = null!;
}
