using System;
using System.Threading.Tasks;

namespace Jenga.BlazorUI.Services.Common
{
    public class ModalService : IModalService
    {
        public event Func<ModalRequest, Task>? OnShow;

        public Task Show(ModalRequest request)
        {
            var handler = OnShow;
            return handler != null ? handler.Invoke(request) : Task.CompletedTask;
        }

        public void ShowConfirmation(string title, string message, Action<bool> onResult)
        {
            var req = new ModalRequest
            {
                Title = title,
                Message = message,
                OnResult = onResult,
                ShowConfirmationButtons = true
            };

            _ = Show(req);
        }
    }
}
