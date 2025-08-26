namespace Jenga.Utility.Toast;

public interface IToastService
{
    event Action<ToastModel>? OnShow;

    void ShowToast(string message, ToastType type = ToastType.Info, int duration = 3000);
    void ShowSuccess(string message, int duration = 3000);
    void ShowError(string message, int duration = 3000);
    void ShowInfo(string message, int duration = 3000);
    void ShowWarning(string message, int duration = 3000);
}
