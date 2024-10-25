using Jenga.Models.DYS;
using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.IRepository.DYS
{
    public interface IMalzemeDagilimRepository : IRepository<MalzemeDagilim>
    {
        IEnumerable<MalzemeDagilim> GetMalzemeDagilimWithDetails();

    }
}
