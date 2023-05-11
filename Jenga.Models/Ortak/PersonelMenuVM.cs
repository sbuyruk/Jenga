using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Models.Ortak
{
    public class PersonelMenuVM
    {
        public PersonelMenu PersonelMenu { get; set; }
        public Personel Personel { get; set; }
        public IEnumerable<SelectListItem>? MenuTanimList { get; set; }
    }
}
