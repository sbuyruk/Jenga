namespace Jenga.Utility.Toast;

public class ToastService : IToastService
{
    public event Action<ToastModel>? OnShow;

    public void ShowToast(string message, ToastType type = ToastType.Info, int duration = 3000)
    {
        OnShow?.Invoke(new ToastModel
        {
            Message = message,
            Type = type,
            Duration = duration
        });
    }
    public void ShowSuccess(string message, int duration = 3000)
    {
        ShowToast(message, ToastType.Success, duration);
    }
    public void ShowError(string message, int duration = 3000)
    {
        ShowToast(message, ToastType.Error, duration);
    }
    public void ShowInfo(string message, int duration = 3000)
    {
        ShowToast(message, ToastType.Info, duration);
    }
    public void ShowWarning(string message, int duration = 3000)
    {
        ShowToast(message, ToastType.Warning, duration);
    }
}
