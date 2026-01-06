using System;
using System.Threading.Tasks;

namespace Jenga.BlazorUI.Services.Common
{
    public interface IModalService
    {
        // Single-subscriber show event (nullable to reflect no subscriber)
        event Func<ModalRequest, Task>? OnShow;

        // Register a single handler (replaces previous). Use Unregister to remove.
        void Register(Func<ModalRequest, Task> onShow);
        void Unregister(Func<ModalRequest, Task> onShow);

        Task Show(ModalRequest request);
        void ShowConfirmation(string title, string message, Action<bool> onResult);
    }
}
