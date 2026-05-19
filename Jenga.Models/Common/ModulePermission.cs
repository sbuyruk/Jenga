using Jenga.Models.Enums;
using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Common
{
    [Table("Auth_ModulePermission_Table")]
    public class ModulePermission : BaseModel
    {
        public ModuleName Module { get; set; }
        public Operation Operation { get; set; }

        public ICollection<RoleModulePermission>? RoleModulePermissions { get; set; }
    }
}
