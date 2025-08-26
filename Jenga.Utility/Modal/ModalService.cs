using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Utility.Modal
{
    public class ModalService : IModalService
    {
        public event Func<ModalRequest, Task> OnShow;

        public void ShowConfirmation(string title, string message, Action<bool> onResult)
        {
            var request = new ModalRequest
            {
                Title = title,
                Message = message,
                OnResult = onResult
            };

            OnShow?.Invoke(request);
        }
    }

}
