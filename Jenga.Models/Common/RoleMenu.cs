using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Common
{

    [Table("RoleMenu_Table")]
    public class RoleMenu : BaseModel
    {
        public int RoleId { get; set; }
        public int MenuId { get; set; }

        public Role? Role { get; set; }
        public MenuItem? Menu { get; set; }
    }
}