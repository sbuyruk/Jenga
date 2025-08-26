namespace Jenga.Utility.Toast;

public class ToastModel
{
    public Guid Id { get; set; }
    public string Message { get; set; } = "";
    public ToastType Type { get; set; } = ToastType.Info;
    public int Duration { get; set; } = 3000; // ms
}
