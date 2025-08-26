using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Utility.Modal
{
    public interface IModalService
    {
        event Func<ModalRequest, Task> OnShow;
        void ShowConfirmation(string title, string message, Action<bool> onResult);
    }

}
