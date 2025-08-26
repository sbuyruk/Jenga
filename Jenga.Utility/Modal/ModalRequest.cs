using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Utility.Modal
{
    public class ModalRequest
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public Action<bool> OnResult { get; set; }
    }

}
