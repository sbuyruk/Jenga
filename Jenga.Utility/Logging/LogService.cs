using Microsoft.Extensions.Logging;

namespace Jenga.Utility.Logging
{
    public class LogService(IEnumerable<ILogWriter> writers) : ILogService
    {
        private readonly ILogWriter[] _writers = writers.ToArray();

        public void LogInfo(string message)
        {
            WriteAll(LogLevel.Information, message);
        }

        public void LogWarning(string message)
        {
            WriteAll(LogLevel.Warning, message);
        }

        public void LogError(string message, Exception? ex = null)
        {
            var formatted = ex is null ? message : $"{message} | {ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}";
            WriteAll(LogLevel.Error, formatted);
        }

        public void Log(string message, LogLevel level = LogLevel.Error)
        {
            WriteAll(level, message);
        }

        public void LogException(Exception ex, string? source = null, string? message = null)
        {
            var src = string.IsNullOrWhiteSpace(source) ? "App" : source;
            var msg = string.IsNullOrWhiteSpace(message) ? ex.Message : message;
            var formatted = $"[{src}] {msg} | {ex.GetType().FullName}: {ex.Message}{Environment.NewLine}{ex}";
            WriteAll(LogLevel.Error, formatted);
        }

        private void WriteAll(LogLevel level, string message)
        {
            foreach (var writer in _writers)
            {
                try
                {
                    writer.Write(level, message);
                }
                catch
                {
                    // Log writer'ların kendi hatası uygulamayı çökertmemeli.
                }
            }
        }
    }
}
