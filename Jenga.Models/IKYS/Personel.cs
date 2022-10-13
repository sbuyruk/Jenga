using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Identity;
using Jenga.Models.MTS;
using System.ComponentModel;

namespace Jenga.Models.IKYS
{
    [Serializable]
    public class Personel : IdentityUser 
    {
        public Personel()
        {

        }

        public int Id { get; set; }
        public int PerId { get; set; }
        public int SicilNo { get; set; }
        public string Adi { get; set; }
        public string Soyadi { get; set; }
        public int Tahsili { get; set; }
        public string KullaniciAdi { get; set; }
        public int Asker_sivil { get; set; }
 
    }
}
