namespace Jenga.Utility.Modal
{
    public interface IModalService
    {
        event Func<ModalRequest, Task> OnShow;
        void ShowConfirmation(string title, string message, Action<bool> onResult);
    }

}
