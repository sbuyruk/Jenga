using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;

public class IzinTalep : BaseModel
{
    public enum OnayDurumuTipi
    {
        IslemBekliyor = 0,
        KontrolEdildi = 1,
        KayitlaraIslendi = 2,
        Reddedildi = 3,
        IptalEdildi = 4,
        DilekceBekleniyor = 5
    }

    public int? PersonelId { get; set; }
    [ForeignKey("PersonelId")]
    public Personel? Personel { get; set; }
    [DisplayName("İzin Tipi")]
    public int? IzinTipi { get; set; }
    [DisplayName("Başlangıç Tarihi")]
    public DateTime? BaslangicTarihi { get; set; }
    [DisplayName("Bitiş Tarihi")]
    public DateTime? BitisTarihi { get; set; }
    [DisplayName("Süre")]
    public string? Sure { get; set; }
    [DisplayName("Birim")]
    public string? Birim { get; set; }
    [DisplayName("Vekil İmza")]
    public int? VekilImza { get; set; }
    [DisplayName("Amir İmza")]
    public int? AmirImza { get; set; }
    [DisplayName("Onay İmza")]
    public int? OnayImza { get; set; }
    [DisplayName("Adres")]
    public string? Adres { get; set; }
    [DisplayName("Aktif")]
    public bool? Aktif { get; set; }
    [DisplayName("İzin Dönem Id")]
    public int? IzinDonemId { get; set; }
    [ForeignKey("IzinDonemId")]
    public IzinDonem? IzinDonem { get; set; }
    [DisplayName("Onay Durumu")]
    public OnayDurumuTipi? OnayDurumu { get; set; }
    [DisplayName("E-Posta Gönder")]
    public bool? EPostaGonder { get; set; }
}
