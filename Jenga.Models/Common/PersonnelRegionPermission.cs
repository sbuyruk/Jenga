using Jenga.Models.IKYS;
using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Common
{
    [Table("Auth_PersonnelRegionPermission_Table")]
    public class PersonnelRegionPermission : BaseModel
    {
        public int PersonnelId { get; set; }
        public int RegionId { get; set; }

        public Personel? Personnel { get; set; }
        public Bolge? Region { get; set; }
    }
}
