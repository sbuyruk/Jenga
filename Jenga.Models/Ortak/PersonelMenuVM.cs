using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Models.Ortak
{
    public class PersonelMenuVM
    {
        public PersonelMenu PersonelMenu { get; set; }
        public Personel Personel { get; set; }
        public IEnumerable<SelectListItem>? MenuTanimSelecList { get; set; }
        public IEnumerable<MenuTanimVM>? MenuTanimList { get; set; }
        public IEnumerable<MenuTanim>? SelectedMenuTanimList { get; set; }
    }
}
