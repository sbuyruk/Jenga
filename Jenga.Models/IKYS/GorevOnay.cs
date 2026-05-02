using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;

public class GorevOnay : BaseModel
{
    public int? PersonelId { get; set; }
    [ForeignKey("PersonelId")]
    public Personel? Personel { get; set; }
    [DisplayName("Görevin Sebebi")]
    public string? GorevinSebebi { get; set; }
    [DisplayName("Görevin Yeri")]
    public string? GorevinYeri { get; set; }
    [DisplayName("Başlangıç Tarihi")]
    public DateTime? BaslangicTarihi { get; set; }
    [DisplayName("Bitiş Tarihi")]
    public DateTime? BitisTarihi { get; set; }
    [DisplayName("Süre")]
    public string? Sure { get; set; }
    [DisplayName("Avans")]
    public string? Avans { get; set; }
    [DisplayName("Yevmiye")]
    public string? Yevmiye { get; set; }
    [DisplayName("Para Birimi")]
    public string? ParaBirimi { get; set; }
    [DisplayName("Araç Tahsisi")]
    public bool? AracTahsisi { get; set; }
    [DisplayName("Araç Plakası")]
    public string? AracPlakasi { get; set; }
    [DisplayName("Per. Şube İmza")]
    public int? PerSubeImza { get; set; }
    [DisplayName("Per. Şube Vekil")]
    public bool? PerSubeVekil { get; set; }
    [DisplayName("Onay İmza")]
    public int? OnayImza { get; set; }
    [DisplayName("Onay Makam")]
    public int? OnayMakam { get; set; }
    [DisplayName("Onay Makam Vekil")]
    public bool? OnayMakamVekil { get; set; }
    [DisplayName("GM İmza")]
    public int? GMImza { get; set; }
    [DisplayName("GM Vekil")]
    public bool? GMVekil { get; set; }
    [DisplayName("Ulaşım Aracı")]
    public string? UlasimAraci { get; set; }
    [DisplayName("Seçildi")]
    public bool? Secildi { get; set; }
    [DisplayName("Günlük Yevmiye")]
    public string? GunlukYevmiye { get; set; }
    [DisplayName("Ödendi")]
    public bool? Odendi { get; set; }
}
