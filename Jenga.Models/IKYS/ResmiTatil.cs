using Jenga.Models.Sistem;
using System.ComponentModel;

namespace Jenga.Models.IKYS;
public class ResmiTatil : BaseModel
{
    [DisplayName("Gün")]
    public int Gun { get; set; }
    [DisplayName("Ay")]
    public int Ay { get; set; }
    [DisplayName("Yil")]
    public int Yil { get; set; }
    [DisplayName("Tatil")]
    public string Tatil { get; set; }
    [DisplayName("Baslama Tarihi")]
    public DateTime BaslamaTarihi { get; set; }
    [DisplayName("Bitis Tarihi")]
    public DateTime BitisTarihi { get; set; }
    [DisplayName("Ilan Tarihi")]
    public DateTime? IlanTarihi { get; set; }
    [DisplayName("Iptal Tarihi")]
    public DateTime? IptalTarihi { get; set; }

}
