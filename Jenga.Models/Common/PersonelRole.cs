using Jenga.Models.IKYS;
using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Common
{

    [Table("PersonelRole_Table")]
    public class PersonelRole : BaseModel
    {
        public int PersonelId { get; set; }
        public int RoleId { get; set; }

        public Role? Role { get; set; }
        public Personel? Personel { get; set; }
    }



}