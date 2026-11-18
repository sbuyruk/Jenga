using System;
using System.Threading.Tasks;

namespace Jenga.BlazorUI.Services.Common
{
    public interface IModalService
    {
        event Func<ModalRequest, Task>? OnShow;

        Task Show(ModalRequest request);
        void ShowConfirmation(string title, string message, Action<bool> onResult);
    }
}
