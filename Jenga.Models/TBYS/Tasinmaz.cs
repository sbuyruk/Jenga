using System;
using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS
{
    [Table("Tasinmaz_Table")]
    public class Tasinmaz : BaseModel
    {
        [Column("Cinsi")]
        public string? Cinsi { get; set; }

        [Column("Ili")]
        public string? Ili { get; set; }

        [Column("MulkiyetSekli")]
        public string? MulkiyetSekli { get; set; }

        [Column("KiraDurumu")]
        public string? KiraDurumu { get; set; }

        [Column("SorumluBolge")]
        public string? SorumluBolge { get; set; }

        [Column("EdinmeSekli")]
        public string? EdinmeSekli { get; set; }

        [Column("BagisYili")]
        public string? BagisYili { get; set; }

        [Column("EmlakSicilNo")]
        public string? EmlakSicilNo { get; set; }

        [Column("MuhasebeyeKayitliDeger", TypeName = "money")]
        public decimal? MuhasebeyeKayitliDeger { get; set; }

        [Column("TahminiRayicDegeri", TypeName = "money")]
        public decimal? TahminiRayicDegeri { get; set; }

        [Column("SigortaDurumu")]
        public string? SigortaDurumu { get; set; }

        [Column("TapuTarihi")]
        public DateTime? TapuTarihi { get; set; }

        [Column("AdaNo")]
        public string? AdaNo { get; set; }

        [Column("ParselNo")]
        public string? ParselNo { get; set; }

        [Column("PaftaNo")]
        public string? PaftaNo { get; set; }

        [Column("Yuzolcumu")]
        public string? Yuzolcumu { get; set; }

        [Column("ArsaPayi")]
        public string? ArsaPayi { get; set; }

        [Column("VakifHissesi")]
        public string? VakifHissesi { get; set; }

        [Column("YevmiyeNo")]
        public string? YevmiyeNo { get; set; }

        [Column("CiltNo")]
        public string? CiltNo { get; set; }

        [Column("SahifeNo")]
        public string? SahifeNo { get; set; }

        [Column("KullanimSekli")]
        public string? KullanimSekli { get; set; }

        [Column("Ilcesi")]
        public string? Ilcesi { get; set; }

        [Column("Adres")]
        public string? Adres { get; set; }

        [Column("TasinmazFoto")]
        public string? TasinmazFoto { get; set; }

        [Column("TasinmazFoto1")]
        public string? TasinmazFoto1 { get; set; }

        [Column("TasinmazFoto2")]
        public string? TasinmazFoto2 { get; set; }

        [Column("TapuFoto")]
        public string? TapuFoto { get; set; }

        [Column("KrokiFoto")]
        public string? KrokiFoto { get; set; }

        [Column("TahkikatFoto")]
        public string? TahkikatFoto { get; set; }

        [Column("Bagisci")]
        public string? Bagisci { get; set; }

        [Column("KatMulkiyeti_Old")]
        public string? KatMulkiyeti_Old { get; set; }

        [Column("Nitelik")]
        public string? Nitelik { get; set; }

        // Aciklama, OlusturmaTarihi, Olusturan, DegistirmeTarihi, Degistiren and Id are provided by BaseModel

        [Column("EnvantereGirisTarihi")]
        public DateTime? EnvantereGirisTarihi { get; set; }

        [Column("EnvanterdeMi")]
        public int? EnvanterdeMi { get; set; }

        [Column("EnvanterdenCikmaSebebi")]
        public string? EnvanterdenCikmaSebebi { get; set; }

        [Column("EnvanterdenCikmaTarihi")]
        public DateTime? EnvanterdenCikmaTarihi { get; set; }

        [Column("EnvanterdenCikmaBedeli", TypeName = "money")]
        public decimal? EnvanterdenCikmaBedeli { get; set; }

        [Column("BagisciId")]
        public int? BagisciId { get; set; }

        [Column("BulunduguKat")]
        public string? BulunduguKat { get; set; }

        [Column("Mahalle")]
        public string? Mahalle { get; set; }

        [Column("Koy")]
        public string? Koy { get; set; }

        [Column("Cadde")]
        public string? Cadde { get; set; }

        [Column("Sokak")]
        public string? Sokak { get; set; }

        [Column("BagimsizBolumNo")]
        public string? BagimsizBolumNo { get; set; }

        [Column("Mevki")]
        public string? Mevki { get; set; }

        [Column("TamHisse")]
        public string? TamHisse { get; set; }

        [Column("HisseMiktariPay")]
        public string? HisseMiktariPay { get; set; }

        [Column("HisseMiktariPayda")]
        public string? HisseMiktariPayda { get; set; }

        [Column("ToplamKatSayisi")]
        public string? ToplamKatSayisi { get; set; }

        [Column("Metrekare_Old")]
        public string? Metrekare_Old { get; set; }

        [Column("TapuTasinmazNo")]
        public string? TapuTasinmazNo { get; set; }

        [Column("InsaYili")]
        public string? InsaYili { get; set; }

        [Column("KirayaUygunluk")]
        public string? KirayaUygunluk { get; set; }

        public const string KirayaUygunDeger = "Kiraya Uygun";
        public const string TamMulkiyetDeger = "TM";
        public const string CiplakMulkiyetDeger = "ÇM";

        [NotMapped]
        public bool KirayaUygunMu =>
            !string.IsNullOrEmpty(KirayaUygunluk) &&
            KirayaUygunluk.Equals(KirayaUygunDeger, StringComparison.OrdinalIgnoreCase);

        [NotMapped]
        public bool TamMulkiyetMi =>
            !string.IsNullOrEmpty(MulkiyetSekli) &&
            MulkiyetSekli.Equals(TamMulkiyetDeger, StringComparison.OrdinalIgnoreCase);

        [NotMapped]
        public bool CiplakMulkiyetMi =>
            !string.IsNullOrEmpty(MulkiyetSekli) &&
            MulkiyetSekli.Equals(CiplakMulkiyetDeger, StringComparison.OrdinalIgnoreCase);

        [Column("TasinmazFoto3")]
        public string? TasinmazFoto3 { get; set; }

        [Column("TasinmazFoto4")]
        public string? TasinmazFoto4 { get; set; }

        [Column("IlId")]
        public int? IlId { get; set; }

        [Column("IlceId")]
        public int? IlceId { get; set; }

        [Column("Metrekare", TypeName = "decimal(18,4)")]
        public decimal? Metrekare { get; set; }

        [Column("ProjeM2")]
        public string? ProjeM2 { get; set; }

        [Column("Giris")]
        public string? Giris { get; set; }

        [Column("Blok")]
        public string? Blok { get; set; }

        [Column("ZeminTipi")]
        public string? ZeminTipi { get; set; }

        [Column("KatMulkiyeti")]
        public bool? KatMulkiyeti { get; set; }

        [Column("KatIrtifaki")]
        public bool? KatIrtifaki { get; set; }

        [Column("AltBolum")]
        public bool? AltBolum { get; set; }

        [Column("ToplamMetrekare", TypeName = "decimal(18,4)")]
        public decimal? ToplamMetrekare { get; set; }

        [Column("ZeminHisse")]
        public string? ZeminHisse { get; set; }

        [Column("BBBrutAlan")]
        public decimal? BBBrutAlan { get; set; }

        [Column("BBNetAlan")]
        public decimal? BBNetAlan { get; set; }

        [Column("TapuIslemTarihi")]
        public string? TapuIslemTarihi { get; set; }

        [Column("BBNitelik")]
        public string? BBNitelik { get; set; }

        [Column("AnaTasinmazNitelik")]
        public string? AnaTasinmazNitelik { get; set; }

        [Column("BagimsizBolumSayisi")]
        public int? BagimsizBolumSayisi { get; set; }

        [Column("MalikSayisi")]
        public int? MalikSayisi { get; set; }

        [Column("EmlakBeyanDegeri", TypeName = "money")]
        public decimal? EmlakBeyanDegeri { get; set; }

        [Column("YaklasikPiyasaDegeri", TypeName = "money")]
        public decimal? YaklasikPiyasaDegeri { get; set; }

            }
        }
