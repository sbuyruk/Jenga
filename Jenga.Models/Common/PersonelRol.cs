using Jenga.Models.IKYS;
using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Common
{

    [Table("PersonelRol_Table")]
    public class PersonelRol : BaseModel
    {
        public int PersonelId { get; set; }
        public int RolId { get; set; }

        public Rol? Rol { get; set; }
        public Personel? Personel { get; set; }
    }



}