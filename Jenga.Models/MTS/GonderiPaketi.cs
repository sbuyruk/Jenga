using Jenga.Models.Ortak;
using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.MTS
{
    public class GonderiPaketi : BaseModel
    {
        [DisplayName("Gönderi Etiketi")]
        public string Etiket { get; set; }

        public int DagitimYeriTanimId { get; set; }
        [DisplayName("Dağıtım Yeri")]
        [ForeignKey("DagitimYeriTanimId")]
        [ValidateNever]
        public DagitimYeriTanim DagitimYeriTanim { get; set; }

        [DataType(DataType.DateTime)]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime GondermeTarihi { get; set; } = DateTime.Today;

        [ValidateNever]
        [DisplayName("Gönderme Aracı")]
        public string? GondermeAraci { get; set; }

        [ValidateNever]
        [DisplayName("Gönderi Takip No")]
        public string? GonderiTakipNo { get; set; }

        public int IlId { get; set; }
        [DisplayName("Gideceği İl")]
        [ForeignKey("IlId")]
        [ValidateNever]
        public Il? IlAdi { get; set; }

        public int IlceId { get; set; }
        [DisplayName("Gideceği İlçe")]
        [ForeignKey("IlceId")]
        [ValidateNever]
        public Ilce? IlceAdi { get; set; }

        [ValidateNever]
        [DisplayName("Adres")]
        public string? Adres { get; set; }

    }
}
