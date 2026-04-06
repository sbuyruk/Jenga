using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS;

[Table("YasalFaiz_Table")]
public class YasalFaiz : BaseModel
{
    [Column("Yil")]
    public int? Yil { get; set; }

    [Column("Ay")]
    public int? Ay { get; set; }

    [Column("AyAdi")]
    public string? AyAdi { get; set; }

    [Column("FaizOrani", TypeName = "decimal(7,4)")]
    public decimal? FaizOrani { get; set; }

    [Column("Aciklama")]
    public new string? Aciklama { get; set; }

    [Column("OlusturmaTarihi", TypeName = "datetime")]
    public new DateTime? OlusturmaTarihi { get; set; }

    [Column("Olusturan")]
    public new string? Olusturan { get; set; }

    [Column("DegistirmeTarihi", TypeName = "datetime")]
    public new DateTime? DegistirmeTarihi { get; set; }

    [Column("Degistiren")]
    public new string? Degistiren { get; set; }

    [Column("Tufe", TypeName = "decimal(7,4)")]
    public decimal? Tufe { get; set; }

    [Column("Ufe", TypeName = "decimal(7,4)")]
    public decimal? Ufe { get; set; }
}