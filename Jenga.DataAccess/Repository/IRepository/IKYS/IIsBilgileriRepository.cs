using Jenga.Models.IKYS;
using Jenga.Models.Ortak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.IRepository.IKYS
{
    public interface IIsBilgileriRepository : IRepository<IsBilgileri>
    {
        void Update(IsBilgileri obj);

    }
}
