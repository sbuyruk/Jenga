using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;


namespace Jenga.Models.MTS
{
    public class AramaGorusmeVM
    {
        public AramaGorusme AramaGorusme { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> GorusmeSekliList { get; set; }
        [ValidateNever]
        public Kisi? GorusulenKisi { get; set; }
        [ValidateNever]
        public string? KisiBilgisi { //custom get method
            get
            {
                return GorusulenKisi?.Adi + " " + GorusulenKisi?.Soyadi;
            } 
            set { }

        }
        [ValidateNever]
        public string? KurumGorev { //custom get method
            get
            {
                var kurum = string.Empty;
                var gorev = string.Empty;
                if (GorusulenKisi!=null)
                {
                    if (GorusulenKisi.MTSKurumGorevs != null && GorusulenKisi.MTSKurumGorevs.Count>0
                        && GorusulenKisi.MTSKurumGorevs[0].MTSKurumTanim != null && GorusulenKisi.MTSKurumGorevs[0].MTSKurumTanim.Adi != null)
                    {
                        kurum = GorusulenKisi.MTSKurumGorevs[0].MTSKurumTanim.Adi;
                    }
                    if (GorusulenKisi.MTSKurumGorevs != null && GorusulenKisi.MTSKurumGorevs.Count > 0
                        && GorusulenKisi.MTSKurumGorevs[0].MTSGorevTanim != null && GorusulenKisi.MTSKurumGorevs[0].MTSGorevTanim.Adi != null)
                    {
                        gorev = GorusulenKisi.MTSKurumGorevs[0].MTSGorevTanim.Adi;
                    } 
                }
                return (kurum + " " + gorev).Trim();// GorusulenKisi?.Kurumu + " / " + GorusulenKisi?.Gorevi;
            } 
            set { }

        }
    }
}
