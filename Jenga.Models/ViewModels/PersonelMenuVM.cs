using Jenga.Models.IKYS;
using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Models.ViewModels
{
    public class PersonelMenuVM
    {
        public PersonelMenu PersonelMenu { get; set; }
        public Personel Personel{ get; set; }
        public IEnumerable<SelectListItem>? MenuTanimList { get; set; } 
    }
}
