using Jenga.Models.MTS;
using Jenga.Models.Ortak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.IRepository.Ortak
{
    public interface IMenuTanimRepository : IRepository<MenuTanim>
    {
        string GetMenuJson(int ustMenuId);
        void Update(MenuTanim obj);
        public List<MenuTanim> GetMenusByIds(IEnumerable<int> menuTanimIds);
        public List<MenuTanimVM> GetSubMenuMenuListByParentId(int? parentId);

    }
}
