using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.NBYS
{
    [Table("BankaTanim_Table")]
    public class BankaTanim : BaseModel
    {
        [Column("Banka")]
        [DisplayName("Banka")]
        public string? Banka { get; set; }

        //[Column("BankaGrup")]
        //[DisplayName("Banka Grup")]
        //public string? BankaGrup { get; set; }

        [Column("BankaGrup2")]
        [DisplayName("Banka Grup 2")]
        public string? BankaGrup2 { get; set; }

        [Column("BankaGrup3")]
        [DisplayName("Banka Grup")]
        public string? BankaGrup3 { get; set; }

        [Column("HesapKodu")]
        [DisplayName("Hesap Kodu")]
        public string? HesapKodu { get; set; }

        [Column("HesapAdi")]
        [DisplayName("Hesap Adı")]
        public string? HesapAdi { get; set; }
    }
}
