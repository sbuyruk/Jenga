using Jenga.Utility.Error;
using Jenga.Utility.Toast;
using Microsoft.Extensions.Logging;

namespace Jenga.Utility.Logging
{
    public class LogService(IToastService toast, IEnumerable<ILogWriter> writers) : ILogService
    {
        private readonly IToastService _toast = toast;
        private readonly ILogWriter[] _writers = writers.ToArray();
        private readonly IErrorService? _errorService;

        public void LogInfo(string message)
        {
            foreach (var writer in _writers)
                writer.Write(LogLevel.Information, message);

            _toast.ShowInfo(message); // opsiyonel
        }

        public void LogWarning(string message)
        {
            foreach (var writer in _writers)
                writer.Write(LogLevel.Warning, message);

            _toast.ShowWarning(message); // opsiyonel
        }


        public void LogError(string message, Exception? ex = null)
        {
            foreach (var writer in _writers)
                writer.Write(LogLevel.Error, $"{message} {ex?.Message}");

            //_errorService?.RaiseError(message, ErrorSeverity.Error, ex?.ToString());
        }


        public void Log(string message, LogLevel level = LogLevel.Error)
        {
            foreach (var writer in _writers)
                writer.Write(level, message);
        }
    }

}
