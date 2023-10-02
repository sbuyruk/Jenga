using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using Jenga.Models.Sistem;
using Jenga.Utility;

namespace Jenga.Models.MTS
{
    public class MTSKurumGorev :BaseModel
    {
        public int MTSKurumTanimId { get; set; }
        [DisplayName("Kurum")]
        [ForeignKey("MTSKurumTanimId")]
        [ValidateNever]
        public MTSKurumTanim? MTSKurumTanim { get; set; }

        public int MTSGorevTanimId { get; set; }
        [DisplayName("Görev")]
        [ForeignKey("MTSGorevTanimId")]
        [ValidateNever]
        public MTSGorevTanim? MTSGorevTanim { get; set; }

        public int KisiId { get; set; }
        [DisplayName("Kişi")]
        [ForeignKey("KisiId")]
        [ValidateNever]
        public Kisi? Kisi { get; set; }
        [DisplayName("Görev Durumu")]
        [Required(ErrorMessage = "Görev Durumu boş olamaz.")]
        public string? Durum { get; set; } = ProjectConstants.MTSGOREVDURUMU_GOREVDE;
        [DisplayName("Başlama Tarihi")]
        [ValidateNever]
        public DateTime? BaslamaTarihi { get; set; }
        [DisplayName("Ayrılma Tarihi")]
        [ValidateNever]
        public DateTime? AyrilmaTarihi { get; set; } = null;

        [ValidateNever]
        public string? AyrilmaSebebi { get; set; } = ProjectConstants.MTSAYRILMASEBEBI_BOS;
    }
}
