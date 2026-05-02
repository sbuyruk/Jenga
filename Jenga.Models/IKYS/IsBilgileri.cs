using Jenga.Models.Enums;
using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;
public class IsBilgileri : BaseModel
{
    [Required]
    public int PersonelId { get; set; }
    [DisplayName("Personel")]
    [ForeignKey("PersonelId")]
    public Personel Personel { get; set; }

    //[Required]
    public int? UnvanId { get; set; }
    [DisplayName("Ünvan")]
    [ForeignKey("UnvanId")]
    public UnvanTanim? UnvanTanim { get; set; }
    //[Required]
    public int? GorevId { get; set; }
    [DisplayName("Görev")]
    [ForeignKey("GorevId")]
    public GorevTanim? GorevTanim { get; set; }
    //[Required]
    public int? BirimId { get; set; }
    [DisplayName("Birim")]
    [ForeignKey("BirimId")]
    public BirimTanim BirimTanim { get; set; }
    //[Required]
    [DisplayName("Baslama Tarihi")]
    public DateTime? BaslamaTar { get; set; }
    [Required]
    [DisplayName("Çal??ma Durumu")]
    public CalismaDurumu? CalismaDurumu { get; set; }
    [DisplayName("Ayrilma Tarihi")]
    public DateTime? AyrilmaTar { get; set; }
    [DisplayName("Ayrilma Sebebi")]
    public string? AyrilmaSebebi { get; set; }
    //[Required]
    [DisplayName("SGK Sicil No")]
    public string? SGKSicilNo { get; set; }
    [DisplayName("SGK Baslama Tarihi")]
    public DateTime? SGKBasTar { get; set; }
    [DisplayName("Vakif Öncesi Prim GünSayisi")]
    public int VakifOncesiPrimGunSayisi { get; set; }
    [DisplayName("Emeklilik Tarihi")]
    public DateTime? EmeklilikTarihi { get; set; }
    //[Required]
    [DisplayName("Izin Dönemi Baslama Tarihi")]
    public DateTime? IzinDonemiBasTar { get; set; }
    [DisplayName("Protokol Sira No")]
    public short? ProtokolSiraNo { get; set; }
}
