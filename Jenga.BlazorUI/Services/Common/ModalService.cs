using System;
using System.Threading.Tasks;

namespace Jenga.BlazorUI.Services.Common
{
    // Simple modal service that keeps a single active subscriber to avoid duplicate calls
    public class ModalService : IModalService
    {
        // single subscriber event (nullable)
        public event Func<ModalRequest, Task>? OnShow;

        // Register a single handler (replaces previous). Use Unregister to remove.
        public void Register(Func<ModalRequest, Task> onShow)
        {
            OnShow = onShow;
        }

        public void Unregister(Func<ModalRequest, Task> onShow)
        {
            if (OnShow == onShow)
                OnShow = null;
        }

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

            // Fire the single registered handler (if any)
            _ = Show(req);
        }
    }
}