using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Common
{

    [Table("RolMenu_Table")]
    public class RolMenu : BaseModel
    {
        public int RolId { get; set; }
        public int MenuId { get; set; }

        public Rol? Rol { get; set; }
        public MenuItem? Menu { get; set; }
    }
}