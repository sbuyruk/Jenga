namespace Jenga.Utility.Error;

public class ErrorModel
{
    public Guid Id { get; set; }
    public string Message { get; set; } = "";
    public string? Detail { get; set; }
    public int Duration { get; set; } = 5000;
    public ErrorSeverity Severity { get; set; } = ErrorSeverity.Error;
}
