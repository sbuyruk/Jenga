namespace Jenga.Utility.Error;

public interface IErrorService
{
    event Action<ErrorModel>? OnError;

    void RaiseError(string message, ErrorSeverity severity = ErrorSeverity.Error, string? detail = null, int duration = 5000);
}
