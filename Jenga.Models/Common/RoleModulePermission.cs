using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Common
{
    [Table("Auth_RoleModulePermission_Table")]
    public class RoleModulePermission : BaseModel
    {
        public int RoleId { get; set; }
        public int ModulePermissionId { get; set; }

        public Role? Role { get; set; }
        public ModulePermission? ModulePermission { get; set; }
    }
}
