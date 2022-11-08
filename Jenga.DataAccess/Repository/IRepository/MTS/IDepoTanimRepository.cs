using Jenga.Models.MTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.IRepository.MTS
{
    public interface IDepoTanimRepository : IRepository<DepoTanim>
    {
        void Update(DepoTanim obj);

    }
}
