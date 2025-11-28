using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Common
{
    [Table("Role_Table")]
    public class Role : BaseModel
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<PersonelRole>? PersonelRoles { get; set; }
        public ICollection<RoleMenu>? RoleMenus { get; set; }
    }
}