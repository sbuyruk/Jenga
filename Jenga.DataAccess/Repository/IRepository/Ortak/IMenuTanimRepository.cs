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
        string GetMenuAll(int ustMenuId);
        void Update(MenuTanim obj);

    }
}
