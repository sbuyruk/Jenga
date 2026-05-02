using Jenga.Models.Common;
using Jenga.Models.IKYS;

namespace Jenga.Models.Ortak
{
    public class GorevTanimVM
    {
        public GorevTanim GorevTanim { get; set; }
        public IEnumerable<ListObj> TanimList { get; set; }
    }
}
