using Jenga.Models.MTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.IRepository.MTS
{
    public interface IFaaliyetKatilimRepository : IRepository<FaaliyetKatilim>
    {
        IEnumerable<FaaliyetKatilim> IncludeIt();
        void Update(FaaliyetKatilim obj);

    }
}
