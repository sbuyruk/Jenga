using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Common
{

    [Table("Rol_Table")]
    public class Rol : BaseModel
    {
        public string Adi { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        

        public ICollection<PersonelRol>? PersonelRoller { get; set; }
        public ICollection<RolMenu>? RolMenuleri { get; set; }
    }



}