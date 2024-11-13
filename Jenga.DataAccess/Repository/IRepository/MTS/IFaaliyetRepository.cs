using Jenga.Models.MTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.IRepository.MTS
{
    public interface IFaaliyetRepository : IRepository<Faaliyet>
    {
        IEnumerable<Faaliyet> IncludeIt(DateTime? baslangicTarihi);

    }
}
