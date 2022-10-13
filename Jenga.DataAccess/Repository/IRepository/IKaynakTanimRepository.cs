using Jenga.Models.MTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.IRepository
{
    public interface IKaynakTanimRepository : IRepository<KaynakTanim>
    {
        void Update(KaynakTanim obj);

    }
}
